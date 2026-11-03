using System;
using ArenaBreak.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaBreak.Player
{
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private string _reloadBinding = "<Keyboard>/r";

        [Header("References")]
        [SerializeField] private Camera _camera;

        [Header("Fire")]
        [SerializeField] private float _fireInterval = 0.15f;
        [SerializeField] private float _range = 50f;
        [SerializeField] private int _damage = 25;
        [SerializeField] private LayerMask _hitMask = ~0;

        [Header("Ammo")]
        [SerializeField] private int _magazineSize = 12;
        [SerializeField] private float _reloadDuration = 1.5f;

        [Header("Debug Hit Marker")]
        // 벽·표적과 같은 색이면 명중 지점이 묻혀 보이지 않으므로 대비되는 색을 기본값으로 둔다
        [SerializeField] private Color _hitMarkerColor = Color.yellow;
        [SerializeField] private float _hitMarkerSize = 0.1f;
        [SerializeField] private float _hitMarkerLifetime = 0.5f;

        private static readonly Vector3 ViewportCenter = new Vector3(0.5f, 0.5f, 0f);

        private InputAction _attackAction;
        private InputAction _reloadAction;
        private Material _hitMarkerMaterial;
        private GameObject _hitMarkerTemplate;

        public event Action ReloadStarted;

        public bool IsReloading => _isReloading;

        private int _currentAmmo;
        private float _nextFireTime;
        private bool _isReloading;
        private float _reloadEndTime;

        private void Awake()
        {
            InputActionMap playerMap = _inputActions.FindActionMap("Player", throwIfNotFound: true);
            _attackAction = playerMap.FindAction("Attack", throwIfNotFound: true);

            // 공유 에셋(InputSystem_Actions)을 수정하지 않기 위해 재장전 액션은 이 컴포넌트가 소유한다
            _reloadAction = new InputAction("Reload", InputActionType.Button, _reloadBinding);

            _currentAmmo = _magazineSize;
            _hitMarkerTemplate = CreateHitMarkerTemplate();
        }

        private void OnEnable()
        {
            _attackAction.Enable();
            _reloadAction.Enable();
        }

        private void OnDisable()
        {
            _attackAction.Disable();
            _reloadAction.Disable();
        }

        private void OnDestroy()
        {
            _reloadAction.Dispose();

            Destroy(_hitMarkerTemplate);
            Destroy(_hitMarkerMaterial);
        }

        private void Update()
        {
            if (_isReloading)
            {
                if (Time.time >= _reloadEndTime)
                {
                    FinishReload();
                }
                return;
            }

            if (_reloadAction.WasPressedThisFrame() && _currentAmmo < _magazineSize)
            {
                StartReload();
                return;
            }

            if (_attackAction.IsPressed() && _currentAmmo > 0 && Time.time >= _nextFireTime)
            {
                Fire();
            }
        }

        private void Fire()
        {
            _nextFireTime = Time.time + _fireInterval;
            _currentAmmo--;

            Ray ray = _camera.ViewportPointToRay(ViewportCenter);
            if (Physics.Raycast(ray, out RaycastHit hit, _range, _hitMask, QueryTriggerInteraction.Ignore))
            {
                SpawnHitMarker(hit.point);

                // 규약(Update 내 GetComponent 금지)의 합의된 예외: 매 프레임이 아니라 명중 시에만 호출되고,
                // 대상이 런타임에 스폰·파괴되므로 Awake 캐싱이 불가능하다
                if (hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(_damage);
                }
            }

            // 3장에서 HUD 이벤트로 옮길 임시 출력
            Debug.Log($"[Weapon] 발사 — 남은 탄약 {_currentAmmo}/{_magazineSize}");
        }

        private void StartReload()
        {
            _isReloading = true;
            _reloadEndTime = Time.time + _reloadDuration;
            Debug.Log($"[Weapon] 재장전 시작 — 남은 탄약 {_currentAmmo}/{_magazineSize}");
            ReloadStarted?.Invoke();
        }

        private void FinishReload()
        {
            _isReloading = false;
            _currentAmmo = _magazineSize;
            Debug.Log($"[Weapon] 재장전 완료 — 남은 탄약 {_currentAmmo}/{_magazineSize}");
        }

        // Update 경로에서 GetComponent를 피하려고 세팅이 끝난 마커를 한 번 만들어두고 복제한다
        private GameObject CreateHitMarkerTemplate()
        {
            GameObject template = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            template.name = "HitMarker";
            template.SetActive(false);

            // 콜라이더가 남아 있으면 같은 지점을 향한 다음 발사가 마커에 맞는다
            DestroyImmediate(template.GetComponent<Collider>());

            template.transform.localScale = Vector3.one * _hitMarkerSize;

            // 마커마다 renderer.material로 인스턴스를 만들면 누수되므로 머티리얼 하나를 공유한다
            Renderer markerRenderer = template.GetComponent<Renderer>();
            _hitMarkerMaterial = new Material(markerRenderer.sharedMaterial) { color = _hitMarkerColor };
            markerRenderer.sharedMaterial = _hitMarkerMaterial;

            return template;
        }

        // 풀링 대상: 발사마다 생성·파괴가 반복된다. 디버그용이라 지금은 단순 생성으로 둔다
        private void SpawnHitMarker(Vector3 position)
        {
            GameObject marker = Instantiate(_hitMarkerTemplate, position, Quaternion.identity);
            marker.SetActive(true);
            Destroy(marker, _hitMarkerLifetime);
        }
    }
}
