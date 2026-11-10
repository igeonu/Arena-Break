using System;
using System.Collections;
using System.Collections.Generic;
using ArenaBreak.Enemy;
using UnityEngine;

namespace ArenaBreak.Core
{
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private List<WaveData> _waves = new List<WaveData>();
        [SerializeField] private Transform _spawnPointsRoot;
        [SerializeField] private Transform _player;
        [SerializeField, Min(0f)] private float _timeBetweenWaves = 3f;

        private Transform[] _spawnPoints;
        // 적마다 다른 람다로 구독하므로, 해제할 때 같은 델리게이트를 찾으려고 Health별로 보관한다
        private readonly Dictionary<Health, Action> _aliveEnemies = new Dictionary<Health, Action>();
        private WaitForSeconds _waitBetweenWaves;
        private readonly EnemyPool _pool = new EnemyPool();

        // 웨이브 번호는 UI에 바로 쓰도록 1부터 센다
        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        public event Action AllWavesCleared;
        // 누적 처치 수를 넘긴다. 살아 있는 적 수를 세려고 이미 사망을 구독하고 있어 여기서 함께 센다
        public event Action<int> KillCountChanged;

        public int TotalWaves => _waves.Count;
        public int KillCount { get; private set; }

        private void Awake()
        {
            CollectSpawnPoints();
            _waitBetweenWaves = new WaitForSeconds(_timeBetweenWaves);
        }

        private void Start()
        {
            if (_spawnPoints.Length == 0)
            {
                Debug.LogError($"[WaveSpawner] {name}: 스폰 포인트가 없어 웨이브를 시작하지 않는다");
                return;
            }

            StartCoroutine(RunWaves());
        }

        private void OnDisable()
        {
            foreach (KeyValuePair<Health, Action> pair in _aliveEnemies)
            {
                // 적이 이미 파괴됐을 수 있다
                if (pair.Key != null)
                {
                    pair.Key.Died -= pair.Value;
                }
            }

            _aliveEnemies.Clear();
        }

        private void OnDestroy()
        {
            _pool.Dispose();
        }

        private void CollectSpawnPoints()
        {
            if (_spawnPointsRoot == null)
            {
                _spawnPoints = Array.Empty<Transform>();
                return;
            }

            _spawnPoints = new Transform[_spawnPointsRoot.childCount];
            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                _spawnPoints[i] = _spawnPointsRoot.GetChild(i);
            }
        }

        private IEnumerator RunWaves()
        {
            for (int i = 0; i < _waves.Count; i++)
            {
                int waveNumber = i + 1;
                WaveStarted?.Invoke(waveNumber);

                yield return SpawnWave(_waves[i]);

                // 스폰이 끝나기 전에 먼저 나온 적이 다 죽어도 클리어로 치지 않도록 스폰 완료 후부터 기다린다
                yield return new WaitUntil(() => _aliveEnemies.Count == 0);

                WaveCleared?.Invoke(waveNumber);

                if (i < _waves.Count - 1)
                {
                    yield return _waitBetweenWaves;
                }
            }

            AllWavesCleared?.Invoke();
        }

        private IEnumerator SpawnWave(WaveData wave)
        {
            if (!HasValidPrefabs(wave))
            {
                Debug.LogError($"[WaveSpawner] {name}: 비어 있는 WaveData 또는 적 프리팹이 있어 이 웨이브를 건너뛴다");
                yield break;
            }

            WaitForSeconds waitInterval = new WaitForSeconds(wave.SpawnInterval);

            for (int i = 0; i < wave.SpawnCount; i++)
            {
                SpawnEnemy(PickRandomPrefab(wave));

                if (i < wave.SpawnCount - 1)
                {
                    yield return waitInterval;
                }
            }
        }

        // 웨이브 시작 전에 한 번에 검사해, 스폰 도중 null 프리팹을 만나 수가 어긋나는 일을 막는다
        private static bool HasValidPrefabs(WaveData wave)
        {
            if (wave == null || wave.EnemyPrefabs.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < wave.EnemyPrefabs.Count; i++)
            {
                if (wave.EnemyPrefabs[i] == null)
                {
                    return false;
                }
            }

            return true;
        }

        // 스폰마다 독립적으로 뽑으므로 한 웨이브가 한 종류로만 나올 수도 있다(사용자 결정)
        private static EnemyBase PickRandomPrefab(WaveData wave)
        {
            return wave.EnemyPrefabs[UnityEngine.Random.Range(0, wave.EnemyPrefabs.Count)];
        }

        private void SpawnEnemy(EnemyBase prefab)
        {
            Transform spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
            EnemyBase enemy = _pool.Get(prefab, spawnPoint.position, spawnPoint.rotation);
            enemy.SetTarget(_player);

            Health health = enemy.GetComponent<Health>();
            Action onDied = null;
            onDied = () => HandleEnemyDied(health, onDied);
            health.Died += onDied;
            _aliveEnemies.Add(health, onDied);
        }

        private void HandleEnemyDied(Health health, Action onDied)
        {
            health.Died -= onDied;
            _aliveEnemies.Remove(health);

            KillCount++;
            KillCountChanged?.Invoke(KillCount);
        }
    }
}
