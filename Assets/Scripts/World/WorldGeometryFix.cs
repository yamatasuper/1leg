using NinetyMinutes.Art;
using UnityEngine;

namespace NinetyMinutes.World
{
    /// <summary>
    /// Keeps locations apart and puts location photos back on the floor.
    /// </summary>
    public static class WorldGeometryFix
    {
        public static void Stabilize(GameObject locker, GameObject street)
        {
            if (street != null && Mathf.Abs(street.transform.position.x) < 24f)
                street.transform.position = new Vector3(48f, street.transform.position.y, street.transform.position.z);

            PaintSurface(locker, "Floor", ArtCatalog.Tex(ArtCatalog.LocationLocker), new Color(0.42f, 0.32f, 0.2f));
            PaintSurface(street, "Pitch",
                ArtCatalog.Tex(ArtCatalog.LocationPitch) ?? ArtCatalog.Tex(ArtCatalog.LocationStreet),
                new Color(0.34f, 0.4f, 0.26f));
            PaintSurface(locker, "LockerArt", ArtCatalog.Tex(ArtCatalog.LocationLocker), new Color(0.45f, 0.4f, 0.32f));
            PaintSurface(street, "StadiumArt", ArtCatalog.Tex(ArtCatalog.LocationStreet), new Color(0.4f, 0.42f, 0.38f));
            Raise(street, "Sideline", 0.14f);
        }

        public static void OpenExits(GameObject locker, GameObject street)
        {
            DisableNamed(locker, "DoorBlockW");
            DisableNamed(street, "DoorBlockE");
            // Trigger reaches into the room so the doorway can be used by walking, and the
            // arrival spawn sits clear of the opposite trigger.
            SetupDoor(locker, "door_to_street", new Vector3(0.7f, 0.2f, 0f), new Vector3(2.4f, 2.6f, 2.2f),
                "loc_street", new Vector2(2f, -1.5f));
            SetupDoor(street, "door_to_locker", new Vector3(-0.7f, 0.2f, 0f), new Vector3(2.4f, 2.6f, 2.2f),
                "loc_locker", new Vector2(-3.5f, -1.2f));
        }

        static void DisableNamed(GameObject root, string name)
        {
            if (root == null) return;
            var t = Find(root.transform, name);
            if (t == null) return;
            t.gameObject.SetActive(false);
        }

        static void SetupDoor(GameObject root, string name, Vector3 center, Vector3 size, string target, Vector2 spawn)
        {
            if (root == null) return;
            var t = Find(root.transform, name);
            if (t == null) return;

            var box = t.GetComponent<BoxCollider>();
            if (box == null) box = t.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = center;
            box.size = size;

            var door = t.GetComponent<DoorInteractable>();
            if (door == null) door = t.gameObject.AddComponent<DoorInteractable>();
            door.TargetLocationId = target;
            door.TargetSpawn = spawn;
            door.RequireFlag = null;
            door.LockedLine = null;
            door.Prompt = target == "loc_street" ? "E — выйти на бровку" : "E — вернуться в раздевалку";
        }

        static void PaintSurface(GameObject root, string name, Texture tex, Color fallback)
        {
            if (root == null) return;
            var t = Find(root.transform, name);
            if (t == null) return;

            var deck = t.Find(name + "_Deck");
            if (deck != null)
            {
                if (Application.isPlaying) Object.Destroy(deck.gameObject);
                else Object.DestroyImmediate(deck.gameObject);
            }

            var mr = t.GetComponent<MeshRenderer>();
            if (mr == null) return;
            mr.enabled = true;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var mat = tex != null ? WorldSprites.Textured(tex, Color.white) : WorldSprites.Lit(fallback);
            mat.renderQueue = 1990;
            mr.sharedMaterial = mat;
        }

        static void Raise(GameObject root, string name, float y)
        {
            if (root == null) return;
            var t = Find(root.transform, name);
            if (t == null) return;
            var p = t.localPosition;
            t.localPosition = new Vector3(p.x, y, p.z);
            var mr = t.GetComponent<MeshRenderer>();
            if (mr != null && (mr.sharedMaterial == null || mr.sharedMaterial.name.StartsWith("Default")))
                mr.sharedMaterial = WorldSprites.Lit(new Color(0.5f, 0.46f, 0.38f));
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
