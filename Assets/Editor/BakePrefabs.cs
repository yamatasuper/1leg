using System.IO;
using NinetyMinutes.Art;
using NinetyMinutes.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NinetyMinutes.EditorTools
{
    public static class BakePrefabs
    {
        const string Characters = "Assets/Resources/Prefabs/Characters";
        const string Locations = "Assets/Resources/Prefabs/Locations";
        const string Props = "Assets/Resources/Prefabs/Props";
        const string World = "Assets/Resources/Prefabs/World";

        [InitializeOnLoadMethod]
        static void BakeIfMissing()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying) return;
                if (File.Exists(Characters + "/Player.prefab") && File.Exists(Locations + "/Loc_Locker.prefab"))
                    return;
                try
                {
                    Bake();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[90 минут] Bake Prefabs failed: " + e);
                }
            };
        }

        [MenuItem("90 минут/Bake Prefabs")]
        public static void Bake()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[90 минут] Stop Play Mode before baking prefabs.");
                return;
            }

            Directory.CreateDirectory(Characters);
            Directory.CreateDirectory(Locations);
            Directory.CreateDirectory(Props);
            Directory.CreateDirectory(World);

            var prev = EditorSceneManager.GetActiveScene();
            PrefabCatalog.ForceBuildFromParts = true;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);
            try
            {
                Save(WorldSceneFactory.BuildPlayerPrefab(), Characters + "/Player.prefab");
                Save(WorldSceneFactory.BuildNpcPrefab("npc_coach", ArtCatalog.SpriteCoach ?? ArtCatalog.PortraitCoach,
                    "npc_coach", "E — говорить с тренером", "training_done", "Тренировка уже позади."),
                    Characters + "/Npc_Coach.prefab");
                Save(WorldSceneFactory.BuildNpcPrefab("npc_glock", ArtCatalog.SpriteGlock ?? ArtCatalog.PortraitGlock,
                    "npc_glock", "E — говорить с Глоком", "street_glock_done", "С Глоком уже поговорили."),
                    Characters + "/Npc_Glock.prefab");
                Save(WorldSceneFactory.BuildNpcPrefab("npc_sokol", ArtCatalog.SpriteSokol ?? ArtCatalog.PortraitSokol,
                    "npc_sokol", "E — говорить с Соколом", "street_sokol_done", "С Соколом уже поговорили."),
                    Characters + "/Npc_Sokol.prefab");
                Save(WorldSceneFactory.BuildNpcPrefab("self_thought", ArtCatalog.SpritePlayer ?? ArtCatalog.PortraitBardin,
                    "self_thought", "E — остаться с собой", "street_self_done", "Этот разговор уже был."),
                    Characters + "/Npc_Self.prefab");
                Save(WorldSceneFactory.BuildSkipPrefab(), Props + "/Skip_Training.prefab");
                Save(WorldSceneFactory.BuildDoorPrefab("door_to_street", "loc_street", new Vector2(6.5f, 0f),
                    "E — на бровку", new Color(0.35f, 0.28f, 0.18f)), Props + "/Door_ToStreet.prefab");
                Save(WorldSceneFactory.BuildDoorPrefab("door_to_locker", "loc_locker", new Vector2(-5.2f, -1.2f),
                    "E — в раздевалку", new Color(0.38f, 0.3f, 0.18f)), Props + "/Door_ToLocker.prefab");

                Save(WorldSceneFactory.BuildLocker(), Locations + "/Loc_Locker.prefab");
                Save(WorldSceneFactory.BuildStreet(), Locations + "/Loc_Street.prefab");
                Save(WorldSceneFactory.BuildPersistent(), World + "/World_Persistent.prefab");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[90 минут] Prefabs baked under Assets/Resources/Prefabs/");
            }
            finally
            {
                PrefabCatalog.ForceBuildFromParts = false;
                EditorSceneManager.CloseScene(scene, true);
                if (prev.IsValid())
                    EditorSceneManager.SetActiveScene(prev);
            }
        }

        static void Save(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
    }
}
