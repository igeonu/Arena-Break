using System;
using System.Collections;
using ArenaBreak.Core;
using UnityEngine;
using UnityEngine.AI;

namespace ArenaBreak.Enemy
{
    // 스포너와 풀은 적 종류를 몰라도 되도록 이 타입만 다룬다. 종류별 행동은 TickAlive와 ResetState에서 정한다
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _player;

        [Header("Death")]
        [SerializeField] private float _despawnDelay = 1f;

        private Health _health;
        private WaitForSeconds _despawnWait;
        private bool _isDead;

        // 사망 연출이 끝나 회수해도 되는 시점. 풀이 구독해서 비활성 적을 돌려받는다
        public event Action<EnemyBase> DeathFinished;

        protected NavMeshAgent Agent { get; private set; }
        protected Transform Player => _player;
        // 플레이어에게 Health가 아직 없을 수 있으므로 null을 허용한다
        protected IDamageable PlayerDamageable { get; private set; }

        protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
            _despawnWait = new WaitForSeconds(_despawnDelay);
            CachePlayerDamageable();
        }

        // 풀에서 재사용될 때 이전 생의 상태가 남지 않게 활성화 시점마다 되돌린다
        protected virtual void OnEnable()
        {
            _isDead = false;
            ResetState();
            _health.Died += HandleDied;
        }

        protected virtual void OnDisable()
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
            if (_isDead || _player == null || !Agent.isOnNavMesh)
            {
                return;
            }

            // 매 프레임 sqrt를 피하려고 제곱 거리로 넘긴다
            TickAlive((_player.position - transform.position).sqrMagnitude);
        }

        // 에이전트 속도·가속도처럼 NavMesh 위가 아니어도 되는 값만 여기서 되돌릴 것.
        // isStopped·SetDestination은 NavMesh 위에서만 호출할 수 있으므로 TickAlive에서 처리한다
        protected abstract void ResetState();

        protected abstract void TickAlive(float sqrDistanceToPlayer);

        protected virtual void OnDied()
        {
        }

        protected void StopMoving()
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        private void HandleDied()
        {
            _isDead = true;

            if (Agent.isOnNavMesh)
            {
                StopMoving();
            }

            OnDied();
            StartCoroutine(DespawnAfterDelay());
        }

        private IEnumerator DespawnAfterDelay()
        {
            yield return _despawnWait;

            DeathFinished?.Invoke(this);

            // 풀이 회수하면서 이미 껐을 수 있다. 풀 없이 씬에 직접 둔 적도 사라지게 하려고 남아 있으면 끈다
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void CachePlayerDamageable()
        {
            PlayerDamageable = null;
            if (_player != null && _player.TryGetComponent(out IDamageable damageable))
            {
                PlayerDamageable = damageable;
            }
        }
    }
}
