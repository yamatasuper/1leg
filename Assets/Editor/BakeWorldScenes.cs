using System.IO;
using NinetyMinutes.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NinetyMinutes.EditorTools
{
    public static class BakeWorldScenes
    {
        const string PersistentPath = "Assets/Scenes/World_Persistent.unity";
        const string LockerPath = "Assets/Scenes/Loc_Locker.unity";
        const string StreetPath = "Assets/Scenes/Loc_Street.unity";

        [InitializeOnLoadMethod]
        static void BakeIfMissing()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying) return;
                if (File.Exists(PersistentPath) && File.Exists(LockerPath) && File.Exists(StreetPath)
                    && HasContent(LockerPath))
                    return;
                try
                {
                    Bake();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[90 минут] Bake World Scenes failed: " + e);
                }
            };
        }

        [MenuItem("90 минут/Bake World Scenes")]
        public static void Bake()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[90 минут] Stop Play Mode before baking scenes.");
                return;
            }

            var prev = EditorSceneManager.GetActiveScene();
            try
            {
                BakeOne(PersistentPath, WorldSceneFactory.BuildPersistent);
                BakeOne(LockerPath, WorldSceneFactory.BuildLocker);
                BakeOne(StreetPath, WorldSceneFactory.BuildStreet);
                EnsureBuildSettings();
                AssetDatabase.Refresh();
                Debug.Log("[90 минут] World scenes baked: World_Persistent, Loc_Locker, Loc_Street.");
            }
            finally
            {
                if (prev.IsValid())
                    EditorSceneManager.SetActiveScene(prev);
            }
        }

        static bool HasContent(string path)
        {
            if (!File.Exists(path)) return false;
            var text = File.ReadAllText(path);
            return text.Contains("LocationScene") || text.Contains("loc_locker") || text.Contains("World_Persistent");
        }

        static void BakeOne(string path, System.Func<GameObject> build)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);
            build();
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);
        }

        static void EnsureBuildSettings()
        {
            var boot = "Assets/Scenes/SampleScene.unity";
            var wanted = new[] { boot, PersistentPath, LockerPath, StreetPath };
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var p in wanted)
            {
                if (!File.Exists(p)) continue;
                list.Add(new EditorBuildSettingsScene(p, true));
            }

            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
