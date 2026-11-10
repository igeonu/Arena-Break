using UnityEngine;

namespace ArenaBreak.Enemy
{
    // 판정 반경을 플레이어가 눈으로 읽을 수 있게, 바닥에 원판을 0에서 반경까지 펼쳤다가 끈다.
    // 적과 함께 풀로 재사용되므로 생성/파괴 대신 렌더러만 켜고 끈다.
    // GameObject 자체를 끄면 부모(적)가 활성화되는 도중 다시 켜야 해서 SetActive 충돌이 나므로 렌더러를 쓴다
    [RequireComponent(typeof(MeshRenderer))]
    public class ShockwaveVisual : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.35f;
        // 원판 메시(Cylinder) 높이 방향 스케일. 바닥에 납작하게 깔리게 한다
        [SerializeField] private float _thickness = 0.025f;

        private MeshRenderer _renderer;
        private float _radius;
        private float _elapsed;
        private bool _isPlaying;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        // 적이 풀에서 다시 나올 때 이전 생의 원판이 남아 보이지 않게 한다
        private void OnEnable()
        {
            Stop();
        }

        public void Play(float radius)
        {
            _radius = radius;
            _elapsed = 0f;
            _isPlaying = true;
            _renderer.enabled = true;
            ApplyScale(0f);
        }

        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            float progress = _elapsed / _duration;
            if (progress >= 1f)
            {
                Stop();
                return;
            }

            ApplyScale(progress);
        }

        private void Stop()
        {
            _isPlaying = false;
            _renderer.enabled = false;
        }

        // Cylinder 메시의 지름이 1이므로 스케일 = 지름 = 반경 × 2
        private void ApplyScale(float progress)
        {
            float diameter = _radius * 2f * progress;
            transform.localScale = new Vector3(diameter, _thickness, diameter);
        }
    }
}
