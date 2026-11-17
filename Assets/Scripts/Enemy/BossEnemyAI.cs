using UnityEngine;
using UnityEngine.AI;

namespace ArenaBreak.Enemy
{
    // 거리에 따라 세 패턴 중 하나를 고른다: 가까우면 내려찍기, 멀면 점프 착지, 중간이면 부채꼴 탄막.
    // 패턴이 끝나면 항상 Recover를 거쳐, 패턴이 연달아 나와 피할 틈이 없어지는 것을 막는다
    public class BossEnemyAI : EnemyBase
    {
        private enum State
        {
            Chase,
            SlamWindup,
            Jump,
            Recover
        }

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 2.5f;
        [SerializeField] private float _turnSpeed = 180f;
        [SerializeField] private float _recoverDuration = 1.2f;

        [Header("Slam (근접 내려찍기)")]
        [SerializeField] private float _slamTriggerRange = 4f;
        // 예비 동작 동안 플레이어가 반경 밖으로 피할 수 있어야 하므로 판정은 예비 동작이 끝난 순간에 한다
        [SerializeField] private float _slamWindup = 0.8f;
        [SerializeField] private float _slamRadius = 5f;
        [SerializeField] private int _slamDamage = 15;
        [SerializeField] private float _slamCooldown = 4f;

        [Header("Volley (부채꼴 탄막)")]
        [SerializeField] private float _volleyMinRange = 6f;
        [SerializeField] private float _volleyMaxRange = 20f;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private EnemyProjectile _projectilePrefab;
        [SerializeField, Min(1)] private int _volleyCount = 7;
        [SerializeField] private float _volleyAngle = 60f;
        [SerializeField] private int _projectileDamage = 4;
        [SerializeField] private float _volleyCooldown = 3f;

        [Header("Jump (점프 착지 충격파)")]
        [SerializeField] private float _jumpMinRange = 12f;
        [SerializeField] private float _jumpDuration = 1f;
        [SerializeField] private float _jumpHeight = 6f;
        [SerializeField] private float _jumpLandingRadius = 6f;
        [SerializeField] private int _jumpDamage = 12;
        [SerializeField] private float _jumpCooldown = 8f;
        // 커진 몸이 플레이어 바로 위에 내려앉으면 CharacterController를 밀어내므로, 이만큼 앞에서 착지한다.
        // 보스 반경(0.5 × 스케일 2.5)과 플레이어 반경을 합친 값보다 크게 둘 것
        [SerializeField] private float _landingStandOff = 2.5f;
        [SerializeField] private float _navSampleRadius = 3f;

        [Header("Visual")]
        [SerializeField] private ShockwaveVisual _shockwaveVisual;

        private State _state;
        private float _stateEndTime;
        private float _nextSlamTime;
        private float _nextVolleyTime;
        private float _nextJumpTime;
        private Vector3 _jumpStart;
        private Vector3 _jumpTarget;
        private float _jumpStartTime;

        protected override void Awake()
        {
            base.Awake();
            // 예비 동작·탄막 중에도 플레이어를 봐야 하므로 회전은 직접 제어한다
            Agent.updateRotation = false;

            if (_muzzle == null)
            {
                _muzzle = transform;
            }
        }

        protected override void ResetState()
        {
            _state = State.Chase;
            _nextSlamTime = 0f;
            _nextVolleyTime = 0f;
            // 등장하자마자 플레이어 머리 위로 뛰어들면 반응할 틈이 없으므로 첫 점프는 쿨다운만큼 미룬다
            _nextJumpTime = Time.time + _jumpCooldown;
            Agent.speed = _walkSpeed;
            // 점프 도중 풀에 회수됐다면 위치 동기화가 꺼진 채로 남으므로 되돌린다
            Agent.updatePosition = true;
        }

        protected override void TickAlive(float sqrDistanceToPlayer)
        {
            if (_state != State.Jump)
            {
                FacePlayer();
            }

            switch (_state)
            {
                case State.Chase:
                    TickChase(sqrDistanceToPlayer);
                    break;

                case State.SlamWindup:
                    if (Time.time >= _stateEndTime)
                    {
                        ReleaseShockwave(sqrDistanceToPlayer, _slamRadius, _slamDamage);
                        EnterRecover();
                    }
                    break;

                case State.Jump:
                    TickJump();
                    break;

                case State.Recover:
                    if (Time.time >= _stateEndTime)
                    {
                        EnterChase();
                    }
                    break;
            }
        }

        private void TickChase(float sqrDistanceToPlayer)
        {
            bool inSlamRange = sqrDistanceToPlayer <= _slamTriggerRange * _slamTriggerRange;
            bool inJumpRange = sqrDistanceToPlayer >= _jumpMinRange * _jumpMinRange;
            bool inVolleyRange = sqrDistanceToPlayer >= _volleyMinRange * _volleyMinRange
                && sqrDistanceToPlayer <= _volleyMaxRange * _volleyMaxRange;

            if (inSlamRange && Time.time >= _nextSlamTime)
            {
                EnterSlamWindup();
            }
            else if (inJumpRange && Time.time >= _nextJumpTime && TryEnterJump())
            {
                return;
            }
            else if (inVolleyRange && Time.time >= _nextVolleyTime && _projectilePrefab != null)
            {
                FireVolley();
                EnterRecover();
            }
            else
            {
                Agent.isStopped = false;
                Agent.SetDestination(Player.position);
            }
        }

        private void EnterChase()
        {
            _state = State.Chase;
            Agent.isStopped = false;
        }

        private void EnterRecover()
        {
            _state = State.Recover;
            _stateEndTime = Time.time + _recoverDuration;
            StopMoving();
        }

        private void EnterSlamWindup()
        {
            _state = State.SlamWindup;
            _stateEndTime = Time.time + _slamWindup;
            _nextSlamTime = Time.time + _slamCooldown;
            StopMoving();
        }

        // 착지 지점은 도약 순간의 플레이어 위치 기준으로 고정한다 — 체공 중에 옆으로 빠지면 피할 수 있게
        private bool TryEnterJump()
        {
            Vector3 toPlayer = Player.position - transform.position;
            toPlayer.y = 0f;
            Vector3 landing = Player.position - toPlayer.normalized * _landingStandOff;

            if (!NavMesh.SamplePosition(landing, out NavMeshHit navHit, _navSampleRadius, NavMesh.AllAreas))
            {
                return false;
            }

            _state = State.Jump;
            _nextJumpTime = Time.time + _jumpCooldown;
            _jumpStart = transform.position;
            _jumpTarget = navHit.position;
            _jumpStartTime = Time.time;

            StopMoving();
            // 체공 중 transform을 직접 움직이려면 에이전트가 위치를 덮어쓰지 않게 해야 한다
            Agent.updatePosition = false;
            return true;
        }

        private void TickJump()
        {
            float progress = GetJumpProgress();
            Vector3 position = Vector3.Lerp(_jumpStart, _jumpTarget, progress);
            position.y += Mathf.Sin(progress * Mathf.PI) * _jumpHeight;
            transform.position = position;

            if (progress < 1f)
            {
                return;
            }

            LandAt(_jumpTarget);

            float sqrDistanceToPlayer = (Player.position - transform.position).sqrMagnitude;
            ReleaseShockwave(sqrDistanceToPlayer, _jumpLandingRadius, _jumpDamage);
            EnterRecover();
        }

        // 체공 중에 죽으면 _isDead 때문에 TickJump가 더 이상 돌지 않아 포물선 위에 멈춘 채 사라진다.
        // 바로 아래 지면으로 떨어뜨리고, 그 지점이 NavMesh 밖이면 원래 착지 지점으로 내린다
        protected override void OnDied()
        {
            if (_state != State.Jump)
            {
                return;
            }

            Vector3 groundBelow = Vector3.Lerp(_jumpStart, _jumpTarget, GetJumpProgress());
            bool onNavMesh = NavMesh.SamplePosition(groundBelow, out NavMeshHit navHit, _navSampleRadius, NavMesh.AllAreas);
            LandAt(onNavMesh ? navHit.position : _jumpTarget);
        }

        private float GetJumpProgress()
        {
            return Mathf.Clamp01((Time.time - _jumpStartTime) / _jumpDuration);
        }

        // Warp로 에이전트 내부 위치를 착지 지점에 맞춘 뒤에 동기화를 다시 켜야 원래 자리로 순간이동하지 않는다
        private void LandAt(Vector3 position)
        {
            Agent.Warp(position);
            Agent.updatePosition = true;
        }

        // 풀링 대상: 탄막 한 번에 _volleyCount개를 생성·파괴한다. 지금은 RangedEnemyAI와 같이 단순 생성/파괴로 둔다
        private void FireVolley()
        {
            _nextVolleyTime = Time.time + _volleyCooldown;

            Vector3 direction = (Player.position - _muzzle.position).normalized;
            Quaternion aim = Quaternion.LookRotation(direction);
            float step = _volleyCount > 1 ? _volleyAngle / (_volleyCount - 1) : 0f;
            float startAngle = _volleyCount > 1 ? -_volleyAngle * 0.5f : 0f;

            for (int i = 0; i < _volleyCount; i++)
            {
                Quaternion spread = Quaternion.Euler(0f, startAngle + step * i, 0f);
                EnemyProjectile projectile = Instantiate(_projectilePrefab, _muzzle.position, spread * aim);
                projectile.Launch(_projectileDamage);
            }
        }

        // 대상은 플레이어 하나뿐이라 OverlapSphere 대신 거리 비교로 판정한다
        private void ReleaseShockwave(float sqrDistanceToPlayer, float radius, int damage)
        {
            if (sqrDistanceToPlayer <= radius * radius && PlayerDamageable != null)
            {
                PlayerDamageable.TakeDamage(damage);
            }

            if (_shockwaveVisual != null)
            {
                // 원판은 보스의 자식이라 보스를 키운 배율만큼 같이 커진다. 판정 반경과 눈에 보이는 크기를 맞추려고 나눈다
                _shockwaveVisual.Play(radius / transform.lossyScale.x);
            }
        }

        private void FacePlayer()
        {
            Vector3 toPlayer = Player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < Mathf.Epsilon)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
        }
    }
}
