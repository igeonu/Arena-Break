using ArenaBreak.Core;
using ArenaBreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaBreak.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private Health _playerHealth;
        [SerializeField] private WeaponSystem _weaponSystem;
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private GameManager _gameManager;

        [Header("Health Bar")]
        // 배경 안에서 앵커가 좌우로 늘어나 있는(stretch) 빨간 막대. 앵커 오른쪽 끝을 체력 비율로 줄인다
        [SerializeField] private RectTransform _healthFill;

        [Header("Boss Health Bar")]
        // 보스 웨이브 전에는 꺼 두고 BossSpawned에서 켠다. 채우기 막대는 플레이어 체력바와 같은 stretch 방식이다
        [SerializeField] private GameObject _bossBarRoot;
        [SerializeField] private RectTransform _bossHealthFill;

        [Header("Texts")]
        [SerializeField] private Text _ammoText;
        [SerializeField] private Text _waveText;
        [SerializeField] private Text _killText;
        [SerializeField] private string _bossWaveTitle = "BOSS WAVE";

        [Header("End Panel")]
        // 재시작 안내 문구는 패널 안의 고정 텍스트로 두고, 상태별로 바뀌는 제목만 코드에서 쓴다
        [SerializeField] private GameObject _endPanel;
        [SerializeField] private Text _endTitleText;
        [SerializeField] private string _gameOverTitle = "GAME OVER";
        [SerializeField] private string _clearedTitle = "CLEAR!";

        // 보스는 스폰 시점에야 알 수 있으므로 직렬화하지 않고 BossSpawned로 받아 둔다
        private Health _bossHealth;

        private void OnEnable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.HealthChanged += HandleHealthChanged;
            }

            if (_weaponSystem != null)
            {
                _weaponSystem.AmmoChanged += HandleAmmoChanged;
            }

            if (_waveSpawner != null)
            {
                _waveSpawner.WaveStarted += HandleWaveStarted;
                _waveSpawner.KillCountChanged += HandleKillCountChanged;
                _waveSpawner.BossSpawned += HandleBossSpawned;
            }

            if (_gameManager != null)
            {
                _gameManager.StateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.HealthChanged -= HandleHealthChanged;
            }

            if (_weaponSystem != null)
            {
                _weaponSystem.AmmoChanged -= HandleAmmoChanged;
            }

            if (_waveSpawner != null)
            {
                _waveSpawner.WaveStarted -= HandleWaveStarted;
                _waveSpawner.KillCountChanged -= HandleKillCountChanged;
                _waveSpawner.BossSpawned -= HandleBossSpawned;
            }

            if (_gameManager != null)
            {
                _gameManager.StateChanged -= HandleStateChanged;
            }

            UnbindBoss();
        }

        // 이벤트는 값이 '바뀔 때'만 오므로 첫 화면은 한 번 직접 그린다.
        // 각 소스의 초기화(Health.OnEnable, WeaponSystem.Awake)가 모두 끝난 Start에서 읽어야 0으로 찍히지 않는다
        private void Start()
        {
            if (_playerHealth != null)
            {
                HandleHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            }

            if (_weaponSystem != null)
            {
                HandleAmmoChanged(_weaponSystem.CurrentAmmo, _weaponSystem.MagazineSize);
            }

            if (_waveSpawner != null)
            {
                HandleKillCountChanged(_waveSpawner.KillCount);
            }

            // 첫 WaveStarted 전까지 비어 보이지 않게 둔다
            if (_waveText != null)
            {
                _waveText.text = "WAVE -";
            }

            if (_endPanel != null)
            {
                _endPanel.SetActive(false);
            }

            if (_bossBarRoot != null)
            {
                _bossBarRoot.SetActive(false);
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            SetFill(_healthFill, current, max);
        }

        private void HandleBossHealthChanged(int current, int max)
        {
            SetFill(_bossHealthFill, current, max);
        }

        private static void SetFill(RectTransform fill, int current, int max)
        {
            if (fill == null)
            {
                return;
            }

            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            Vector2 anchorMax = fill.anchorMax;
            anchorMax.x = ratio;
            fill.anchorMax = anchorMax;
        }

        private void HandleBossSpawned(Health bossHealth)
        {
            UnbindBoss();

            _bossHealth = bossHealth;
            _bossHealth.HealthChanged += HandleBossHealthChanged;
            _bossHealth.Died += HandleBossDied;

            if (_bossBarRoot != null)
            {
                _bossBarRoot.SetActive(true);
            }

            HandleBossHealthChanged(_bossHealth.CurrentHealth, _bossHealth.MaxHealth);
        }

        private void HandleBossDied()
        {
            UnbindBoss();

            if (_bossBarRoot != null)
            {
                _bossBarRoot.SetActive(false);
            }
        }

        private void UnbindBoss()
        {
            if (_bossHealth == null)
            {
                return;
            }

            _bossHealth.HealthChanged -= HandleBossHealthChanged;
            _bossHealth.Died -= HandleBossDied;
            _bossHealth = null;
        }

        private void HandleAmmoChanged(int current, int max)
        {
            if (_ammoText != null)
            {
                _ammoText.text = $"{current} / {max}";
            }
        }

        private void HandleWaveStarted(int waveNumber)
        {
            if (_waveText == null)
            {
                return;
            }

            _waveText.text = _waveSpawner.IsBossWave(waveNumber)
                ? _bossWaveTitle
                : $"WAVE {waveNumber} / {_waveSpawner.NormalWaveCount}";
        }

        private void HandleKillCountChanged(int killCount)
        {
            if (_killText != null)
            {
                _killText.text = $"KILLS {killCount}";
            }
        }

        private void HandleStateChanged(GameState state)
        {
            bool isEnd = state == GameState.GameOver || state == GameState.Cleared;

            if (_endPanel != null)
            {
                _endPanel.SetActive(isEnd);
            }

            if (isEnd && _endTitleText != null)
            {
                _endTitleText.text = state == GameState.GameOver ? _gameOverTitle : _clearedTitle;
            }
        }
    }
}
