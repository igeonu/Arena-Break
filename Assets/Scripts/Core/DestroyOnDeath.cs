using UnityEngine;

namespace ArenaBreak.Core
{
    // Health는 사망 후 처리를 정하지 않으므로, 파괴가 필요한 대상에만 이 컴포넌트를 붙인다
    [RequireComponent(typeof(Health))]
    public class DestroyOnDeath : MonoBehaviour
    {
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.Died += HandleDied;
        }

        private void OnDisable()
        {
            _health.Died -= HandleDied;
        }

        private void HandleDied()
        {
            Destroy(gameObject);
        }
    }
}
