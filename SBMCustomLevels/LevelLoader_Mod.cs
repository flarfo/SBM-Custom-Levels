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

        public static List<World> worldsList = new List<World>();

        public static readonly int maxLevels = 10;
        public static readonly int maxWorlds = 16;

        public static Material skyboxWorld1;
        public static Material skyboxWorld2;
        public static Material skyboxWorld3;
        public static Material skyboxWorld4;
        public static Material skyboxWorld5;

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
            skyboxWorld5 = sbmBundle.LoadAsset<Material>("Skybox_World5");

            EditorManager.fakeWater = sbmBundle.LoadAsset<GameObject>("Water_W4");
            EditorManager.fakeWater.AddComponent<WaterDataContainer>();

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
            foreach (string worldPath in Directory.GetDirectories(levelsPath).OrderBy(p => new DirectoryInfo(p).CreationTime))
            {
                if (count == maxWorlds)
                {
                    break;
                }

                //List<string> levels = Directory.GetFiles(world, "*.sbm").ToList();
                string worldName = new DirectoryInfo(worldPath).Name;

                worldsList.Add(new World(worldName));

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

    public sealed class World
    {
        public List<string> levels = new List<string>();
        public string worldPath;

        private string _name;
        public string Name
        {
            get { return _name; }

            set
            {
                _name = value;

                _worldHash = GetHashFromName();
            }
        }

        private ulong _worldHash;
        public ulong WorldHash 
        {
            get { return _worldHash; }
        }

        public World(string name)
        {
            Name = name;
            worldPath = Path.Combine(LevelLoader_Mod.levelsPath, name);
            UpdateLevels();
        }

        public void UpdateLevels()
        {
            levels = Directory.GetFiles(worldPath, "*.sbm").ToList();
        }

        private ulong GetHashFromName()
        {
            string hash = "";
            char[] name;

            if (Name.Length > 18)
            {
                name = Name.Take(18).ToArray();
            }
            else
            {
                name = Name.ToArray();
            }

            // convert name to list of integers
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                int value;

                if (char.IsLetter(c))
                {
                    // convert character to its alphabetical position 'A' = 0, 'B' = 1, ...
                    value = char.ToUpper(c) - 65;
                    // if greater than 9, overflow back to 0, since 9 is largest single digit integer.
                    value %= 9;
                }
                else if (char.IsDigit(c))
                {
                    value = (int)char.GetNumericValue(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    value = 0;
                }
                else if (char.IsSymbol(c))
                {
                    // INVALID: <>:"/\|?*
                    // use most common symbols for unique number identities
                    switch(c)
                    {
                        case '!':
                            value = 1;
                            break;
                        case '_':
                            value = 1;
                            break;
                        case '#':
                            value = 2;
                            break;
                        case '$':
                            value = 3;
                            break;
                        case '+':
                            value = 4;
                            break;
                        case '^':
                            value = 5;
                            break;
                        case '&':
                            value = 6;
                            break;
                        case '(':
                            value = 7;
                            break;
                        case ')':
                            value = 8;
                            break;
                        case '-':
                            value = 9;
                            break;
                        default:
                            continue;
                    }
                }
                else
                {
                    continue;
                }
                hash += value;
            }

            // add 'salt' to ensure numerically equivalent names don't produce the same hash. 
            ulong total = 0;
            for (int i = 0; i < hash.Length; i++)
            {
                total += (ulong)char.GetNumericValue(hash[i]);
            }

            ulong final = ulong.Parse(hash) + total;

            if (final > ulong.MaxValue)
            {
                final = ulong.MaxValue - total;
            }

            return final;
        }
    }
}
