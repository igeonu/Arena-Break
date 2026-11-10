using System;
using ArenaBreak.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArenaBreak.Core
{
    public enum GameState
    {
        Ready,
        Playing,
        GameOver,
        Cleared
    }

    public class GameManager : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField, Min(0f)] private float _readyDuration = 2f;

        [Header("References")]
        [SerializeField] private Health _playerHealth;
        [SerializeField] private WaveSpawner _waveSpawner;
        // 배열로 묶지 않고 타입별로 받는다 — 타입을 넓히면 인스펙터에서 엉뚱한 컴포넌트도 들어갈 수 있다
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private WeaponSystem _weaponSystem;

        [Header("Input")]
        [SerializeField] private string _restartBinding = "<Keyboard>/r";
        [SerializeField] private string _quitBinding = "<Keyboard>/escape";

        private InputAction _restartAction;
        private InputAction _quitAction;
        private float _readyEndTime;

        public event Action<GameState> StateChanged;

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            // 공유 에셋(InputSystem_Actions)을 수정하지 않기 위해 재시작·종료 액션은 이 컴포넌트가 소유한다
            _restartAction = new InputAction("Restart", InputActionType.Button, _restartBinding);
            _quitAction = new InputAction("Quit", InputActionType.Button, _quitBinding);
        }

        private void OnEnable()
        {
            _restartAction.Enable();
            _quitAction.Enable();

            if (_playerHealth != null)
            {
                _playerHealth.Died += HandlePlayerDied;
            }

            if (_waveSpawner != null)
            {
                _waveSpawner.AllWavesCleared += HandleAllWavesCleared;
            }
        }

        private void OnDisable()
        {
            _restartAction.Disable();
            _quitAction.Disable();

            if (_playerHealth != null)
            {
                _playerHealth.Died -= HandlePlayerDied;
            }

            if (_waveSpawner != null)
            {
                _waveSpawner.AllWavesCleared -= HandleAllWavesCleared;
            }
        }

        private void OnDestroy()
        {
            _restartAction.Dispose();
            _quitAction.Dispose();
        }

        private void Start()
        {
            // 이전 판이 timeScale 0인 채로 재시작됐어도 여기서 확실히 정상 속도로 시작한다
            Time.timeScale = 1f;
            _readyEndTime = Time.time + _readyDuration;
            ChangeState(GameState.Ready);
        }

        private void Update()
        {
            // 어느 상태에서든 종료할 수 있어야 하므로 상태 분기보다 먼저 검사한다
            if (_quitAction.WasPressedThisFrame())
            {
                Application.Quit();
                return;
            }

            switch (CurrentState)
            {
                case GameState.Ready:
                    if (Time.time >= _readyEndTime)
                    {
                        ChangeState(GameState.Playing);
                    }
                    break;

                case GameState.Playing:
                    break;

                // timeScale 0이어도 Update는 돌기 때문에 이 상태에서 입력을 받을 수 있다
                case GameState.GameOver:
                case GameState.Cleared:
                    if (_restartAction.WasPressedThisFrame())
                    {
                        RestartScene();
                    }
                    break;
            }
        }

        private void HandlePlayerDied()
        {
            EnterEndState(GameState.GameOver);
        }

        private void HandleAllWavesCleared()
        {
            EnterEndState(GameState.Cleared);
        }

        // 클리어 직후 같은 프레임에 죽는 등 끝 상태가 겹쳐도 먼저 정해진 결과를 유지한다
        private void EnterEndState(GameState endState)
        {
            if (CurrentState == GameState.GameOver || CurrentState == GameState.Cleared)
            {
                return;
            }

            // timeScale 0은 Update를 멈추지 않아 조작 컴포넌트를 끄지 않으면 멈춘 화면에서도 총이 나간다
            if (_playerController != null)
            {
                _playerController.enabled = false;
            }

            if (_weaponSystem != null)
            {
                _weaponSystem.enabled = false;
            }

            // PlayerController.OnDisable도 커서를 풀지만, 참조가 비어 있어도 R/Esc 안내를 볼 수 있게 직접 보장한다
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;

            ChangeState(endState);
        }

        private void RestartScene()
        {
            // timeScale은 씬을 다시 로드해도 초기화되지 않는 전역 값이라 먼저 되돌린다
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ChangeState(GameState newState)
        {
            CurrentState = newState;
            StateChanged?.Invoke(newState);
        }
    }
}
