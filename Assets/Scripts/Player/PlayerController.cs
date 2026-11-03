using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaBreak.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;

        [Header("References")]
        [SerializeField] private Transform _cameraPivot;

        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -20f;
        // isGrounded가 경사·계단에서 깜빡이지 않도록 지면에 붙여두는 작은 하강 속도
        [SerializeField] private float _groundedStickVelocity = -2f;

        [Header("Look")]
        [SerializeField] private float _mouseSensitivity = 0.1f;
        [SerializeField] private float _minPitch = -80f;
        [SerializeField] private float _maxPitch = 80f;
        // 커서가 잠기는 첫 프레임에 delta가 크게 튀어 시점이 순간이동하는 것을 막는다
        [SerializeField] private float _maxLookDeltaPerFrame = 50f;

        [Header("Cursor")]
        [SerializeField] private bool _lockCursor = true;

        private CharacterController _characterController;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;

        private float _pitch;
        private float _verticalVelocity;

        // 조준(ADS) 등 외부 상태가 인스펙터 감도를 덮어쓰지 않고 배율로만 조정하도록 분리
        public float SensitivityMultiplier { get; set; } = 1f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            InputActionMap playerMap = _inputActions.FindActionMap("Player", throwIfNotFound: true);
            _moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
            _lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
            _jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);
        }

        // 맵 전체가 아니라 사용하는 액션만 켜고 끈다 — 같은 에셋을 다른 컴포넌트(무기 등)도 공유하기 때문
        private void OnEnable()
        {
            _moveAction.Enable();
            _lookAction.Enable();
            _jumpAction.Enable();

            if (_lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _lookAction.Disable();
            _jumpAction.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
        }

        private void HandleLook()
        {
            // 마우스 delta는 이미 프레임 단위 값이므로 deltaTime을 곱하지 않는다
            Vector2 lookDelta = Vector2.ClampMagnitude(_lookAction.ReadValue<Vector2>(), _maxLookDeltaPerFrame);
            lookDelta *= _mouseSensitivity * SensitivityMultiplier;

            transform.Rotate(Vector3.up, lookDelta.x, Space.Self);

            _pitch = Mathf.Clamp(_pitch - lookDelta.y, _minPitch, _maxPitch);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            bool isGrounded = _characterController.isGrounded;

            if (isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedStickVelocity;
            }

            if (isGrounded && _jumpAction.WasPressedThisFrame())
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }

            _verticalVelocity += _gravity * Time.deltaTime;

            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            Vector3 horizontal = (transform.right * moveInput.x + transform.forward * moveInput.y) * _moveSpeed;
            Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;

            _characterController.Move(velocity * Time.deltaTime);
        }
    }
}
