using UnityEngine;

namespace ArenaBreak.Enemy
{
    public class ChargerEnemyAI : EnemyBase
    {
        private enum State
        {
            Idle,
            Chase,
            Dash,
            Attack
        }

        [Header("Ranges")]
        [SerializeField] private float _chaseRange = 60f;
        [SerializeField] private float _attackRange = 2f;

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 2f;

        [Header("Dash (생에 한 번, 끝나면 충격파)")]
        [SerializeField] private float _dashTriggerRange = 8f;
        [SerializeField] private float _dashSpeed = 14f;
        [SerializeField] private float _dashAcceleration = 60f;
        [SerializeField] private float _dashDuration = 0.5f;

        [Header("Shockwave")]
        [SerializeField] private float _shockwaveRadius = 3f;
        [SerializeField] private int _shockwaveDamage = 6;
        [SerializeField] private ShockwaveVisual _shockwaveVisual;

        [Header("Melee")]
        [SerializeField] private float _attackInterval = 2f;
        [SerializeField] private int _attackDamage = 4;

        private State _state;
        private float _defaultAcceleration;
        private float _dashEndTime;
        private float _nextAttackTime;
        // 돌진+충격파는 일회용 스킬이다. 이후 공격 수단은 근접 공격뿐이다
        private bool _dashUsed;

        protected override void Awake()
        {
            base.Awake();
            // 돌진이 끝나면 프리팹에 설정된 가속도로 되돌리기 위해 기억해 둔다
            _defaultAcceleration = Agent.acceleration;
        }

        protected override void ResetState()
        {
            _state = State.Idle;
            _nextAttackTime = 0f;
            _dashUsed = false;
            Agent.speed = _walkSpeed;
            Agent.acceleration = _defaultAcceleration;
        }

        protected override void TickAlive(float sqrDistanceToPlayer)
        {
            bool inChaseRange = sqrDistanceToPlayer <= _chaseRange * _chaseRange;
            bool inAttackRange = sqrDistanceToPlayer <= _attackRange * _attackRange;
            bool inDashRange = sqrDistanceToPlayer <= _dashTriggerRange * _dashTriggerRange;

            switch (_state)
            {
                case State.Idle:
                    if (inChaseRange)
                    {
                        EnterChase();
                    }
                    break;

                case State.Chase:
                    if (!inChaseRange)
                    {
                        EnterIdle();
                    }
                    else if (inAttackRange)
                    {
                        EnterAttack();
                    }
                    else if (inDashRange && !_dashUsed)
                    {
                        EnterDash();
                    }
                    else
                    {
                        Agent.SetDestination(Player.position);
                    }
                    break;

                case State.Dash:
                    // 경로 계산 직후에는 remainingDistance가 아직 0이라 pathPending으로 걸러낸다
                    bool arrived = !Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance;
                    // 플레이어가 돌진 경로로 다가온 경우에도 공격 거리에서 멈춰야 몸이 겹쳐 밀어내지 않는다
                    if (Time.time >= _dashEndTime || arrived || inAttackRange)
                    {
                        EndDash(sqrDistanceToPlayer, inAttackRange);
                    }
                    break;

                case State.Attack:
                    if (!inAttackRange)
                    {
                        EnterChase();
                    }
                    else if (Time.time >= _nextAttackTime)
                    {
                        PerformMelee();
                    }
                    break;
            }
        }

        private void EnterIdle()
        {
            _state = State.Idle;
            StopMoving();
        }

        private void EnterChase()
        {
            _state = State.Chase;
            Agent.isStopped = false;
        }

        private void EnterAttack()
        {
            _state = State.Attack;
            StopMoving();
        }

        // 돌진은 시작 순간의 플레이어 방향으로 고정한다 — 옆으로 피하면 빗나가게 하려고.
        // 목적지를 플레이어 위치 자체로 잡으면 몸이 겹칠 때까지 파고들어 CharacterController를 밀어내므로 공격 거리 앞에서 멈춘다
        private void EnterDash()
        {
            Vector3 toPlayer = Player.position - transform.position;
            toPlayer.y = 0f;
            float dashDistance = Mathf.Max(0f, toPlayer.magnitude - _attackRange);

            _state = State.Dash;
            // 시작하는 순간 소모 처리한다 — 도중에 끊겨도 다시 쓰지 못하게
            _dashUsed = true;
            _dashEndTime = Time.time + _dashDuration;
            Agent.speed = _dashSpeed;
            Agent.acceleration = _dashAcceleration;
            Agent.isStopped = false;
            Agent.SetDestination(transform.position + toPlayer.normalized * dashDistance);
        }

        private void EndDash(float sqrDistanceToPlayer, bool inAttackRange)
        {
            // 속도 상한만 낮추면 남은 돌진 속도가 원래 가속도로 천천히 줄어 수 미터를 미끄러진다. 즉시 멈춘다
            Agent.velocity = Vector3.zero;
            Agent.speed = _walkSpeed;
            Agent.acceleration = _defaultAcceleration;

            ReleaseShockwave(sqrDistanceToPlayer);

            if (inAttackRange)
            {
                EnterAttack();
            }
            else
            {
                EnterChase();
            }
        }

        // 대상은 플레이어 하나뿐이라 OverlapSphere 대신 거리 비교로 판정한다
        private void ReleaseShockwave(float sqrDistanceToPlayer)
        {
            bool hit = sqrDistanceToPlayer <= _shockwaveRadius * _shockwaveRadius;

            if (hit && PlayerDamageable != null)
            {
                PlayerDamageable.TakeDamage(_shockwaveDamage);
            }

            if (_shockwaveVisual != null)
            {
                _shockwaveVisual.Play(_shockwaveRadius);
            }

            // 플레이어에게 Health가 붙기 전까지는 이 로그로만 충격파 발생을 확인한다
            Debug.Log($"[ChargerEnemyAI] {name} 충격파 — 명중 {hit}, 데미지 {_shockwaveDamage} (대상 IDamageable: {PlayerDamageable != null})");
        }

        private void PerformMelee()
        {
            _nextAttackTime = Time.time + _attackInterval;

            if (PlayerDamageable != null)
            {
                PlayerDamageable.TakeDamage(_attackDamage);
            }

            Debug.Log($"[ChargerEnemyAI] {name} 근접 공격 — 데미지 {_attackDamage} (대상 IDamageable: {PlayerDamageable != null})");
        }
    }
}
