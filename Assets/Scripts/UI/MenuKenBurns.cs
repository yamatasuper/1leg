using UnityEngine;

namespace NinetyMinutes.UI
{
    public sealed class MenuKenBurns : MonoBehaviour
    {
        public float MinScale = 1.06f;
        public float MaxScale = 1.14f;
        public float Seconds = 36f;

        RectTransform _rt;

        void Awake()
        {
            _rt = transform as RectTransform;
        }

        void Update()
        {
            if (_rt == null) return;
            var u = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / Seconds) + 1f) * 0.5f;
            var s = Mathf.Lerp(MinScale, MaxScale, u);
            _rt.localScale = new Vector3(s, s, 1f);
        }
    }
}
