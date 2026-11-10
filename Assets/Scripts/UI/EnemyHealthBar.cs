using ArenaBreak.Core;
using UnityEngine;

namespace ArenaBreak.UI
{
    // 적 머리 위 월드 공간 체력바. 체력이 가득일 땐 숨기고 처음 맞은 순간부터 보인다
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Health _health;
        // 배경 안에서 앵커가 좌우로 늘어나 있는(stretch) 빨간 막대. 앵커 오른쪽 끝을 체력 비율로 줄인다
        [SerializeField] private RectTransform _fill;
        // GameObject를 끄면 이 컴포넌트도 꺼져 HealthChanged를 못 받으므로, 캔버스 렌더링만 켜고 끈다
        [SerializeField] private Canvas _canvas;

        private Transform _cameraTransform;

        private void Awake()
        {
            // 프리팹은 씬의 카메라를 직렬화로 참조할 수 없어 생성 시 한 번만 찾아 둔다
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _cameraTransform = mainCamera.transform;
            }
        }

        // 적은 풀에서 재사용되므로 다시 나올 때 이전 생의 체력바가 남지 않게 되돌린다.
        // Health.OnEnable과의 실행 순서는 보장되지 않으므로 Health 값을 읽지 않고 '가득'으로 직접 맞춘다
        private void OnEnable()
        {
            SetFillRatio(1f);
            _canvas.enabled = false;
            _health.HealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            _health.HealthChanged -= HandleHealthChanged;
        }

        // 막대가 항상 화면과 나란히 보이도록 카메라 방향을 따른다. 숨겨져 있을 땐 계산하지 않는다
        private void LateUpdate()
        {
            if (!_canvas.enabled || _cameraTransform == null)
            {
                return;
            }

            transform.rotation = _cameraTransform.rotation;
        }

        private void HandleHealthChanged(int current, int max)
        {
            SetFillRatio(max > 0 ? (float)current / max : 0f);

            // 사망 후 사라지기까지의 1초 동안 빈 막대가 떠 있지 않게 0이면 숨긴다
            _canvas.enabled = current > 0 && current < max;
        }

        private void SetFillRatio(float ratio)
        {
            Vector2 anchorMax = _fill.anchorMax;
            anchorMax.x = Mathf.Clamp01(ratio);
            _fill.anchorMax = anchorMax;
        }
    }
}
