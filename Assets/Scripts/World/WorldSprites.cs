using UnityEngine;

namespace NinetyMinutes.World
{
    public static class WorldSprites
    {
        static Sprite _pixel;

        public static Sprite Pixel
        {
            get
            {
                if (_pixel != null) return _pixel;
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                tex.filterMode = FilterMode.Point;
                _pixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                return _pixel;
            }
        }

        public static GameObject Quad(string name, Vector2 size, Color color, Transform parent, int sortingOrder = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Pixel;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            return go;
        }

        public static GameObject SpriteGo(string name, Sprite sprite, Vector2 worldSize, Transform parent, int sortingOrder = 0, Color? tint = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sr.sprite = sprite;
                var b = sprite.bounds.size;
                if (b.x > 0.001f && b.y > 0.001f)
                    go.transform.localScale = new Vector3(worldSize.x / b.x, worldSize.y / b.y, 1f);
            }
            else
            {
                sr.sprite = Pixel;
                sr.color = tint ?? Color.magenta;
                go.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
            }

            if (tint.HasValue && sprite != null)
                sr.color = tint.Value;
            sr.sortingOrder = sortingOrder;
            return go;
        }
    }
}
