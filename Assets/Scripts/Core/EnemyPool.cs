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
        private readonly Dictionary<EnemyBase, Stack<EnemyBase>> _inactiveByPrefab = new Dictionary<EnemyBase, Stack<EnemyBase>>();
        private readonly Dictionary<EnemyBase, EnemyBase> _prefabByInstance = new Dictionary<EnemyBase, EnemyBase>();

        public EnemyBase Get(EnemyBase prefab, Vector3 position, Quaternion rotation)
        {
            Stack<EnemyBase> inactive = GetInactiveStack(prefab);

            if (inactive.Count > 0)
            {
                EnemyBase reused = inactive.Pop();
                reused.transform.SetPositionAndRotation(position, rotation);
                // 활성화 순간 Health와 EnemyBase의 OnEnable이 체력과 상태를 초기화한다
                reused.gameObject.SetActive(true);
                return reused;
            }

            EnemyBase created = UnityEngine.Object.Instantiate(prefab, position, rotation);
            // 인스턴스의 수명 동안 한 번만 구독한다. 해제는 Dispose에서 한다
            created.DeathFinished += Release;
            _prefabByInstance.Add(created, prefab);
            return created;
        }

        public void Dispose()
        {
            foreach (EnemyBase instance in _prefabByInstance.Keys)
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

        private void Release(EnemyBase instance)
        {
            instance.gameObject.SetActive(false);
            GetInactiveStack(_prefabByInstance[instance]).Push(instance);
        }

        private Stack<EnemyBase> GetInactiveStack(EnemyBase prefab)
        {
            if (!_inactiveByPrefab.TryGetValue(prefab, out Stack<EnemyBase> stack))
            {
                stack = new Stack<EnemyBase>();
                _inactiveByPrefab.Add(prefab, stack);
            }

            return stack;
        }
    }
}
