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

        // 웨이브 번호는 UI에 바로 쓰도록 1부터 센다
        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        public event Action AllWavesCleared;

        public int TotalWaves => _waves.Count;

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
            if (wave == null || wave.EnemyPrefab == null)
            {
                Debug.LogError($"[WaveSpawner] {name}: 비어 있는 WaveData 또는 적 프리팹이 있어 이 웨이브를 건너뛴다");
                yield break;
            }

            WaitForSeconds waitInterval = new WaitForSeconds(wave.SpawnInterval);

            for (int i = 0; i < wave.SpawnCount; i++)
            {
                SpawnEnemy(wave.EnemyPrefab);

                if (i < wave.SpawnCount - 1)
                {
                    yield return waitInterval;
                }
            }
        }

        // TODO(풀링): 웨이브마다 생성/파괴가 반복되므로 풀링 대상
        private void SpawnEnemy(EnemyAI prefab)
        {
            Transform spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
            EnemyAI enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
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
        }
    }
}
