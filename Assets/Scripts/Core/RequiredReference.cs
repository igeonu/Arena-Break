using UnityEngine;

namespace ArenaBreak.Core
{
    // 인스펙터 연결이 빠졌을 때 OnEnable·Update에서 NRE가 터지는 대신, 어느 필드인지 바로 알리고 그 컴포넌트만 끈다.
    // Awake에서 끄면 OnEnable·OnDisable이 불리지 않으므로 구독 코드에 null 검사를 흩뿌리지 않아도 된다
    public static class RequiredReference
    {
        // 빠진 필드를 한 번에 모두 보이도록, 호출하는 쪽은 &&가 아니라 &로 이어서 검사할 것
        public static bool Check(MonoBehaviour owner, Object reference, string fieldName)
        {
            if (reference != null)
            {
                return true;
            }

            Debug.LogError($"[{owner.GetType().Name}] {owner.name}: {fieldName}이(가) 연결되지 않아 비활성화한다", owner);
            owner.enabled = false;
            return false;
        }
    }
}
