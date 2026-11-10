using ArenaBreak.Enemy;
using ArenaBreak.Player;
using UnityEngine;

namespace ArenaBreak.Core
{
    // 명중 지점에 불꽃을 튀긴다. 명중마다 파티클 오브젝트를 생성/파괴하지 않도록
    // 월드 공간 파티클 시스템 하나를 명중 지점으로 옮겨 Emit만 한다 — 이미 뿌린 입자는 월드에 남아 제자리에서 사라진다
    public class HitParticles : MonoBehaviour
    {
        [SerializeField] private WeaponSystem _weaponSystem;
        [SerializeField] private ParticleSystem _particles;
        [SerializeField, Min(1)] private int _emitCount = 10;
        // 누가 맞혔는지 바로 구분되도록 플레이어 총알과 적 탄의 색을 다르게 둔다
        [SerializeField] private Color _playerHitColor = new Color(1f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color _enemyHitColor = new Color(1f, 0.25f, 0.2f, 1f);

        private void OnEnable()
        {
            _weaponSystem.HitSurface += HandlePlayerHit;
            EnemyProjectile.AnyHit += HandleEnemyProjectileHit;
        }

        // 정적 이벤트는 씬이 바뀌어도 남으므로, 해제하지 않으면 파괴된 이 컴포넌트를 계속 호출한다
        private void OnDisable()
        {
            _weaponSystem.HitSurface -= HandlePlayerHit;
            EnemyProjectile.AnyHit -= HandleEnemyProjectileHit;
        }

        private void HandlePlayerHit(Vector3 point, Vector3 normal)
        {
            Emit(point, normal, _playerHitColor);
        }

        private void HandleEnemyProjectileHit(Vector3 point, Vector3 normal)
        {
            Emit(point, normal, _enemyHitColor);
        }

        // 파티클의 Shape(원뿔)가 transform.forward로 뿜어지므로 표면 법선 방향으로 튀게 돌려 놓는다
        private void Emit(Vector3 point, Vector3 normal, Color color)
        {
            Vector3 direction = normal.sqrMagnitude > Mathf.Epsilon ? normal : Vector3.up;
            _particles.transform.SetPositionAndRotation(point, Quaternion.LookRotation(direction));

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                startColor = color
            };
            _particles.Emit(emitParams, _emitCount);
        }
    }
}
