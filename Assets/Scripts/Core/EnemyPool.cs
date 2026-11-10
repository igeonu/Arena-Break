using System;
using System.Collections.Generic;
using ArenaBreak.Enemy;
using UnityEngine;

namespace ArenaBreak.Core
{
    // UnityEngine.Pool.ObjectPool은 Get에 위치를 넘길 수 없어, 꺼낸 뒤 활성 상태에서 옮기면 NavMeshAgent가
    // 이전 사망 위치에서 먼저 켜진다. 비활성 상태에서 위치를 정한 뒤 켜려고 직접 스택으로 관리한다
    public class EnemyPool : IDisposable
    {
        // 웨이브마다 다른 적 프리팹을 쓸 수 있으므로 프리팹별로 따로 보관한다
        private readonly Dictionary<EnemyAI, Stack<EnemyAI>> _inactiveByPrefab = new Dictionary<EnemyAI, Stack<EnemyAI>>();
        private readonly Dictionary<EnemyAI, EnemyAI> _prefabByInstance = new Dictionary<EnemyAI, EnemyAI>();

        public EnemyAI Get(EnemyAI prefab, Vector3 position, Quaternion rotation)
        {
            Stack<EnemyAI> inactive = GetInactiveStack(prefab);

            if (inactive.Count > 0)
            {
                EnemyAI reused = inactive.Pop();
                reused.transform.SetPositionAndRotation(position, rotation);
                // 활성화 순간 Health와 EnemyAI의 OnEnable이 체력과 상태를 초기화한다
                reused.gameObject.SetActive(true);
                return reused;
            }

            EnemyAI created = UnityEngine.Object.Instantiate(prefab, position, rotation);
            // 인스턴스의 수명 동안 한 번만 구독한다. 해제는 Dispose에서 한다
            created.DeathFinished += Release;
            _prefabByInstance.Add(created, prefab);
            return created;
        }

        public void Dispose()
        {
            foreach (EnemyAI instance in _prefabByInstance.Keys)
            {
                // 씬 종료 중에는 적이 먼저 파괴됐을 수 있다
                if (instance != null)
                {
                    instance.DeathFinished -= Release;
                }
            }

            _prefabByInstance.Clear();
            _inactiveByPrefab.Clear();
        }

        private void Release(EnemyAI instance)
        {
            instance.gameObject.SetActive(false);
            GetInactiveStack(_prefabByInstance[instance]).Push(instance);
        }

        private Stack<EnemyAI> GetInactiveStack(EnemyAI prefab)
        {
            if (!_inactiveByPrefab.TryGetValue(prefab, out Stack<EnemyAI> stack))
            {
                stack = new Stack<EnemyAI>();
                _inactiveByPrefab.Add(prefab, stack);
            }

            return stack;
        }
    }
}
