using System.Collections;
using ArenaBreak.Core;
using UnityEngine;

namespace ArenaBreak.Enemy
{
    // 피격 순간 몸 색을 잠깐 덮어 맞았다는 것을 눈으로 알린다.
    // renderer.material로 바꾸면 적마다 머티리얼 복제본이 생기고, sharedMaterial을 바꾸면 같은 종류 적이 전부 하얘지므로
    // 머티리얼은 그대로 두고 MaterialPropertyBlock으로 이 렌더러 하나에만 덮어쓴다
    [RequireComponent(typeof(Health))]
    public class HitFlash : MonoBehaviour
    {
        // URP Lit의 기본 색 프로퍼티
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField, Min(0f)] private float _flashDuration = 0.05f;

        private Health _health;
        private MaterialPropertyBlock _flashBlock;
        // 아무 값도 넣지 않은 블록을 적용하면 덮어쓴 값이 모두 빠져 머티리얼 원래 색으로 돌아간다
        private MaterialPropertyBlock _clearBlock;
        private WaitForSeconds _flashWait;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _flashBlock = new MaterialPropertyBlock();
            _flashBlock.SetColor(BaseColorId, _flashColor);
            _clearBlock = new MaterialPropertyBlock();
            _flashWait = new WaitForSeconds(_flashDuration);
        }

        private void OnEnable()
        {
            _health.HealthChanged += HandleHealthChanged;
        }

        // 플래시 도중 비활성화(풀 회수)되면 코루틴이 멈춰 흰색이 남으므로, 꺼질 때 항상 원래 색으로 돌린다
        private void OnDisable()
        {
            _health.HealthChanged -= HandleHealthChanged;
            _flashRoutine = null;
            _bodyRenderer.SetPropertyBlock(_clearBlock);
        }

        private void HandleHealthChanged(int current, int max)
        {
            // 연사로 플래시가 겹치면 앞의 것이 중간에 색을 되돌리지 않도록 새로 시작한다
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            _bodyRenderer.SetPropertyBlock(_flashBlock);
            yield return _flashWait;
            _bodyRenderer.SetPropertyBlock(_clearBlock);
            _flashRoutine = null;
        }
    }
}
