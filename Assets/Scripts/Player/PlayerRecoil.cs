using ArenaBreak.Core;
using UnityEngine;

namespace ArenaBreak.Player
{
    // 발사할 때마다 조준(카메라)을 위로, 좌우로 살짝 튀게 하고 쏘지 않으면 원래 조준으로 되돌린다.
    // 시점 회전은 PlayerController가 부모(CameraPivot)에 쓰므로 그 위에 이 카메라의 로컬 회전으로 덧씌운다.
    // WeaponSystem은 이 카메라 기준으로 레이를 쏘므로, 튄 조준이 다음 탄의 실제 탄착에 반영된다
    public class PlayerRecoil : MonoBehaviour
    {
        [SerializeField] private WeaponSystem _weaponSystem;
        // 총 모델 연출. 없으면 조준 반동만 한다
        [SerializeField] private WeaponKick _weaponKick;

        [Header("Aim Punch (셰리프 느낌의 근사값)")]
        // 한 발당 조준이 위로 튀는 각도(도)
        [SerializeField] private float _kickPitch = 3.5f;
        // 한 발당 좌우로 튀는 최대 각도(도). 매 발 이 범위에서 무작위
        [SerializeField, Min(0f)] private float _kickYawRange = 0.8f;
        [SerializeField, Min(0f)] private float _snapSpeed = 35f;
        // 발사 간격(0.4초) 안에 대부분 돌아오도록 잡은 회복 속도. 낮추면 연사 시 반동이 더 쌓인다
        [SerializeField, Min(0f)] private float _recoverySpeed = 8f;

        private const float SettleThreshold = 0.001f;

        private Quaternion _restRotation;
        // x = 위로 튄 각도, y = 좌우로 튄 각도
        private Vector2 _targetOffset;
        private Vector2 _currentOffset;
        private bool _isActive;

        private void Awake()
        {
            _restRotation = transform.localRotation;
        }

        private void OnEnable()
        {
            _weaponSystem.Fired += HandleFired;
        }

        private void OnDisable()
        {
            _weaponSystem.Fired -= HandleFired;
            Settle();
        }

        private void HandleFired()
        {
            _targetOffset.x += _kickPitch;
            _targetOffset.y += Random.Range(-_kickYawRange, _kickYawRange);
            _isActive = true;

            if (_weaponKick != null)
            {
                _weaponKick.Kick();
            }
        }

        private void LateUpdate()
        {
            if (!_isActive)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            // 프레임레이트와 무관하게 같은 속도로 따라가도록 지수 보간을 쓴다
            _currentOffset = Vector2.Lerp(_currentOffset, _targetOffset, 1f - Mathf.Exp(-_snapSpeed * deltaTime));
            _targetOffset = Vector2.Lerp(_targetOffset, Vector2.zero, 1f - Mathf.Exp(-_recoverySpeed * deltaTime));

            if (_targetOffset.sqrMagnitude < SettleThreshold && _currentOffset.sqrMagnitude < SettleThreshold)
            {
                Settle();
                return;
            }

            // 유니티에서 x축 음수 회전이 '위를 봄'이다
            transform.localRotation = _restRotation * Quaternion.Euler(-_currentOffset.x, _currentOffset.y, 0f);
        }

        private void Settle()
        {
            _isActive = false;
            _targetOffset = Vector2.zero;
            _currentOffset = Vector2.zero;
            transform.localRotation = _restRotation;
        }
    }
}
