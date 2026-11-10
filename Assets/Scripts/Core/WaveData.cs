using System.Collections.Generic;
using ArenaBreak.Enemy;
using UnityEngine;

namespace ArenaBreak.Core
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "ArenaBreak/Wave Data")]
    public class WaveData : ScriptableObject
    {
        // 스폰할 때마다 이 목록에서 균등하게 하나를 뽑는다. 두 종류를 넣으면 각각 50%
        // GameObject 대신 EnemyBase로 받아 적 AI가 없는 프리팹을 인스펙터에서부터 막는다
        [SerializeField] private List<EnemyBase> _enemyPrefabs = new List<EnemyBase>();
        [SerializeField, Min(1)] private int _spawnCount = 5;
        [SerializeField, Min(0f)] private float _spawnInterval = 1f;

        public IReadOnlyList<EnemyBase> EnemyPrefabs => _enemyPrefabs;
        public int SpawnCount => _spawnCount;
        public float SpawnInterval => _spawnInterval;
    }
}
