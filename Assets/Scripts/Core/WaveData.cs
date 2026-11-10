using ArenaBreak.Enemy;
using UnityEngine;

namespace ArenaBreak.Core
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "ArenaBreak/Wave Data")]
    public class WaveData : ScriptableObject
    {
        // GameObject 대신 EnemyAI로 받아 EnemyAI가 없는 프리팹을 인스펙터에서부터 막는다
        [SerializeField] private EnemyAI _enemyPrefab;
        [SerializeField, Min(1)] private int _spawnCount = 5;
        [SerializeField, Min(0f)] private float _spawnInterval = 1f;

        public EnemyAI EnemyPrefab => _enemyPrefab;
        public int SpawnCount => _spawnCount;
        public float SpawnInterval => _spawnInterval;
    }
}
