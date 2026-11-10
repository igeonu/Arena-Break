using UnityEngine;
using UnityEngine.AI;

namespace ArenaBreak.Enemy
{
    public class RangedEnemyAI : EnemyBase
    {
        private enum State
        {
            Idle,
            Chase,
            Attack,
            Retreat
        }

        [Header("Ranges")]
        [SerializeField] private float _chaseRange = 60f;
        [SerializeField] private float _attackRange = 15f;
        [SerializeField] private float _retreatRange = 6f;
        // 후퇴 진입(_retreatRange)과 종료 거리를 다르게 두어, 경계에서 후퇴/정지를 매 프레임 오가지 않게 한다
        [SerializeField] private float _safeDistance = 9f;

        [Header("Movement")]
        [SerializeField] private float _turnSpeed = 360f;
        [SerializeField] private float _retreatStep = 4f;
        [SerializeField] private float _navSampleRadius = 2f;

        [Header("Fire")]
        [SerializeField] private Transform _muzzle;
        [SerializeField] private EnemyProjectile _projectilePrefab;
        [SerializeField] private float _fireInterval = 2f;
        [SerializeField] private int _projectileDamage = 2;

        private State _state;
        private float _nextFireTime;

        protected override void Awake()
        {
            base.Awake();
            // 후퇴하면서도 플레이어를 보고 쏴야 하므로 회전은 이동 방향이 아니라 직접 제어한다
            Agent.updateRotation = false;

            // 총구를 연결하지 않았으면 몸 중심에서라도 쏘게 한다
            if (_muzzle == null)
            {
                _muzzle = transform;
            }
        }

        protected override void ResetState()
        {
            _state = State.Idle;
            _nextFireTime = 0f;
        }

        protected override void TickAlive(float sqrDistanceToPlayer)
        {
            FacePlayer();

            bool inChaseRange = sqrDistanceToPlayer <= _chaseRange * _chaseRange;
            bool inAttackRange = sqrDistanceToPlayer <= _attackRange * _attackRange;
            bool tooClose = sqrDistanceToPlayer <= _retreatRange * _retreatRange;
            bool isSafe = sqrDistanceToPlayer >= _safeDistance * _safeDistance;

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
                    else if (tooClose)
                    {
                        EnterRetreat();
                    }
                    else if (inAttackRange)
                    {
                        EnterAttack();
                    }
                    else
                    {
                        Agent.SetDestination(Player.position);
                    }
                    break;

                case State.Attack:
                    if (tooClose)
                    {
                        EnterRetreat();
                    }
                    else if (!inAttackRange)
                    {
                        EnterChase();
                    }
                    else
                    {
                        TryFire();
                    }
                    break;

                case State.Retreat:
                    if (isSafe)
                    {
                        if (inAttackRange)
                        {
                            EnterAttack();
                        }
                        else
                        {
                            EnterChase();
                        }
                    }
                    else
                    {
                        MoveAwayFromPlayer();
                        // 플레이어가 계속 붙어 오면 영영 못 쏘게 되므로 물러나면서도 쏜다
                        TryFire();
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

        private void EnterRetreat()
        {
            _state = State.Retreat;
            Agent.isStopped = false;
            // 이전 추적 경로가 남아 있으면 MoveAwayFromPlayer가 새 목적지를 잡지 않으므로 비운다
            Agent.ResetPath();
        }

        // 매 프레임 경로를 다시 계산하지 않도록, 이전 후퇴 지점에 거의 도착했을 때만 새 지점을 잡는다
        private void MoveAwayFromPlayer()
        {
            if (Agent.pathPending || (Agent.hasPath && Agent.remainingDistance > Agent.stoppingDistance))
            {
                return;
            }

            Vector3 away = transform.position - Player.position;
            away.y = 0f;
            if (away.sqrMagnitude < Mathf.Epsilon)
            {
                away = -transform.forward;
            }

            Vector3 candidate = transform.position + away.normalized * _retreatStep;

            // 벽 너머처럼 NavMesh 밖이면 물러나지 못하고 제자리에서 쏜다
            if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, _navSampleRadius, NavMesh.AllAreas))
            {
                Agent.SetDestination(navHit.position);
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

        private void TryFire()
        {
            if (Time.time < _nextFireTime || _projectilePrefab == null)
            {
                return;
            }

            _nextFireTime = Time.time + _fireInterval;

            // 발사 순간의 플레이어 위치를 겨냥하고 이후엔 방향을 바꾸지 않는다(유도 없음)
            Vector3 direction = (Player.position - _muzzle.position).normalized;
            EnemyProjectile projectile = Instantiate(_projectilePrefab, _muzzle.position, Quaternion.LookRotation(direction));
            projectile.Launch(_projectileDamage);

            Debug.Log($"[RangedEnemyAI] {name} 발사 — 데미지 {_projectileDamage} (대상 IDamageable: {PlayerDamageable != null})");
        }
    }
}
