using UnityEngine;

namespace ArenaBreak.Enemy
{
    // 쫓아가서 근접 공격만 하는 기본 적. 사망·디스폰·풀 반환은 EnemyBase가 처리한다
    public class MeleeEnemyAI : EnemyBase
    {
        private enum State
        {
            Idle,
            Chase,
            Attack
        }

        [Header("Ranges")]
        [SerializeField] private float _chaseRange = 60f;
        [SerializeField] private float _attackRange = 2f;

        [Header("Attack")]
        [SerializeField] private float _attackInterval = 2f;
        [SerializeField] private int _attackDamage = 10;

        private State _state;
        private float _nextAttackTime;

        protected override void ResetState()
        {
            _state = State.Idle;
            _nextAttackTime = 0f;
        }

        protected override void TickAlive(float sqrDistanceToPlayer)
        {
            bool inChaseRange = sqrDistanceToPlayer <= _chaseRange * _chaseRange;
            bool inAttackRange = sqrDistanceToPlayer <= _attackRange * _attackRange;

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
                    else
                    {
                        Agent.SetDestination(Player.position);
                    }
                    break;

                case State.Attack:
                    if (!inAttackRange)
                    {
                        EnterChase();
                    }
                    else if (Time.time >= _nextAttackTime)
                    {
                        PerformAttack();
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

        // 공격 쿨다운은 상태 전환 때 초기화하지 않는다 — 사거리 경계를 오가며 공격 간격을 우회하지 못하게
        private void EnterAttack()
        {
            _state = State.Attack;
            StopMoving();
        }

        private void PerformAttack()
        {
            _nextAttackTime = Time.time + _attackInterval;

            if (PlayerDamageable != null)
            {
                PlayerDamageable.TakeDamage(_attackDamage);
            }
        }
    }
}
