using UnityEngine;

namespace ArenaBreak.Core
{
    // 발사 순간 총 모델의 총구가 들리고(필요하면 뒤로 밀렸다가) 원래 자세로 돌아오는 연출.
    // 플레이어와 적이 같이 쓰도록 누가 쐈는지는 모르고 Kick() 호출만 받는다
    public class WeaponKick : MonoBehaviour
    {
        // 한 발당 총구가 들리는 각도(도)
        [SerializeField] private float _kickPitch = 12f;
        // 한 발당 뒤로 밀리는 거리(미터). 플레이어 총은 AimController가 위치를 쓰므로 0으로 둘 것
        [SerializeField, Min(0f)] private float _kickBack;
        // 반동이 목표 자세까지 따라붙는 속도. 클수록 '탁' 하고 튄다
        [SerializeField, Min(0f)] private float _snapSpeed = 40f;
        // 반동이 원래 자세로 돌아오는 속도
        [SerializeField, Min(0f)] private float _recoverySpeed = 10f;

        private const float SettleThreshold = 0.001f;

        private Quaternion _restRotation;
        private Vector3 _restPosition;
        // 누적 반동량(한 발 = 1). 연사하면 회복 전에 쌓인다
        private float _targetAmount;
        private float _currentAmount;
        private bool _isActive;

        private void Awake()
        {
            _restRotation = transform.localRotation;
            _restPosition = transform.localPosition;
        }

        // 적이 풀로 돌아갈 때 반동 자세가 남지 않게 되돌린다
        private void OnDisable()
        {
            Settle();
        }

        public void Kick()
        {
            _targetAmount += 1f;
            _isActive = true;
        }

        // 다른 스크립트가 자세를 정한 뒤 덧씌우려고 LateUpdate에서 적용한다
        private void LateUpdate()
        {
            if (!_isActive)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            // 프레임레이트와 무관하게 같은 속도로 따라가도록 지수 보간을 쓴다
            _currentAmount = Mathf.Lerp(_currentAmount, _targetAmount, 1f - Mathf.Exp(-_snapSpeed * deltaTime));
            _targetAmount = Mathf.Lerp(_targetAmount, 0f, 1f - Mathf.Exp(-_recoverySpeed * deltaTime));

            if (_targetAmount < SettleThreshold && _currentAmount < SettleThreshold)
            {
                Settle();
                return;
            }

            transform.localRotation = _restRotation * Quaternion.Euler(-_kickPitch * _currentAmount, 0f, 0f);

            if (_kickBack > 0f)
            {
                transform.localPosition = _restPosition + Vector3.back * (_kickBack * _currentAmount);
            }
        }

        private void Settle()
        {
            _isActive = false;
            _targetAmount = 0f;
            _currentAmount = 0f;
            transform.localRotation = _restRotation;

            if (_kickBack > 0f)
            {
                transform.localPosition = _restPosition;
            }
        }
    }
}
