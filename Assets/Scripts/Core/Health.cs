using System;
using UnityEngine;

namespace ArenaBreak.Core
{
    // 사망 이후의 반응(파괴, 게임오버, 풀 반환)은 Died를 구독하는 쪽이 정한다 — 플레이어와 적이 같은 Health를 쓰기 위해
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int _maxHealth = 100;
        // 플레이 중 인스펙터에서 눈으로 검증하려고 직렬화한다. 여기서 값을 바꿔도 이벤트는 나가지 않는다
        [SerializeField] private int _currentHealth;

        public event Action<int, int> HealthChanged;
        public event Action Died;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public bool IsDead { get; private set; }

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);

            // 연사로 같은 프레임에 여러 번 맞아도 Died가 한 번만 나가도록 IsDead로 막는다
            if (_currentHealth <= 0)
            {
                IsDead = true;
                Died?.Invoke();
            }
        }
    }
}
