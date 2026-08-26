using UnityEngine;

namespace NinetyMinutes.World
{
    /// <summary>
    /// Runtime 3D primitives. Manga textures go on walls/floors; characters are billboards.
    /// </summary>
    public static class WorldSprites
    {
        static Material _litSrc;
        static Material _unlitSrc;
        static Material _cutoutSrc;

        public static readonly Color KitDark = new Color(0.16f, 0.15f, 0.14f);
        public static readonly Color KitYellow = new Color(0.78f, 0.58f, 0.22f);
        public static readonly Color Ochre = new Color(0.62f, 0.48f, 0.28f);
        public static readonly Color TealShadow = new Color(0.28f, 0.36f, 0.38f);

        static Material LitSrc
        {
            get
            {
                if (_litSrc != null) return _litSrc;
                var sh = Shader.Find("Standard") ?? Shader.Find("Diffuse");
                _litSrc = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
                if (_litSrc.HasProperty("_Glossiness")) _litSrc.SetFloat("_Glossiness", 0.18f);
                if (_litSrc.HasProperty("_Metallic")) _litSrc.SetFloat("_Metallic", 0.04f);
                return _litSrc;
            }
        }

        static Material UnlitSrc
        {
            get
            {
                if (_unlitSrc != null) return _unlitSrc;
                var sh = Shader.Find("Unlit/Texture") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                _unlitSrc = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
                return _unlitSrc;
            }
        }

        static Material CutoutSrc
        {
            get
            {
                if (_cutoutSrc != null) return _cutoutSrc;
                var sh = Shader.Find("Sprites/Default")
                         ?? Shader.Find("Unlit/Transparent")
                         ?? Shader.Find("Unlit/Texture");
                _cutoutSrc = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
                return _cutoutSrc;
            }
        }

        public static Material Lit(Color color)
        {
            var m = new Material(LitSrc) { color = color };
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            return m;
        }

        public static Material Textured(Texture tex, Color? tint = null)
        {
            var m = new Material(UnlitSrc);
            if (tex != null)
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                m.mainTexture = tex;
            }
            var c = tint ?? Color.white;
            m.color = c;
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            return m;
        }

        public static Material Cutout(Texture tex)
        {
            var m = new Material(CutoutSrc);
            if (tex != null) m.mainTexture = tex;
            m.color = Color.white;
            if (m.HasProperty("_Color")) m.SetColor("_Color", Color.white);
            return m;
        }

        public static GameObject Box(string name, Vector3 size, Color color, Transform parent, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = Lit(color);
            if (!collider) StripCollider(go);
            return go;
        }

        public static void StripCollider(GameObject go)
        {
            if (go == null) return;
            var col = go.GetComponent<Collider>();
            if (col == null) return;
            if (Application.isPlaying) Object.Destroy(col);
            else Object.DestroyImmediate(col);
        }

        public static GameObject Cylinder(string name, Vector3 size, Color color, Transform parent, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = Lit(color);
            if (!collider) StripCollider(go);
            return go;
        }

        public static GameObject Floor(string name, float width, float depth, Transform parent, Color color, Texture tex = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
            go.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            go.transform.localScale = new Vector3(width, depth, 1f);
            StripCollider(go);
            var mr = go.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = true;
            var mat = tex != null ? Textured(tex, Color.white) : Lit(color);
            mat.renderQueue = 1990;
            mr.sharedMaterial = mat;

            var colGo = new GameObject("FloorCollider");
            colGo.transform.SetParent(parent, false);
            colGo.transform.localPosition = new Vector3(0f, -0.06f, 0f);
            var box = colGo.AddComponent<BoxCollider>();
            box.size = new Vector3(width, 0.1f, depth);
            return go;
        }

        public static GameObject Backdrop(string name, Texture tex, float width, float height, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            StripCollider(go);
            go.transform.localScale = new Vector3(width, height, 1f);
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = tex != null ? Textured(tex) : Lit(new Color(0.4f, 0.42f, 0.45f));
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        public static GameObject Pawn(string name, Sprite portrait, Color bodyColor, Transform parent, float height = 1.8f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            body.transform.localScale = new Vector3(0.48f, height * 0.5f, 0.48f);
            body.GetComponent<MeshRenderer>().sharedMaterial = Lit(bodyColor);
            StripCollider(body);

            var stripe = Box("Stripe", new Vector3(0.52f, 0.12f, 0.52f), KitYellow, root.transform, false);
            stripe.transform.localPosition = new Vector3(0f, height * 0.72f, 0f);

            if (portrait != null && portrait.texture != null)
            {
                var card = GameObject.CreatePrimitive(PrimitiveType.Quad);
                card.name = "Portrait";
                card.transform.SetParent(root.transform, false);
                StripCollider(card);
                var aspect = (float)portrait.texture.width / Mathf.Max(1, portrait.texture.height);
                var h = 0.72f;
                card.transform.localScale = new Vector3(h * Mathf.Clamp(aspect, 0.6f, 1.15f), h, 1f);
                card.transform.localPosition = new Vector3(0f, height * 0.62f, 0.28f);
                card.GetComponent<MeshRenderer>().sharedMaterial = Cutout(portrait.texture);
                card.AddComponent<BillboardFacing>();
            }

            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, height * 0.5f, 0f);
            trigger.size = new Vector3(0.9f, height, 0.9f);
            return root;
        }

        public static GameObject PlayerVisual(string name, Sprite sprite, Transform parent)
        {
            return PaintedFigure(name, sprite, parent, 1.9f);
        }

        public static GameObject PaintedFigure(string name, Sprite sprite, Transform parent, float height = 1.9f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);

            var shadow = Cylinder("Shadow", new Vector3(0.55f, 0.02f, 0.55f), new Color(0.08f, 0.07f, 0.06f, 0.55f), root.transform, false);
            shadow.transform.localPosition = new Vector3(0f, 0.02f, 0f);

            var card = GameObject.CreatePrimitive(PrimitiveType.Quad);
            card.name = "Paint";
            card.transform.SetParent(root.transform, false);
            StripCollider(card);
            var aspect = 0.55f;
            if (sprite != null && sprite.texture != null)
            {
                aspect = (float)sprite.texture.width / Mathf.Max(1, sprite.texture.height);
                card.GetComponent<MeshRenderer>().sharedMaterial = Cutout(sprite.texture);
            }
            else
            {
                card.GetComponent<MeshRenderer>().sharedMaterial = Lit(KitYellow);
            }

            var w = height * Mathf.Clamp(aspect, 0.38f, 0.72f);
            card.transform.localScale = new Vector3(w, height, 1f);
            card.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            card.AddComponent<BillboardFacing>();

            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, height * 0.5f, 0f);
            trigger.size = new Vector3(0.7f, height, 0.55f);
            return root;
        }
    }
}
