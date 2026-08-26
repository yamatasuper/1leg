using UnityEngine;
using UnityEngine.UI;

namespace NinetyMinutes.Match
{
    /// <summary>
    /// One comic frame, cycling stills like a flipbook.
    /// </summary>
    public sealed class PanelStripAnimator : MonoBehaviour
    {
        public Image Target;
        public float FrameSeconds = 0.22f;

        Sprite[] _frames;
        int _head;
        float _t;
        bool _playing;

        public void Play(Sprite[] frames, float frameSeconds = 0.22f)
        {
            _frames = frames;
            FrameSeconds = Mathf.Max(0.08f, frameSeconds);
            _head = 0;
            _t = 0f;
            _playing = _frames != null && _frames.Length > 0;
            Push();
        }

        public void Stop()
        {
            _playing = false;
        }

        void Update()
        {
            if (!_playing || _frames == null || _frames.Length < 2) return;
            _t += Time.unscaledDeltaTime;
            if (_t < FrameSeconds) return;
            _t = 0f;
            _head = (_head + 1) % _frames.Length;
            Push();
        }

        void Push()
        {
            if (Target == null || _frames == null || _frames.Length == 0) return;
            var sprite = _frames[_head % _frames.Length];
            if (sprite == null)
            {
                Target.sprite = null;
                Target.color = new Color(0.25f, 0.28f, 0.3f);
                return;
            }

            Target.sprite = sprite;
            Target.color = Color.white;
            Target.preserveAspect = true;
            Target.type = Image.Type.Simple;
        }
    }
}
