using ArenaBreak.Core;
using UnityEngine;
using UnityEngine.AI;

namespace ArenaBreak.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public class EnemyAI : MonoBehaviour
    {
        private enum State
        {
            Idle,
            Chase,
            Attack,
            Dead
        }

        [Header("Target")]
        [SerializeField] private Transform _player;

        [Header("Ranges")]
        [SerializeField] private float _chaseRange = 60f;
        [SerializeField] private float _attackRange = 2f;

        [Header("Attack")]
        [SerializeField] private float _attackInterval = 2f;
        [SerializeField] private int _attackDamage = 10;

        [Header("Death")]
        [SerializeField] private float _destroyDelay = 1f;

        private NavMeshAgent _agent;
        private Health _health;
        // 플레이어에게 Health가 아직 없을 수 있으므로 null을 허용한다. 공격 로그는 이와 무관하게 남긴다
        private IDamageable _playerDamageable;

        private State _state = State.Idle;
        private float _nextAttackTime;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
            CachePlayerDamageable();
        }

        private void OnEnable()
        {
            _health.Died += HandleDied;
        }

        private void OnDisable()
        {
            _health.Died -= HandleDied;
        }

        // 프리팹은 씬의 플레이어를 직렬화로 참조할 수 없으므로, 스폰한 쪽이 생성 직후 넣어준다
        public void SetTarget(Transform player)
        {
            _player = player;
            CachePlayerDamageable();
        }

        private void Update()
        {
            // 플레이어가 없거나 NavMesh 밖에 스폰되면 에이전트 API가 에러를 내므로 아무것도 하지 않는다
            if (_state == State.Dead || _player == null || !_agent.isOnNavMesh)
            {
                return;
            }

            // 매 프레임 sqrt를 피하려고 제곱 거리로 비교한다
            float sqrDistance = (_player.position - transform.position).sqrMagnitude;
            bool inChaseRange = sqrDistance <= _chaseRange * _chaseRange;
            bool inAttackRange = sqrDistance <= _attackRange * _attackRange;

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
                        _agent.SetDestination(_player.position);
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
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        private void EnterChase()
        {
            _state = State.Chase;
            _agent.isStopped = false;
        }

        // 공격 쿨다운(_nextAttackTime)은 상태 전환 때 초기화하지 않는다 — 사거리 경계를 오가며 공격 간격을 우회하지 못하게
        private void EnterAttack()
        {
            _state = State.Attack;
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        private void PerformAttack()
        {
            _nextAttackTime = Time.time + _attackInterval;

            if (_playerDamageable != null)
            {
                _playerDamageable.TakeDamage(_attackDamage);
            }

            // 플레이어에게 Health가 붙기 전까지는 이 로그로만 공격 발생을 확인한다
            Debug.Log($"[EnemyAI] {name} 공격 — 데미지 {_attackDamage} (대상 IDamageable: {_playerDamageable != null})");
        }

        private void HandleDied()
        {
            _state = State.Dead;

            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }

            Destroy(gameObject, _destroyDelay);
        }

        private void CachePlayerDamageable()
        {
            _playerDamageable = null;
            if (_player != null)
            {
                _player.TryGetComponent(out _playerDamageable);
            }
        }
    }
}
