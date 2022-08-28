using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
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

        public static AssetBundle asyncBundle;

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
        }
        
        public static void UpdateWorldsList()
        {
            worldsList.Clear();

            int count = 0;

            foreach (string world in Directory.GetDirectories(levelsPath))
            {
                if (count >= maxWorlds)
                {
                    break;
                }

                worldsList.Add(new Tuple<string, List<string>>(world, Directory.GetFiles(world).ToList()));

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
