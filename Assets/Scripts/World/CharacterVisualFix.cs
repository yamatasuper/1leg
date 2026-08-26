using NinetyMinutes.Art;
using UnityEngine;

namespace NinetyMinutes.World
{
    /// <summary>
    /// Hides capsule "sausages" and puts Disco-style painted billboards on characters.
    /// </summary>
    public static class CharacterVisualFix
    {
        public static void Dress(PlayerController player, GameObject locker, GameObject street)
        {
            if (player != null)
                DressRoot(player.transform, ArtCatalog.SpritePlayer ?? ArtCatalog.PortraitBardin);

            DressNamed(locker, "npc_coach", ArtCatalog.SpriteCoach ?? ArtCatalog.PortraitCoach);
            DressNamed(street, "npc_glock", ArtCatalog.SpriteGlock ?? ArtCatalog.PortraitGlock);
            DressNamed(street, "npc_sokol", ArtCatalog.SpriteSokol ?? ArtCatalog.PortraitSokol);
            DressNamed(street, "self_thought", ArtCatalog.SpritePlayer ?? ArtCatalog.PortraitBardin);
        }

        static void DressNamed(GameObject root, string name, Sprite sprite)
        {
            if (root == null) return;
            var t = Find(root.transform, name);
            if (t != null) DressRoot(t, sprite);
        }

        static void DressRoot(Transform root, Sprite sprite)
        {
            if (root == null) return;
            HideMesh(root, "Body");
            HideMesh(root, "Stripe");

            var paint = Find(root, "Paint");
            if (paint == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name = "Paint";
                go.transform.SetParent(root, false);
                WorldSprites.StripCollider(go);
                paint = go.transform;
            }

            var tex = sprite != null ? sprite.texture : null;
            var aspect = 0.55f;
            if (tex != null)
                aspect = (float)tex.width / Mathf.Max(1, tex.height);

            const float height = 1.9f;
            var w = height * Mathf.Clamp(aspect, 0.38f, 0.78f);
            paint.localPosition = new Vector3(0f, height * 0.5f, 0f);
            paint.localScale = new Vector3(w, height, 1f);
            paint.localRotation = Quaternion.identity;

            var mr = paint.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = true;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.sharedMaterial = tex != null
                    ? WorldSprites.Cutout(tex)
                    : WorldSprites.Lit(WorldSprites.KitYellow);
            }

            if (paint.GetComponent<BillboardFacing>() == null)
                paint.gameObject.AddComponent<BillboardFacing>();
        }

        static void HideMesh(Transform root, string name)
        {
            var t = Find(root, name);
            if (t == null) return;
            foreach (var r in t.GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }

        static Transform Find(Transform root, string name)
        {
            if (root == null) return null;
            if (string.Equals(root.name, name, System.StringComparison.OrdinalIgnoreCase))
                return root;
            foreach (Transform child in root)
            {
                var found = Find(child, name);
                if (found != null) return found;
            }

            return null;
        }
    }
}
