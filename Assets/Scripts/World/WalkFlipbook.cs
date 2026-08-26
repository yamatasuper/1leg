using NinetyMinutes.Art;
using UnityEngine;

namespace NinetyMinutes.World
{
    public sealed class WalkFlipbook : MonoBehaviour
    {
        public float Fps = 8f;
        PlayerController _player;
        MeshRenderer _paint;
        Material _mat;
        Texture[] _frames;
        Texture _idle;
        float _t;
        int _i;

        void LateUpdate()
        {
            EnsureBound();
            if (_mat == null || _frames == null || _frames.Length == 0) return;
            var moving = _player != null && _player.IsMoving;
            if (!moving)
            {
                if (_idle != null) _mat.mainTexture = _idle;
                return;
            }

            _t += Time.deltaTime * Fps;
            if (_t >= 1f)
            {
                _t -= 1f;
                _i = (_i + 1) % _frames.Length;
            }

            if (_frames[_i] != null)
                _mat.mainTexture = _frames[_i];
        }

        void EnsureBound()
        {
            if (_player == null)
                _player = GetComponent<PlayerController>() ?? GetComponentInParent<PlayerController>();
            if (_paint == null)
            {
                var paint = transform.Find("Paint");
                if (paint != null) _paint = paint.GetComponent<MeshRenderer>();
            }

            if (_paint != null && _mat == null)
            {
                _mat = _paint.material;
                var idleSprite = ArtCatalog.SpritePlayer;
                _idle = idleSprite != null ? idleSprite.texture : _mat.mainTexture;
            }

            if (_frames != null) return;
            var sprites = ArtCatalog.RunFrames();
            if (sprites == null || sprites.Length == 0) return;
            _frames = new Texture[sprites.Length];
            for (var i = 0; i < sprites.Length; i++)
                _frames[i] = sprites[i] != null ? sprites[i].texture : null;
        }
    }
}
