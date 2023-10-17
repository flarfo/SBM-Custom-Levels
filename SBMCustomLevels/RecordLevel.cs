using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SBM_CustomLevels.ObjectWrappers;
using SBM_CustomLevels.Editor;

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
        "JetSki", "LevelLight_World4", "PalmTree", "Parasol", "SeaLeaf", "SeaMine", "Seashell", "Seaweed", "TreasureChest", "Water_W4", "World4_BG" };

        //prefabs stored in the Resources/prefabs/level/world5 folder
        static List<string> world5Contents = new List<string>{ "BalanceBall", "BumperBar", "BumperBarSpinWheel", "BumperPad", "BumperPadJumbo", "BumperSpinner", "BumperSpinnerJumbo",
        "BumperSpinnerPlatform", "ConveyorBelt", "DirtBike", "FireLightPrefab", "Flamethrower", "Flamethrowers_Decorative", "FlipBlock", "FloppyObstacle", "FloppyRod", 
        "HalfCircleSpongeyBlock", "HopBars", "HotdogKart", "HotdogKart_Rocket Variant", "PistonPlatform", "Pivot", "RingOfFire", "SeeSaw", "Signpost_W5_Arrow",
        "Signpost_W5_Death", "SlipNSlide", "CrossPipe", "ScaffoldForwardPipeRemover", "Scaffolding", "ScaffoldPanel", "ScaffoldPanel_RB", "ScaffoldPanelDestroyer",
        "ScaffoldPanelSpline", "ScaffoldPipeExtra", "ScaffoldPipeExtraDouble", "ScaffoldPipeSpline", "SparkShower", "Spotlight", "Spotlight_SurfaceMount", "SteamMachine", "StiffRod", "SupportPole", 
        "TrackingSpotlight", "WaterTank", "World5_BG" }; //may not work with scaffolding, objects are inside another folder ("scaffolding")

        //prefabs stored in the Resources/prefabs/level/basketball folder
        static List<string> basketallContents = new List<string> { "BasketBall", "BasketballHoop" };

        //prefabs stored in the Resources/prefabs/items folder
        static List<string> itemContents = new List<string> { "PickupSpawnPoint", "PickupTrigger_Carrot", "PickupTrigger_Jetpack", "PickupTrigger_MagnetGloves", 
        "PickupTrigger_Rollerskates", "PickupTrigger_UnicornHorn" };

        static List<string> customContents = new List<string> { "SplineObject", "ScaffoldingBlock", "ScaffoldingCorner", "ScaffoldPanelBlack", "ScaffoldPanelBrown", "ColorBlock",
        "ColorBlockCorner", };
        #endregion

        public static void RecordJSONLevel()
        {
            if (!Directory.Exists(LevelLoader_Mod.levelsPath))
            {
                Directory.CreateDirectory(LevelLoader_Mod.levelsPath);
            }

            //TODO: add try/catch to prevent saving with errors
            var objects = FindObjectsOfType(typeof(EditorSelectable)).Cast<EditorSelectable>().ToList();
            WriteJSON(objects);
        }

        static void WriteJSON(List<EditorSelectable> objects)
        {
            int worldStyle = EditorManager.instance.worldStyle;

            var p1 = GameObject.Find("PlayerSpawn_1");
            var p2 = GameObject.Find("PlayerSpawn_2");
            var p3 = GameObject.Find("PlayerSpawn_3");
            var p4 = GameObject.Find("PlayerSpawn_4");

            FloatObject spawnPos1;
            FloatObject spawnPos2;
            FloatObject spawnPos3;
            FloatObject spawnPos4;

            if (!p1)
            {
                spawnPos1 = new FloatObject(new Vector3(0, 0, 0));
            }
            else
            {
                spawnPos1 = new FloatObject(p1);
            }

            if (!p2)
            {
                spawnPos2 = new FloatObject(new Vector3(1, 0, 0));
            }
            else
            {
                spawnPos2 = new FloatObject(p2);
            }

            if (!p3)
            {
                spawnPos3 = new FloatObject(new Vector3(0, 0, -999));
            }
            else
            {
                spawnPos3 = new FloatObject(p3);
            }

            if (!p4)
            {
                spawnPos4 = new FloatObject(new Vector3(0, 0, -999));
            }
            else
            {
                spawnPos4 = new FloatObject(p4);
            }

            Dictionary<int, DefaultObject> objectsDict = new Dictionary<int, DefaultObject>();

            for (int i = 0; i < objects.Count; i++)
            {
                //TODO: make sure not to consider children BEFORE their parent
                DefaultObject parent;

                if (TryGetAsSBMObject(objects[i].gameObject, out DefaultObject parentObject))
                {
                    parent = parentObject;
                }
                else
                {
                    continue;
                }

                parent.isChild = objects[i].isChild;

                objectsDict.Add(objects[i].GetInstanceID(), parent);

                foreach (Transform childTransform in objects[i].transform)
                {
                    if (childTransform.TryGetComponent(out EditorSelectable child) && !childTransform.name.Contains("Node"))
                    {
                        // make sure child is active to save, otherwise deleted children will be recorded
                        if (childTransform.gameObject.activeSelf)
                        {
                            parent.children.Add(child.GetInstanceID());
                        }
                    }
                }
            }

            string filePath = Path.Combine(LevelLoader_Mod.levelsPath, EditorManager.instance.selectedLevel);

            File.WriteAllLines(filePath, new string[] { JsonConvert.SerializeObject(new JSONObjectContainer(worldStyle, spawnPos1, spawnPos2, spawnPos3, spawnPos4, objectsDict), Formatting.Indented)});
        }

        private static bool TryGetAsSBMObject(GameObject gameObject,  out DefaultObject defaultObject)
        {
            string objectName = NameToPath(gameObject.name);

            if (objectName.Contains("Water"))
            {
                defaultObject = new WaterObject(gameObject);
                return true;
            }
            else if (objectName.Contains("MinecartRail") && !objectName.Contains("Sleeper"))
            {
                defaultObject = new RailObject(gameObject);
                return true;
            }
            else if (objectName.Contains("SplineObject"))
            {
                defaultObject = new SplineObject(gameObject);
                return true;
            }
            else if (objectName.Contains("StiffRod") || objectName.Contains("SlipNSlide"))
            {
                defaultObject = new MeshSliceObject(gameObject);
                return true;
            }
            else if (objectName.Contains("SeeSaw"))
            {
                defaultObject = new SeeSawObject(gameObject);
                return true;
            }
            else if (objectName.Contains("FlipBlock"))
            {
                defaultObject = new FlipBlockObject(gameObject);
                return true;
            }
            else if (objectName.Contains("PistonPlatform"))
            {
                defaultObject = new PistonObject(gameObject);
                return true;
            }
            else if (objectName.Contains("ColorBlock"))
            {
                defaultObject = new ColorBlockObject(gameObject);
                return true;
            }
            else if (objectName != string.Empty && !objectName.Contains("Node"))
            {
                defaultObject = new DefaultObject(gameObject);
                return true;
            }
            else
            {
                defaultObject = null;
                return false;
            }
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
            else if (basketallContents.Contains(goName))
            {
                path = Path.Combine(baseResourcesPath, "basketball");
            }
            else if (itemContents.Contains(goName))
            {
                path = Path.Combine("prefabs", "items");
            }
            else if (customContents.Contains(goName))
            {
                return goName;
            }
            else return "";

            return Path.Combine(path, goName);
        }
    }
}
