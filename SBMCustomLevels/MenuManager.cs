using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using UI = SBM.UI;
using Debug = UnityEngine.Debug;
using UIFocusable = SBM.UI.Utilities.Focus.UIFocusable;
using UIFocusableGroup = SBM.UI.Utilities.Focus.UIFocusableGroup;
using UIWorldSelector = SBM.UI.MainMenu.StoryMode.UIWorldSelector;
using UITransitioner = SBM.UI.Utilities.Transitioner.UITransitioner;
using UITransitionerCarousel = SBM.UI.Utilities.Transitioner.UITransitionerCarousel;
using UIScaler = SBM.UI.Scaler.UIScaler;

namespace SBM_CustomLevels
{
    [HarmonyPatch]
    internal class MenuManager : MonoBehaviour
    {
        public static MenuManager instance;

        private UIWorldSelector worldSelector;
        private UIFocusable editorWorldUI;

        private UIFocusable worldNameButton;
        private InputField worldNameField;

        private GameObject customWorldSelector;
        private GameObject customLevelSelector;
        //private GameObject addToWorldUI;
        //private Text addToWorldText;

        private string selectedWorld;

        private int worldCount = 0;
        private int lastWorldSelected = -1;
        private int selectedWorldIndex = 0;
        private List<UIFocusable> customWorlds = new List<UIFocusable>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log($"{GetType().Name} already exists, destroying object!");
                Destroy(this);
            }
        }

        private void Update() //allows for typing in inputfields because rewired goofs them
        {
            if (!worldNameButton || !worldNameField)
            {
                return;
            }

            if (!worldNameButton.focusEnabled)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            { 
                worldNameButton.Navigate(UI.Utilities.Focus.UINav.Down);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                worldNameButton.Navigate(UI.Utilities.Focus.UINav.Down);
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                worldNameButton.Navigate(UI.Utilities.Focus.UINav.Down);
            }
        }

        //searches all objects of specific name, returns object matching name if found
        static GameObject FindInactiveGameObject(string name)
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

            for (int i = 0; i < objects.Length; i++)
            {
                if (name == objects[i].name)
                {
                    return objects[i];
                }
            }

            return null;
        }

        //searches all objects of specific type and name, returns object matching name if found
        static T FindInactiveGameObject<T>(string name) where T : UnityEngine.Object
        {
            T[] objects = Resources.FindObjectsOfTypeAll<T>();

            for (int i = 0; i < objects.Length; i++)
            {
                if (name == objects[i].name)
                {
                    return objects[i];
                }
            }

            return null;
        }

        public void UpdateWorldButtons()
        {
            LevelLoader_Mod.UpdateWorldsList();

            int count = 0;

            UIFocusableGroup customWorlds = customWorldSelector.GetComponent<UIFocusableGroup>();

            foreach (UIFocusable world in customWorlds.group)
            {
                world.gameObject.SetActive(false); //disable all previously activated world gameobjects
            }

            foreach (Tuple<string, List<string>> world in LevelLoader_Mod.worldsList) //create world selection UI
            {
                GameObject worldObject = customWorlds.group[count].gameObject;
                worldObject.SetActive(true);

                worldObject.GetComponentInChildren<Text>().text = world.Item1.Split(Path.DirectorySeparatorChar).Last();

                if (world.Item2.Count == 0)
                {
                    worldObject.GetComponentInChildren<UI.Components.RawImageUVScroll>().gameObject.GetComponent<RawImage>().color = new Color32(255, 10, 0, 255); // red
                }
                else if (world.Item2.Count >= LevelLoader_Mod.maxLevels)
                {
                    worldObject.GetComponentInChildren<UI.Components.RawImageUVScroll>().gameObject.GetComponent<RawImage>().color = new Color32(0, 205, 30, 255); // green
                }
                else
                {
                    worldObject.GetComponentInChildren<UI.Components.RawImageUVScroll>().gameObject.GetComponent<RawImage>().color = new Color32(255, 224, 0, 255); // yellow
                }

                worldObject.GetComponent<UIFocusable>().onSubmitSuccess.RemoveAllListeners();
                worldObject.GetComponent<UIFocusable>().onSubmitSuccess.AddListener(delegate
                {
                    selectedWorld = world.Item1;

                    if (world.Item2.Count > 0)
                    {
                        customWorldSelector.GetComponent<UITransitioner>().Transition_Out_To_Right();
                        customLevelSelector.GetComponent<UITransitioner>().Transition_In_From_Top();
                    }
                    else
                    {
                        instance.CreateNewLevel(world.Item1);
                        return;
                    }

                    UIFocusableGroup customLevels = customLevelSelector.GetComponent<UIFocusableGroup>();

                    foreach (UIFocusable level in customLevels.group)
                    {
                        level.gameObject.SetActive(false); //disable all previously activated level gameobjects
                    }

                    int count2 = 0;
                    foreach (string level in world.Item2) //create level selection ui
                    {
                        if (count2 >= LevelLoader_Mod.maxLevels)
                        {
                            break;
                        }

                        GameObject levelObject = customLevels.group[count2].gameObject;
                        levelObject.SetActive(true);

                        levelObject.GetComponentInChildren<Text>().text = (count2 + 1).ToString();

                        levelObject.GetComponent<UIFocusable>().onSubmitSuccess.RemoveAllListeners();
                        levelObject.GetComponent<UIFocusable>().onSubmitSuccess.AddListener(delegate
                        {
                            EditorManager.instance.selectedLevel = level;
                            EditorManager.InEditor = true;

                            if (File.ReadAllBytes(level).Length != 0)
                            {
                                LevelManager.instance.BeginLoadLevel(true, false, level, 0); // if level is not empty, load as existing level
                            }
                            else
                            {
                                LevelManager.instance.BeginLoadLevel(true, true, level, 0); // if level is empty, load as new level (create carrot, prefabs, etc.)
                            }
                        });

                        count2++;
                    }
                });

                count++;
            }
        }

        public void UpdateLevelButtons()
        {
            UIFocusableGroup customLevels = customLevelSelector.GetComponent<UIFocusableGroup>();
            List<string> levels = new List<string>();

            foreach (Tuple<string, List<string>> world in LevelLoader_Mod.worldsList)
            {
                if (world.Item1 == selectedWorld)
                {
                    levels = world.Item2;
                    break;
                }
            }

            int count = 0;
            foreach (string level in levels) //create level selection ui
            {
                print("TEST!");

                if (count >= LevelLoader_Mod.maxLevels)
                {
                    break;
                }

                GameObject levelObject = customLevels.group[count].gameObject;
                levelObject.SetActive(true);

                levelObject.GetComponentInChildren<Text>().text = (count + 1).ToString();

                levelObject.GetComponent<UIFocusable>().onSubmitSuccess.RemoveAllListeners();
                levelObject.GetComponent<UIFocusable>().onSubmitSuccess.AddListener(delegate
                {
                    EditorManager.instance.selectedLevel = level;
                    EditorManager.InEditor = true;

                    if (File.ReadAllBytes(level).Length != 0)
                    {
                        LevelManager.instance.BeginLoadLevel(true, false, level, 0); // if level is not empty, load as existing level
                    }
                    else
                    {
                        LevelManager.instance.BeginLoadLevel(true, true, level, 0); // if level is empty, load as new level (create carrot, prefabs, etc.)
                    }
                });

                count++;
            }
        }

        private void CreateNewWorld(string worldPath)
        {
            //worldCount + 6, initially there are 5 worlds
            //worldCount + 5, world 1 is set at position 0

            GameObject worldModel = Instantiate(FindInactiveGameObject("WorldModel_1"), FindInactiveGameObject("transform").transform); //create a copy of world 1's world model for custom world
            worldModel.transform.localPosition = new Vector3(5*(worldCount+5), 0, 0); //set to proper position (5 to the right of world 5)

            UIFocusable worldUI_1 = FindInactiveGameObject("World 1").GetComponent<UIFocusable>();
            UIFocusable worldUI = Instantiate(worldUI_1.gameObject, worldUI_1.transform.parent).GetComponent<UIFocusable>();
            worldUI.gameObject.name = "World " + (worldCount+6);

            UIWorldSelector worldSelector = FindInactiveGameObject<UIWorldSelector>("World Selector");

            UIFocusableGroup worldSelectorGroup = worldSelector.GetComponent<UIFocusableGroup>();
            worldSelectorGroup.group = worldSelectorGroup.group.Append(worldUI).ToArray(); //add new world to worldselector group, fixes selected world swapping when changing focus

            int worldIndex = worldCount + 6;

            worldUI.onFocused = new UnityEngine.Events.UnityEvent();
            worldUI.onFocused.AddListener(delegate
            {
                worldSelector.onSelectedWorldIsUnlocked.Invoke();
                worldSelector.SetSelectedWorldIndex(worldIndex);
            });

            worldUI.onSubmit.AddListener(delegate
            {
                lastWorldSelected = worldIndex;
            });

            customWorlds.Add(worldUI);

            //set ui navtargets for changing worlds
            worldUI.navTargetRight = null;

            if (worldCount == 0)
            {
                UIFocusable world5 = FindInactiveGameObject<UIFocusable>("World 5");

                worldUI.navTargetLeft = world5;
                world5.navTargetRight = worldUI;
            }
            else
            {
                worldUI.navTargetLeft = customWorlds[worldCount - 1];
                customWorlds[worldCount - 1].navTargetRight = worldUI;  
            }

            //make world selecting arrows properly animate/color with custom worlds
            UI.Utilities.Focus.UIFocusableColorShift.Condition newCondition = new UI.Utilities.Focus.UIFocusableColorShift.Condition();
            newCondition.Focusable = worldUI;
            newCondition.AnimateWhen = UI.Utilities.Focus.UIFocusableConditionHandler.ConditionType.NotFocused;

            UI.Utilities.Focus.UIFocusableColorShift arrowLeft = FindInactiveGameObject<UI.Utilities.Focus.UIFocusableColorShift>("Arrow_Left");
            arrowLeft.conditions = arrowLeft.conditions.Append(newCondition).ToArray();

            UI.Utilities.Focus.UIFocusableColorShift arrowRight = FindInactiveGameObject<UI.Utilities.Focus.UIFocusableColorShift>("Arrow_Right");
            arrowRight.conditions = arrowRight.conditions.Append(newCondition).ToArray();

            UITransitionerCarousel worldNamesTransitioner = GameObject.Find("World Names").GetComponent<UITransitionerCarousel>();
            GameObject worldName_1 = FindInactiveGameObject<UI.Components.UI_SBMTheme_Text>("Text_WorldTitle_1").gameObject;
            //end

            GameObject customWorldName = Instantiate(worldName_1, worldName_1.transform.parent);
            customWorldName.name = "Text_WorldTitle_" + (worldCount+6);
            customWorldName.SetActive(false);
            customWorldName.GetComponent<Text>().text = worldPath.Split('\\').Last();

            worldNamesTransitioner.transitioners = worldNamesTransitioner.transitioners.Append(customWorldName.GetComponent<UITransitioner>()).ToArray();

            worldCount++;
        }

        private void CreateNewLevel(string worldPath)
        {
            string[] levels = Directory.GetFiles(worldPath);
            string levelName = "";

            for (int i = 0; i < levels.Length + 1; i++)
            {
                Debug.Log(i);

                if (i >= 10)
                {
                    return;
                }

                if (!File.Exists(Path.Combine(worldPath, (i + 1).ToString() + ".sbm")))
                {
                    levelName = (i + 1).ToString() + ".sbm";

                    Debug.Log("success");

                    break;
                }
            }

            string path = Path.Combine(worldPath, levelName);

            if (path.IndexOfAny(Path.GetInvalidPathChars()) == -1 && !File.Exists(path))
            {
                using (File.Create(path)) { }

                //TODO: get and update XML file when creating a new level

                LevelLoader_Mod.UpdateWorldsList();

                instance.UpdateLevelButtons();
            }
        }

        //TODO: make faster, fully implement
        private void CreateCFG(string worldPath) //creates .cfg file for storing world data 
        {
            Config config = new Config();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
            
            using (TextWriter textWriter = new StreamWriter(Path.Combine(worldPath, "world.cfg")))
            {
                xmlSerializer.Serialize(textWriter, config);
            }
        }

        private void CreateWorldEvent(string worldName)
        {
            string path = Path.Combine(LevelLoader_Mod.levelsPath, worldName);

            if (path.IndexOfAny(Path.GetInvalidPathChars()) < 0 && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            LevelLoader_Mod.UpdateWorldsList();

            instance.CreateNewWorld(path);
            //instance.CreateCFG(path);
            instance.CreateNewLevel(path);

            instance.worldNameField.text = String.Empty;

            instance.UpdateWorldButtons();
        }

        private void CreateEditorWorldSelect()
        {
            GameObject editorWorldModel = Instantiate(GameObject.Find("WorldModel_1"), GameObject.Find("transform").transform); //create a copy of world 1's world model for editor world
            editorWorldModel.transform.localPosition = new Vector3(-5, 0, 0); //set to proper position (5 to the left of world 1)


            UIFocusable worldUI_1 = GameObject.Find("World 1").GetComponent<UIFocusable>(); //cache world 1 ui
            editorWorldUI = Instantiate(worldUI_1.gameObject, worldUI_1.transform).GetComponent<UIFocusable>(); //create a copy of world 1's ui
            editorWorldUI.name = "World Editor";

            //adjust ui targets to account for new world
            worldUI_1.navTargetLeft = editorWorldUI;
            editorWorldUI.navTargetRight = worldUI_1;

            UITransitionerCarousel worldNamesTransitioner = GameObject.Find("World Names").GetComponent<UITransitionerCarousel>();

            GameObject worldName_1 = FindInactiveGameObject<UI.Components.UI_SBMTheme_Text>("Text_WorldTitle_1").gameObject;

            GameObject editorWorldName = Instantiate(worldName_1, worldName_1.transform.parent);
            editorWorldName.SetActive(false);

            editorWorldName.GetComponent<Text>().text = "Level Editor";

            worldNamesTransitioner.transitioners = worldNamesTransitioner.transitioners.Append(editorWorldName.GetComponent<UITransitioner>()).ToArray();

            UIFocusableGroup worldSelectorGroup = worldSelector.GetComponent<UIFocusableGroup>();
            worldSelectorGroup.group = worldSelectorGroup.group.Append(editorWorldUI).ToArray(); //add new world to worldselector group, fixes selected world swapping when changing focus

            editorWorldUI.onFocused.AddListener(delegate
            {
                worldSelector.SetSelectedWorldIndex(5);
            });

            editorWorldUI.onNavRightSuccess.AddListener(delegate
            {
                editorWorldName.GetComponent<UITransitioner>().Transition_Out_To_Left();
                worldName_1.GetComponent<UITransitioner>().Transition_In_From_Right();
            });

            //adjust listeners so that when loading the level editor UI, no level select menu is brought up
            worldUI_1.onNavRightSuccess.AddListener(delegate
            {
                editorWorldName.SetActive(false);
            });

            worldUI_1.onFocused.AddListener(delegate
            {
                if (editorWorldName.activeSelf)
                {
                    editorWorldName.SetActive(false);
                    worldName_1.GetComponent<UITransitioner>().Transition_In_From_Right();
                }
            });

            editorWorldUI.onNavRightFailed = worldUI_1.onNavRightFailed;
            editorWorldUI.onNavLeftSuccess = worldUI_1.onNavLeftSuccess;
            editorWorldUI.onNavLeftFailed = worldUI_1.onNavLeftFailed;
        }

        private void CreateWorldSelects()
        {
            GameObject levelSelector = FindInactiveGameObject("Level Selector");

            foreach (Tuple<string, List<string>> world in LevelLoader_Mod.worldsList)
            {
                instance.CreateNewWorld(world.Item1);
            }

            foreach (UIFocusable level in levelSelector.GetComponent<UIFocusableGroup>().group)
            {
                level.gameObject.AddComponent<CustomLevelID>(); // add custom id component to identify which custom level each button points to
            }
        }

        private void CreateMenuUI()
        {
            AssetBundle loadedBundle = LevelLoader_Mod.GetAssetBundleFromResources("ui-bundle");
            GameObject menuUI = Instantiate(loadedBundle.LoadAsset("MenuUI") as GameObject);

            instance.customWorldSelector = GameObject.Find("CustomWorldSelector");
            instance.customWorldSelector.transform.localScale = new Vector3(.7f, .7f, .7f);

            instance.customLevelSelector = GameObject.Find("CustomLevelSelector");
            instance.customLevelSelector.transform.localScale = new Vector3(.7f, .7f, .7f);

            GameObject buttonContainer = GameObject.Find("ButtonContainer");

            buttonContainer.GetComponent<UIFocusable>().onCancel.AddListener(delegate
            {
                instance.worldSelector.gameObject.GetComponent<UITransitioner>().Transition_In_From_Center();
            });

            GameObject editLevelButton = GameObject.Find("EditLevelButton");
            editLevelButton.GetComponent<UIFocusable>().onSubmitSuccess.AddListener(delegate
            {
                if (worldCount == 0)
                {
                    CreateWorldEvent("Default World");
                }
            });


            instance.UpdateWorldButtons();

            GameObject newWorldUI = GameObject.Find("NewWorldUI");

            GameObject worldNameUI = GameObject.Find("WorldNameField");
            instance.worldNameButton = worldNameUI.GetComponent<UIFocusable>();
            instance.worldNameField = worldNameUI.GetComponent<InputField>();

            GameObject worldCreateButton = GameObject.Find("CreateButton");
            worldCreateButton.GetComponent<UIFocusable>().onSubmitSuccess.AddListener(delegate
            {
                CreateWorldEvent(instance.worldNameField.text);
            });

            UIFocusable addLevelButton = GameObject.Find("AddLevelButton").GetComponent<UIFocusable>();
            addLevelButton.onSubmitSuccess.AddListener(delegate
            {
                if (selectedWorld != string.Empty)
                {
                    CreateNewLevel(selectedWorld);
                }
            });

            editorWorldUI.onSubmitSuccess = new UnityEngine.Events.UnityEvent(); //UIFocusable events are CACHED!!!! meaning they are carried over when instantiated :(
            editorWorldUI.onSubmitSuccess.AddListener(delegate
            {
                //open level editor ui
                buttonContainer.GetComponent<UITransitioner>().Transition_In_From_Right();
                worldSelector.GetComponent<UITransitioner>().Transition_Out_To_Center();
            });

            loadedBundle.Unload(false);

            buttonContainer.SetActive(false);
            newWorldUI.SetActive(false);

            instance.customWorldSelector.SetActive(false);
            instance.customLevelSelector.SetActive(false);
        }

        [HarmonyPatch(typeof(UI.MainMenu.StoryMode.UIStoryWorldModels), "Awake")]
        [HarmonyPostfix]
        static void CreateCustomWorlds()
        {
            instance.worldCount = 0;
            instance.customWorlds.Clear();

            instance.worldSelector = FindObjectOfType<UIWorldSelector>();

            instance.worldSelector.GetComponent<UIFocusable>().onFocused.AddListener(delegate
            {
                instance.lastWorldSelected = -1;
            });

            instance.CreateEditorWorldSelect();
            instance.CreateWorldSelects();
            instance.CreateMenuUI();
        }

        [HarmonyPatch(typeof(UIWorldSelector), "SetSelectedWorldIndex")]
        [HarmonyPrefix]
        static bool OverrideSelectedWorldIndex(UIWorldSelector __instance, int index)
        {
            instance.selectedWorldIndex = index;

            if (index >= 5)
            {
                __instance.onWorldSelected.Invoke(index);

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(UITransitionerCarousel), "SetIndex")]
        [HarmonyPrefix]
        static bool OverrideIndexPosition(UITransitionerCarousel __instance, int index)
        {
            if (__instance.gameObject.name != "World Names")
            {
                return true;
            }

            if (__instance.currentIndex == 5)
            {
                __instance.transitioners[__instance.currentIndex].Transition_Out_To_Right();
                __instance.transitioners[index].Transition_In_From_Left();

                return false;
            }

            if (index == 5)
            {
                __instance.transitioners[__instance.currentIndex].Transition_Out_To_Right();
                __instance.transitioners[index].Transition_In_From_Left();

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(UI.MainMenu.StoryMode.UIStoryLevelButtons), "SetWorldIndex")]
        [HarmonyPrefix]
        static bool OverrideLevelButtons(UI.MainMenu.StoryMode.UIStoryLevelButtons __instance, int index)
        {
            if (index == 5) //if level editor, dont set index
            {
                return false;
            }

            UIFocusableGroup levelGroup = __instance.GetComponent<UIFocusableGroup>();

            if (index >= 6) //if custom world, adjust level numbers
            {
                int levelCount = LevelLoader_Mod.worldsList[index - 6].Item2.Count;
                
                for (int i = 0; i < levelCount; i++)
                {
                    if (i == 10)
                    {
                        break;
                    }
                    
                    UI.MainMenu.StoryMode.UIStoryLevelButton levelButton = levelGroup.group[i].GetComponent<UI.MainMenu.StoryMode.UIStoryLevelButton>();

                    levelButton.levelLock.SetActive(false);
                    levelButton.SetLevelNumber(i + 1);
                    levelButton.GetComponent<CustomLevelID>().ID = LevelLoader_Mod.worldsList[index - 6].Item2[i];
                    levelButton.GetComponent<CustomLevelID>().levelNumber = i+1;

                    levelButton.gameObject.SetActive(true);
                }
                
                for (int i = 0; i < levelGroup.group.Length; i++)
                {
                    if (i >= levelCount)
                    {
                        levelGroup.group[i].gameObject.SetActive(false);
                    }
                }

                if (levelCount == 0)
                {
                    UI.MainMenu.StoryMode.UIStoryLevelButton levelButton = levelGroup.group[0].GetComponent<UI.MainMenu.StoryMode.UIStoryLevelButton>();
                    levelButton.gameObject.SetActive(true);

                    levelButton.GetComponentInChildren<Text>().text = "No";
                    levelButton.levelLock.SetActive(true);
                    levelButton.GetComponent<CustomLevelID>().ID = "NULL LEVEL";
                }

                return false;
            }

            for (int i = 0; i < levelGroup.group.Length; i++) //if not custom world, reactivate all buttons (assuming some are disabled)
            {
                levelGroup.group[i].gameObject.SetActive(true);
            }

            return true;
        }

        [HarmonyPatch(typeof(UI.MainMenu.StoryMode.UIStoryLevelButtons), "OnLevelButtonSubmit")]
        [HarmonyPrefix]
        static bool OverrideLevelSelect(UI.MainMenu.StoryMode.UIStoryLevelButton b)
        {
            if (instance.selectedWorldIndex >= 6) //if custom world selected, load custom level
            {
                if (b.GetComponent<CustomLevelID>().ID == "NULL LEVEL") //if custom world has no levels, this button is the "No Levels" button - should not enter a level
                {
                    return false;
                }
                try
                {
                    LevelManager.instance.BeginLoadLevel(false, false, b.GetComponent<CustomLevelID>().ID, b.GetComponent<CustomLevelID>().levelNumber);

                    return false;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(UI.MainMenu.StoryMode.UIWorldColorTargetGroup), "SetTargetWorldIndex")]
        [HarmonyPrefix]
        static bool OverrideWorldUIColor(UI.MainMenu.StoryMode.UIWorldColorTargetGroup __instance, int index)
        {
            if (index >= 5)
            {
                __instance.SetTargetColor(new Color32(204, 0, 0, 255));

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(UI.MainMenu.StoryMode.UIText_BadgesRequiredToUnlockWorld), "SetWorldIndex")]
        [HarmonyPrefix]
        static bool OverrideBadgeRequirement(int index)
        {
            if (index >= 5)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(UI.MainMenu.StoryMode.UIStoryWorldModels), "SetTargetWorldIndex")]
        [HarmonyPrefix]
        static void OverrideWorldModelIndex(ref int index)
        {
            Debug.Log("Index: " + index);

            if (index == 5)
            {
                index = -1; //allow editor to be before world 1
            }

            if (index >= 6)
            {
                index = index - 1; //account for gap caused by editor being index 5, but at index -1
            }
        }
    }

    class CustomLevelID : MonoBehaviour
    {
        private string id;
        public int levelNumber;

        public string ID
        {
            get { return id; }
            set { id = value; }
        }
    }
}
