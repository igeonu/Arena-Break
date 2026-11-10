using ArenaBreak.Core;
using UnityEngine;

namespace ArenaBreak.Player
{
    // 플레이어가 맞은 순간 카메라를 짧게 흔든다.
    // 회전은 PlayerController가 부모(CameraPivot)에 쓰고 있으므로 충돌하지 않게 이 카메라의 로컬 위치만 흔든다
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private Health _playerHealth;
        [SerializeField, Min(0f)] private float _duration = 0.1f;
        // 흔들림 반경(미터). 카메라 로컬 위치가 원위치에서 이 거리 안에서 무작위로 움직인다
        [SerializeField, Min(0f)] private float _magnitude = 0.08f;

        private Vector3 _restLocalPosition;
        private float _shakeEndTime;
        private bool _isShaking;

        private void Awake()
        {
            _restLocalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            _playerHealth.HealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            _playerHealth.HealthChanged -= HandleHealthChanged;
            StopShake();
        }

        // HealthChanged는 데미지를 받을 때만 나간다
        private void HandleHealthChanged(int current, int max)
        {
            // 사망하는 일격에서 GameOver가 timeScale을 0으로 만들면 Time.time이 멈춰 흔들림이 끝나지 않으므로
            // 일시정지와 무관한 실제 시간으로 종료 시점을 잡는다
            _shakeEndTime = Time.unscaledTime + _duration;
            _isShaking = true;
        }

        // 다른 스크립트가 카메라 계층을 움직인 뒤 마지막에 덧씌우려고 LateUpdate에서 한다
        private void LateUpdate()
        {
            if (!_isShaking)
            {
                return;
            }

            if (Time.unscaledTime >= _shakeEndTime)
            {
                StopShake();
                return;
            }

            transform.localPosition = _restLocalPosition + Random.insideUnitSphere * _magnitude;
        }

        private void StopShake()
        {
            _isShaking = false;
            transform.localPosition = _restLocalPosition;
        }
    }
}
