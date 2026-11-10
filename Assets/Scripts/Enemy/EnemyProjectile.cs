using ArenaBreak.Core;
using UnityEngine;

namespace ArenaBreak.Enemy
{
    // 풀링 대상: 원거리 적마다 발사 간격으로 생성·파괴가 반복된다. 지금은 단순 생성/파괴로 둔다
    public class EnemyProjectile : MonoBehaviour
    {
        // 플레이어가 보고 옆으로 피할 수 있는 속도. 사거리 15m 기준 도달까지 약 2초
        [SerializeField] private float _speed = 8f;
        // 사거리 끝까지 날아갈 시간(15m / 8 ≈ 1.9초)보다 길게 둔다
        [SerializeField] private float _lifetime = 3f;
        // 눈에 보이는 탄 크기와 맞추려고 선이 아닌 구로 판정한다. 프리팹 스케일(지름)의 절반으로 둘 것
        [SerializeField] private float _hitRadius = 0.2f;
        [SerializeField] private LayerMask _hitMask = ~0;

        // 한 프레임 이동 구간에서 동시에 걸릴 수 있는 콜라이더 수의 상한. 적이 몰려 있어도 이 정도면 충분하다
        private const int MaxHitsPerStep = 8;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[MaxHitsPerStep];
        // 아군을 통과하는 몇 프레임 동안 같은 콜라이더에 GetComponentInParent를 반복하지 않으려고 기억한다
        private Collider _lastAllyCollider;
        private int _damage;
        private float _despawnTime;

        // 발사 순간의 방향(transform.forward)으로만 날아간다. 유도하지 않는다
        public void Launch(int damage)
        {
            _damage = damage;
            _despawnTime = Time.time + _lifetime;
        }

        private void Update()
        {
            if (Time.time >= _despawnTime)
            {
                Destroy(gameObject);
                return;
            }

            // 프레임 사이에 얇은 콜라이더를 지나치지 않도록 이동할 거리만큼 탄 크기의 구로 미리 검사한다.
            // 앞에 아군이 있으면 첫 번째 충돌만 보는 SphereCast로는 그 뒤를 볼 수 없으므로 전부 받아 아군을 거른다
            float step = _speed * Time.deltaTime;
            Vector3 direction = transform.forward;
            int hitCount = Physics.SphereCastNonAlloc(transform.position, _hitRadius, direction, _hitBuffer, step, _hitMask, QueryTriggerInteraction.Ignore);
            if (TryFindNearestNonAlly(hitCount, out Collider target))
            {
                HandleHit(target);
                return;
            }

            transform.position += direction * step;
        }

        // 적끼리는 같은 팀이므로 탄이 맞지도 막히지도 않고 통과한다
        private bool TryFindNearestNonAlly(int hitCount, out Collider target)
        {
            target = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _hitBuffer[i];
                if (hit.distance < nearestDistance && !IsAlly(hit.collider))
                {
                    nearestDistance = hit.distance;
                    target = hit.collider;
                }
            }

            return target != null;
        }

        // WeaponSystem과 같은 합의된 예외: 매 프레임이 아니라 무언가에 닿았을 때만 GetComponent 계열을 호출한다.
        // 콜라이더가 적의 자식(무기 등)에 붙어 있어도 같은 팀으로 보도록 부모까지 찾는다
        private bool IsAlly(Collider hitCollider)
        {
            if (hitCollider == _lastAllyCollider)
            {
                return true;
            }

            if (hitCollider.GetComponentInParent<EnemyBase>() != null)
            {
                _lastAllyCollider = hitCollider;
                return true;
            }

            return false;
        }

        private void HandleHit(Collider hitCollider)
        {
            if (hitCollider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage);
            }

            // 플레이어에게 Health가 붙기 전까지는 이 로그로만 명중 여부를 확인한다
            Debug.Log($"[EnemyProjectile] 명중 — {hitCollider.name}");
            Destroy(gameObject);
        }
    }
}
