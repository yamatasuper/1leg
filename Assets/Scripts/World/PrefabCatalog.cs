using UnityEngine;

namespace NinetyMinutes.World
{
    public static class PrefabCatalog
    {
        public const string Root = "Prefabs/";

        public static bool ForceBuildFromParts;

        public static GameObject Player => Load("Characters/Player");
        public static GameObject NpcCoach => Load("Characters/Npc_Coach");
        public static GameObject NpcGlock => Load("Characters/Npc_Glock");
        public static GameObject NpcSokol => Load("Characters/Npc_Sokol");
        public static GameObject NpcSelf => Load("Characters/Npc_Self");
        public static GameObject SkipCrate => Load("Props/Skip_Training");
        public static GameObject DoorToStreet => Load("Props/Door_ToStreet");
        public static GameObject DoorToLocker => Load("Props/Door_ToLocker");
        public static GameObject LocLocker => Load("Locations/Loc_Locker");
        public static GameObject LocStreet => Load("Locations/Loc_Street");
        public static GameObject WorldPersistent => Load("World/World_Persistent");

        public static GameObject Spawn(GameObject prefab, Transform parent)
        {
            if (ForceBuildFromParts || prefab == null) return null;
            var go = Object.Instantiate(prefab, parent);
            if (go.name.EndsWith("(Clone)"))
                go.name = go.name.Substring(0, go.name.Length - 7);
            return go;
        }

        static GameObject Load(string path)
        {
            return Resources.Load<GameObject>(Root + path);
        }
    }
}
