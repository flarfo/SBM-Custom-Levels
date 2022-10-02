using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace SBM_CustomLevels
{
    internal class RecordLevel : MonoBehaviour
    {
        static string baseResourcesPath = Path.Combine("prefabs", "level");

        #region data
        //prefabs stored in the Resources/prefabs/level folder
        static List<string> levelContents = new List<string>{ "Carrot", "CarrotDestroyer", "CarrotGrabBasket", "CarrotSpawner", "Cloud Spawner W2", "Cloud Spawner", "EasterEgg",
        "KillBounds", "RotatingSwing", "SecretCat", "Signpost_Arrow", "Signpost_Death", "Signpost_Text", "Signpost_UI_Message", "Spikes", "SpringBoard", "TimeAttackGhost", "Wormhole"};

        //prefabs stored in the Resources/prefabs/level/world1 folder
        static List<string> world1Contents = new List<string>{ "Block_W1", "Block_W1_1x1Beveled", "Block_W1_Corner", "Cloud_W1", "DetailRock", "Fence", "Flower_Blue", "Flower_Orange",
        "Flower_Purple", "Flower_Red", "Grass", "Grass_Angleable", "Grass_Corner_1x1_45deg_Left", "Grass_Corner_1x1_45deg_Left_Endcap", "Grass_Corner_1x1_45deg_Right",
        "Grass_Corner_1x1_45deg_Right_Endcap", "Grass_Endcap_Left", "Grass_Endcap_Right", "Grass_MidBlock", "Grass_Single_1x1", "MushroomShort", "MushroomTall", "Tree_A",
        "Tree_B", "Tree_Static", "TreeLog", "World1BackgroundPrototype", "WorldBG_1" }; //27

        //prefabs stored in the Resources/prefabs/level/world2 folder
        static List<string> world2Contents = new List<string>{ "Block_W2", "Block_W2_Corner", "Cloud_W2", "Daffodil", "DetailRock", "Grass_Winter", "IceBlockBottomPanel", "IceBlockCorner_LD",
        "IceBlockCorner_LU", "IceBlockCorner_RD", "IceBlockCorner_RU", "IceBlockEdgeLeft", "IceBlockEdgeRight", "IceBlockMidPanel", "IceBlockMidPanel_Left",
        "IceBlockMidPanel_Right", "IceBlockSection", "IceBlockTopPanel", "IceCorner", "IceCorner_Midpanel", "IceCube", "IceQuarterCircle", "IceQuarterPipe",
        "IceSled_1x3", "IceSled_1x4", "IceSled_1x5", "IceSledSpikesGuide", "Mountains", "Rock_EdgeDetail", "Signpost_Arrow_Snow", "Signpost_Death_Snow", "Signpost_Snow",
        "Snow_Angleable", "Snow_Corner_1x1_45deg_Left", "Snow_Corner_1x1_45deg_Left_Endcap", "Snow_Corner_1x1_45deg_Right", "Snow_Corner_1x1_45deg_Right_Endcap",
        "Snow_Endcap_Left", "Snow_Endcap_Right", "Snow_MidBlock", "Snow_Single_1x1", "Snowball", "Snowman", "WinterPlant", "WinterTree", "World2Background_3DTest",
        "World_2_BG" }; //38

        //prefabs stored in the Resources/prefabs/level/world3 folder
        static List<string> world3Contents = new List<string>{ "Block_W3", "Block_W3_Corner", "Boulder", "Boulder_Kickable", "Boulder_Spawned", "BoulderDestroyer", "BoulderRollable",
        "BoulderSpawner", "Cave_Endcap_1x1", "Cave_Endcap_Left", "Cave_Endcap_Right", "Cave_Midblock", "CaveRock", "CaveRock_small", "CaveRockCrystals", "Crystal_Blue",
        "Crystal_Green", "Crystal_Orange", "Crystal_Red", "Minecart", "Minecart_Static", "MinecartRail", "MinecartRail_Sleeper", "MinecartRail_Sleeper_SplineTile",
        "MineLamp", "Mineshaft_Support", "Mineshaft_Support_NoLamp", "Mineshaft_Support_Unlit", "Pickaxe", "Stalagmite", "World3_BG", "World3_Block_1x1_Bevel", "RailSleeper" }; //RailSleeper = MinecartRail_Sleeper

        //prefabs stored in the Resources/prefabs/level/world4 folder
        static List<string> world4Contents = new List<string>{ "BarrelRaft_2x1", "BarrelRaft_2x2", "BarrelRaft_2x4", "BarrelRaft_Round", "BarrelRaftIsland_LShape_A", "BarrelRaftIsland_LShape_B",
        "Beachball", "BeachBarrel", "BeachPlant", "BeachQuarterCircle", "BeachQuarterCircle_Cave", "BeachQuarterPipe", "BeachQuarterPipe_Cave", "BeachRock", "BeachRock_RB",
        "Block_W4", "Block_W4_1x1_CurvedBottom", "Block_W4_Corner", "Boardwalk", "Coconut", "Coral", "DetailRock", "FloatingPlatform", "FloatingPlatform_Small",
        "GrassBeach_Corner_1x1_45deg_Right_Endcap", "GrassBeach_Endcap_1x1", "GrassBeach_Endcap_Left", "GrassBeach_Endcap_Right", "GrassBeach_MidBlock", "HibiscusBush",
        "JetSki", "LevelLight_World4", "PalmTree", "Parasol", "SeaLeaf", "SeaMine", "Seashell", "Seaweed", "TreasureChest", "Water", "World4_BG" };

        //prefabs stored in the Resources/prefabs/level/world5 folder
        static List<string> world5Contents = new List<string>{ "Billboard", "BumperBar", "BumperBarSpinWheel", "BumperPad", "BumperPadJumbo", "BumperSpinner", "BumperSpinnerJumbo",
        "BumperSpinnerPlatform", "BurningPlatform", "Crowd", "CrowdSingle", "DirtBike", "FireLightPrefab", "HopBars", "HotdogKart", "LevelBlock_W5", "Masher", "RingOfFire",
        "Rope", "RopeSwingPendulum", "CrossPipe", "ScaffoldForwardPipeRemover", "Scaffolding", "ScaffoldPanel", "ScaffoldPanel_RB", "ScaffoldPanelDestroyer",
        "ScaffoldPanelSpline", "ScaffoldPipeExtra", "ScaffoldPipeExtraDouble", "ScaffoldPipeSpline", "SparkShower", "Spotlight", "SwingingPlatform", "TrackingSpotlight",
        "World5_BG", "WorldBG_5_old" }; //may not work with scaffolding, objects are inside another folder ("scaffolding")
        #endregion

        public static void RecordJSONLevel()
        {
            if (!Directory.Exists(LevelLoader_Mod.levelsPath))
            {
                Directory.CreateDirectory(LevelLoader_Mod.levelsPath);
            }

            WriteJSON(FindObjectsOfType(typeof(GameObject)) as GameObject[]);
        }

        static void WriteJSON(GameObject[] objects)
        {
            List<string> jsonList = new List<string>();

            jsonList.Add(JsonUtility.ToJson(new FloatObject(GameObject.Find("PlayerSpawn_1"))));
            jsonList.Add(JsonUtility.ToJson(new FloatObject(GameObject.Find("PlayerSpawn_2"))));

            for (int i = 0; i < objects.Length; i++)
            {
                string objectName = NameToPath(objects[i].name);

                if (objectName != string.Empty)
                {
                    jsonList.Add(JsonUtility.ToJson(new JsonObject(objects[i])));
                }
            }

            string filePath = Path.Combine(LevelLoader_Mod.levelsPath, EditorManager.instance.selectedLevel);

            File.WriteAllLines(filePath, jsonList);
        }

        public static string NameToPath(string goName)
        {
            string path;

            if (goName.Contains("("))
            {
                goName = goName.Remove(goName.IndexOf("(")).Trim();
            }

            if (levelContents.Contains(goName))
            {
                path = baseResourcesPath;
            }
            else if (world1Contents.Contains(goName))
            {
                path = Path.Combine(baseResourcesPath, "world1");
            }
            else if (world2Contents.Contains(goName))
            {
                path = Path.Combine(baseResourcesPath, "world2");
            }
            else if (world3Contents.Contains(goName))
            {
                if (goName == "RailSleeper")
                {
                    goName = "MinecartRail_Sleeper";
                }

                path = Path.Combine(baseResourcesPath, "world3");
            }
            else if (world4Contents.Contains(goName))
            {
                path = Path.Combine(baseResourcesPath, "world4");
            }
            else if (world5Contents.Contains(goName))
            {
                if (goName.Contains("Scaffold") || goName.Contains("CrossPipe"))
                {
                    path = Path.Combine(baseResourcesPath, "world5", "scaffolding");
                }
                else
                {
                    path = Path.Combine(baseResourcesPath, "world5");
                }
                
            }
            else return "";

            return Path.Combine(path, goName);
        }

        [Serializable]
        public class JsonObject
        {
            public string objectName;

            public float[] position = new float[3];
            public float[] rotation = new float[3];
            public float[] scale = new float[3];


            public JsonObject(GameObject gameObject)
            {
                objectName = NameToPath(gameObject.name);

                position[0] = gameObject.transform.position.x;
                position[1] = gameObject.transform.position.y;
                position[2] = gameObject.transform.position.z;

                rotation[0] = gameObject.transform.rotation.eulerAngles.x;
                rotation[1] = gameObject.transform.rotation.eulerAngles.y;
                rotation[2] = gameObject.transform.rotation.eulerAngles.z;

                scale[0] = gameObject.transform.localScale.x;
                scale[1] = gameObject.transform.localScale.y;
                scale[2] = gameObject.transform.localScale.z;
            }
        }
    }
}
