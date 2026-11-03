using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaBreak.Player
{
    public class AimController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private string _aimBinding = "<Mouse>/rightButton";

        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _weapon;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private WeaponSystem _weaponSystem;

        [Header("Aim")]
        [SerializeField] private float _aimFieldOfView = 40f;
        // 조준선(화면 중앙)에 가늠쇠가 오도록 맞춘 무기의 카메라 기준 로컬 위치
        [SerializeField] private Vector3 _aimWeaponLocalPosition = new Vector3(0f, -0.08f, 0.55f);
        [SerializeField] private float _aimSensitivityMultiplier = 0.5f;
        [SerializeField] private float _transitionDuration = 0.15f;

        private InputAction _aimAction;

        // 비조준 상태는 씬에 배치된 값을 그대로 쓴다 — 인스펙터와 씬 값이 어긋나 시작 시 튀는 것을 막기 위해
        private float _hipFieldOfView;
        private Vector3 _hipWeaponLocalPosition;

        private bool _isAiming;
        // FOV·무기 위치·감도를 이 값 하나로 보간해 세 값이 절대 어긋나지 않게 한다
        private float _aimBlend;

        private void Awake()
        {
            // 공유 에셋(InputSystem_Actions)을 수정하지 않기 위해 조준 액션은 이 컴포넌트가 소유한다
            _aimAction = new InputAction("Aim", InputActionType.Button, _aimBinding);

            _hipFieldOfView = _camera.fieldOfView;
            _hipWeaponLocalPosition = _weapon.localPosition;
        }

        private void OnEnable()
        {
            _aimAction.Enable();
        }

        // 조준 중에 꺼지면 좁은 FOV·낮은 감도가 남으므로 비조준 상태로 되돌린다
        private void OnDisable()
        {
            _aimAction.Disable();

            _isAiming = false;
            _aimBlend = 0f;
            ApplyBlend(0f);
        }

        private void OnDestroy()
        {
            _aimAction.Dispose();
        }

        private void Update()
        {
            // 재장전 시작 시 해제와 재장전 중 차단을 한 곳에서 처리한다.
            // 목표만 바꾸면 아래 보간이 현재 진행값에서 부드럽게 비조준으로 되돌린다
            if (_weaponSystem.IsReloading)
            {
                _isAiming = false;
            }
            else if (_aimAction.WasPressedThisFrame())
            {
                _isAiming = !_isAiming;
            }

            float target = _isAiming ? 1f : 0f;
            if (Mathf.Approximately(_aimBlend, target))
            {
                return;
            }

            // 전환 도중 다시 토글해도 현재 진행값에서 곧바로 되돌아가도록 MoveTowards로 진행시킨다
            _aimBlend = Mathf.MoveTowards(_aimBlend, target, Time.deltaTime / _transitionDuration);
            ApplyBlend(Mathf.SmoothStep(0f, 1f, _aimBlend));
        }

        private void ApplyBlend(float t)
        {
            _camera.fieldOfView = Mathf.Lerp(_hipFieldOfView, _aimFieldOfView, t);
            _weapon.localPosition = Vector3.Lerp(_hipWeaponLocalPosition, _aimWeaponLocalPosition, t);
            _playerController.SensitivityMultiplier = Mathf.Lerp(1f, _aimSensitivityMultiplier, t);
        }
    }
}
