using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

namespace SBM_CustomLevels
{
    [BepInPlugin("flarfo.sbm.level_loader", "SBM_LevelLoader", "0.0.1")]
    public class LevelLoader_Mod : BaseUnityPlugin
    {
        private readonly string pluginGUID = "flarfo.sbm.level_loader";
        private readonly string pluginName = "SBM_LevelLoader";
        private readonly string pluginVersion = "0.0.1";

        public static string levelsPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "levels");

        public static List<Tuple<string, List<string>>> worldsList = new List<Tuple<string, List<string>>>();

        public static readonly int maxLevels = 10;
        public static readonly int maxWorlds = 16;

        public static Material skyboxWorld1;
        public static Material skyboxWorld2;
        public static Material skyboxWorld3;
        public static Material skyboxWorld4;

        public static Material colliderRenderMat;

        //initialize plugin and start harmony
        private void Awake()
        {
            Harmony harmony = new Harmony(pluginName);

            harmony.PatchAll();

            Logger.LogInfo($"Plugin {pluginName} is loaded!");

            if (!Directory.Exists(levelsPath))
            {
                Directory.CreateDirectory(levelsPath);
            }

            UpdateWorldsList();

            GameObject levelManager = new GameObject("LevelManager", typeof(LevelManager));
            levelManager.hideFlags = HideFlags.HideAndDontSave;

            GameObject editorManager = new GameObject("LevelEditor", typeof(EditorManager));
            editorManager.hideFlags = HideFlags.HideAndDontSave;

            GameObject menuManager = new GameObject("MenuManager", typeof(MenuManager));
            menuManager.hideFlags = HideFlags.HideAndDontSave;

            AssetBundle sbmBundle = GetAssetBundleFromResources("sbm-bundle");

            skyboxWorld1 = sbmBundle.LoadAsset<Material>("Skybox_World1");
            skyboxWorld2 = sbmBundle.LoadAsset<Material>("Skybox_World2");
            skyboxWorld3 = sbmBundle.LoadAsset<Material>("Skybox_World3");
            skyboxWorld4 = sbmBundle.LoadAsset<Material>("Skybox_World4");

            EditorManager.fakeWater = sbmBundle.LoadAsset<GameObject>("Water");
            EditorManager.fakeWater.AddComponent<FakeWater>();

            EditorManager.iceSledSpikesGuide = sbmBundle.LoadAsset<GameObject>("IceSledSpikesGuide");
            EditorManager.playerSpawn = sbmBundle.LoadAsset<GameObject>("PlayerSpawn");

            EditorManager.outlineMask = sbmBundle.LoadAsset<Material>("OutlineMask");
            EditorManager.outlineFill = sbmBundle.LoadAsset<Material>("OutlineFill");

            MinecartRailHelper.railSplineTile = sbmBundle.LoadAsset<Mesh>("RailSplineTile");
            MinecartRailHelper.railMaterial = sbmBundle.LoadAsset<Material>("MinecartRail");
            MinecartRailHelper.railNodeHandle = sbmBundle.LoadAsset<GameObject>("RailNode");

            sbmBundle.Unload(false);
        }
        
        public static void UpdateWorldsList()
        {
            worldsList.Clear();

            int count = 0;

            // order by creation time, a bad partial fix for alphabetical not being consistent when new worlds added
            string[] worldDirectories = Directory.GetDirectories(levelsPath).OrderBy(p => new DirectoryInfo(p).CreationTime).ToArray();

            foreach (string world in worldDirectories)
            {
                if (count == maxWorlds)
                {
                    break;
                }

                List<string> levels = Directory.GetFiles(world, "*.sbm").ToList();
                worldsList.Add(new Tuple<string, List<string>>(world, levels));

                count++;
            }
        }

        /// <summary>
        /// Returns AssetBundle from embedded resources based on the given filename.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static AssetBundle GetAssetBundleFromResources(string fileName)
        {
            var execAssembly = Assembly.GetExecutingAssembly();

            var resourceName = execAssembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(fileName));

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }

        /// <summary>
        /// Returns AssetBundle from file location (bundle must be located in same folder as plugin).
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public static AssetBundle GetAssetBundleFromFile(string bundleName)
        {
            string path = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), bundleName);

            AssetBundle bundle = null;

            try
            {
                bundle = AssetBundle.LoadFromFile(path);
            }
            catch
            {
                Debug.LogError("Loading AssetBundle from file failed!");
            }

            return bundle;
        }
    }
}
