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
using SoftMasking;

namespace SBM_CustomLevels
{
    [HarmonyPatch]
    internal class MenuManager : MonoBehaviour
    {
        public static MenuManager instance;

        private UIWorldSelector worldSelector;
        private UIFocusable levelEditorButton;

        private UIFocusable worldNameButton;
        private InputField worldNameField;

        private GameObject customWorldSelector;
        private GameObject customLevelSelector;
        private GameObject customPartyLevelSelect;
        private Level selectedPartyLevel;
        //private GameObject addToWorldUI;
        //private Text addToWorldText;

        private World selectedWorld;
        private bool worldsSelectsCreated = false;

        private int worldCount = 0;
        private int lastWorldSelected = 0;
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
        public static GameObject FindInactiveGameObject(string name)
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
        public static T FindInactiveGameObject<T>(string name) where T : UnityEngine.Object
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

            foreach (World world in LevelLoader_Mod.worldsList) //create world selection UI
            {
                GameObject worldObject = customWorlds.group[count].gameObject;
                worldObject.SetActive(true);

                worldObject.GetComponentInChildren<Text>().text = world.Name;

                if (world.Name == "Basketball" || world.Name == "Deathmatch" || world.Name == "Carrot Grab")
                {
                    worldObject.GetComponentInChildren<UI.Components.RawImageUVScroll>().gameObject.GetComponent<RawImage>().color = new Color32(255, 126, 0, 255); // orange
                }
                else if (world.levels.Count == 0)
                {
                    worldObject.GetComponentInChildren<UI.Components.RawImageUVScroll>().gameObject.GetComponent<RawImage>().color = new Color32(255, 10, 0, 255); // red
                }
                else if (world.levels.Count >= LevelLoader_Mod.maxLevels)
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
                    selectedWorld = world;

                    if (world.levels.Count > 0)
                    {
                        customWorldSelector.GetComponent<UITransitioner>().Transition_Out_To_Right();
                        customLevelSelector.GetComponent<UITransitioner>().Transition_In_From_Top();
                    }
                    else
                    {
                        instance.CreateNewLevel(world, true);
                        return;
                    }

                    UIFocusableGroup customLevels = customLevelSelector.GetComponent<UIFocusableGroup>();

                    foreach (UIFocusable level in customLevels.group)
                    {
                        level.gameObject.SetActive(false); // disable all previously activated level gameobjects
                    }

                    int count2 = 0;
                    foreach (Level level in world.levels) // create level selection ui
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
                            EditorManager.instance.selectedLevel = level.levelPath;
                            EditorManager.InEditor = true;

                            if (File.ReadAllBytes(level.levelPath).Length != 0)
                            {
                                LevelManager.instance.BeginLoadLevel(true, false, level.levelPath, 0, LevelManager.LevelType.Editor); // if level is not empty, load as existing level
                            }
                            else
                            {
                                LevelManager.instance.BeginLoadLevel(true, true, level.levelPath, 0, LevelManager.LevelType.Editor); // if level is empty, load as new level (create carrot, prefabs, etc.)
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

            if (selectedWorld == null)
            {
                return;
            }

            int count = 0;
            foreach (Level level in selectedWorld.levels) //create level selection ui
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
                    EditorManager.instance.selectedLevel = level.levelPath;
                    EditorManager.InEditor = true;

                    if (File.ReadAllBytes(level.levelPath).Length != 0)
                    {
                        LevelManager.instance.BeginLoadLevel(true, false, level.levelPath, 0, LevelManager.LevelType.Editor); // if level is not empty, load as existing level
                    }
                    else
                    {
                        LevelManager.instance.BeginLoadLevel(true, true, level.levelPath, 0, LevelManager.LevelType.Editor); // if level is empty, load as new level (create carrot, prefabs, etc.)
                    }
                });

                count++;
            }

            customLevels.FocusOnGroupMember(0);
        }

        private void CreateNewWorld(World world)
        {;
            //worldCount + 6, initially there are 5 worlds
            //worldCount + 5, world 1 is set at position 0

            if (world.Name == "Deathmatch" || world.Name == "Basketball" || world.Name == "Carrot Grab")
            {
                return;
            }

            GameObject worldModel = Instantiate(FindInactiveGameObject("WorldModel_1"), FindInactiveGameObject("transform").transform); //create a copy of world 1's world model for custom world
            worldModel.transform.localPosition = new Vector3(5 * (worldCount + 5), 0, 0); //set to proper position (5 to the right of world 5)
            
            UIFocusable worldUI_1 = FindInactiveGameObject("World 1").GetComponent<UIFocusable>();
            UIFocusable worldUI = Instantiate(worldUI_1.gameObject, worldUI_1.transform.parent).GetComponent<UIFocusable>();
            worldUI.gameObject.name = "World " + (worldCount + 6);
            
            UIWorldSelector worldSelector = FindInactiveGameObject<UIWorldSelector>("World Selector");

            UIFocusableGroup worldSelectorGroup = worldSelector.GetComponent<UIFocusableGroup>();
            worldSelectorGroup.group = worldSelectorGroup.group.Append(worldUI).ToArray(); //add new world to worldselector group, fixes selected world swapping when changing focus
            
            int worldIndex = worldCount + 5;

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
            /*UI.Utilities.Focus.UIFocusableColorShift.Condition newCondition = new UI.Utilities.Focus.UIFocusableColorShift.Condition();
            newCondition.Focusable = worldUI;
            newCondition.AnimateWhen = UI.Utilities.Focus.UIFocusableConditionHandler.ConditionType.NotFocused;
            
            //UI.Utilities.Focus.UIFocusableColorShift arrowLeft = FindInactiveGameObject<UI.Utilities.Focus.UIFocusableColorShift>("Arrow_Left");
            //arrowLeft.conditions = arrowLeft.conditions.Append(newCondition).ToArray();

            //UI.Utilities.Focus.UIFocusableColorShift arrowRight = FindInactiveGameObject<UI.Utilities.Focus.UIFocusableColorShift>("Arrow_Right");
            //arrowRight.conditions = arrowRight.conditions.Append(newCondition).ToArray();*/
            
            UITransitionerCarousel worldNamesTransitioner = FindInactiveGameObject<UITransitionerCarousel>("World Names");
            GameObject worldName_1 = FindInactiveGameObject<UI.Components.UI_SBMTheme_Text>("Text_WorldTitle_1").gameObject;
            //end
            
            GameObject customWorldName = Instantiate(worldName_1, worldName_1.transform.parent);
            customWorldName.name = "Text_WorldTitle_" + (worldCount + 6);

            Destroy(customWorldName.GetComponent<UnityEngine.Localization.PropertyVariants.GameObjectLocalizer>());
            Destroy(customWorldName.GetComponent<UnityEngine.Localization.Components.LocalizeStringEvent>());
            customWorldName.GetComponent<Text>().text = world.Name;
            
            customWorldName.SetActive(false);

            worldNamesTransitioner.transitioners = worldNamesTransitioner.transitioners.Append(customWorldName.GetComponent<UITransitioner>()).ToArray();

            worldCount++;
        }

        private void CreateNewLevel(World world, bool emptyWorld = false)
        {
            //string[] levels = Directory.GetFiles(worldPath);
            string levelName = "";

            if (world.Name == "Deathmatch" || world.Name == "Basketball" || world.Name == "Carrot Grab")
            {
                // create random 18 digit identifier for party levels, allowing individual levels to be synced across network based on identifier
                var random = new System.Random();
                for (int i = 0; i < 18; i++) //  hash length max 18
                    levelName = String.Concat(levelName, random.Next(10).ToString());
                levelName += ".sbm";
            }
            else
            {
                for (int i = 0; i < world.levels.Count + 1; i++)
                {
                    if (i >= 10)
                    {
                        return;
                    }

                    if (!File.Exists(Path.Combine(world.worldPath, (i + 1).ToString() + ".sbm")))
                    {
                        levelName = (i + 1).ToString() + ".sbm";

                        Debug.Log("Success! Level created at: " + levelName);

                        break;
                    }
                }
            }

            string path = Path.Combine(world.worldPath, levelName);

            if (path.IndexOfAny(Path.GetInvalidPathChars()) == -1 && !File.Exists(path))
            {
                using (File.Create(path)) { }

                world.UpdateLevels();

                instance.UpdateWorldButtons();
                instance.UpdateLevelButtons();
            }

            if (emptyWorld)
            {
                customWorldSelector.GetComponent<UITransitioner>().Transition_Out_To_Right();
                customLevelSelector.GetComponent<UITransitioner>().Transition_In_From_Top();
            }
        }

        private void CreateWorldEvent(string worldName)
        {
            string path = Path.Combine(LevelLoader_Mod.levelsPath, worldName);

            if (path.IndexOfAny(Path.GetInvalidPathChars()) < 0 && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                return;
            }

            World world = new World(worldName);

            if (worldsSelectsCreated)
            {
                instance.CreateNewWorld(world);
            }
            
            //instance.CreateCFG(path);
            instance.CreateNewLevel(world);

            LevelLoader_Mod.UpdateWorldsList();

            instance.worldNameField.text = String.Empty;
            
            instance.UpdateWorldButtons();
        }

        private void CreateWorldSelects()
        {
            GameObject levelSelector = FindInactiveGameObject("Level Selector");

            foreach (World world in LevelLoader_Mod.worldsList)
            {
                instance.CreateNewWorld(world);
            }

            foreach (UIFocusable level in levelSelector.GetComponent<UIFocusableGroup>().group)
            {
                level.gameObject.AddComponent<CustomLevelID>(); // add custom id component to identify which custom level each button points to
            }

            worldsSelectsCreated = true;
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
                FindInactiveGameObject<UITransitioner>("Screen_MainMenu").Transition_In_From_Bottom();
            });

            GameObject editLevelButton = GameObject.Find("EditLevelButton");
            editLevelButton.GetComponent<UIFocusable>().onSubmitSuccess.AddListener(delegate
            {
                if (worldCount == 0 && LevelLoader_Mod.worldsList.Count == 0)
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
                if (selectedWorld != null)
                {
                    CreateNewLevel(selectedWorld);
                }
            });
            
            levelEditorButton.onSubmitSuccess = new UnityEngine.Events.UnityEvent();
            levelEditorButton.onSubmitSuccess.AddListener(delegate
            {
                //open level editor ui
                FindInactiveGameObject<UITransitioner>("Screen_MainMenu").Transition_Out_To_Bottom();
                buttonContainer.GetComponent<UITransitioner>().Transition_In_From_Right();
            });
            
            loadedBundle.Unload(false);

            buttonContainer.SetActive(false);
            newWorldUI.SetActive(false);

            instance.customWorldSelector.SetActive(false);
            instance.customLevelSelector.SetActive(false);
        }

        [HarmonyPatch(typeof(SoftMask), "Start")]
        [HarmonyPostfix]
        static void UpdateMainMenu(SoftMask __instance)
        { 
            if (__instance.gameObject.name != "Button_PartyMode")
            {
                return;
            }
            
            AssetBundle uiBundle = LevelLoader_Mod.GetAssetBundleFromResources("ui-bundle");

            GameObject editorButton = Instantiate(uiBundle.LoadAsset<GameObject>("Button_LevelEditor"));
            editorButton.transform.parent = __instance.gameObject.transform.parent;
            editorButton.transform.localPosition = new Vector3(87, -124.5f, 0);
            editorButton.transform.localScale = Vector3.one;
            editorButton.GetComponent<SoftMask>().defaultShader = Shader.Find("Hidden/UI Default (Soft Masked)");
            
            RectTransform partyButton = __instance.gameObject.GetComponent<RectTransform>();

            GameObject emptyGO = new GameObject();
            emptyGO.transform.parent = partyButton;
            emptyGO.transform.localPosition = new Vector3(0, -40, 0);
            partyButton.GetChild(2).parent = emptyGO.transform;
            partyButton.localScale = new Vector3(300 / partyButton.sizeDelta.x, 1, 1);
            partyButton.localPosition = new Vector3(-140, -124.5f, 0);
            emptyGO.transform.localScale = new Vector3(1 / partyButton.localScale.x, 1, 0);
            
            foreach (Transform child in partyButton)
            {
                child.gameObject.AddComponent<SoftMaskable>();
            }
            
            Transform buttonContainer = partyButton.transform.parent;
            
            UIFocusable coopFocusable = buttonContainer.GetChild(1).gameObject.GetComponent<UIFocusable>();
            UIFocusable partyFocusable = buttonContainer.GetChild(2).gameObject.GetComponent<UIFocusable>();
            UIFocusable settingsFocusable = buttonContainer.GetChild(3).gameObject.GetComponent<UIFocusable>();
            UIFocusable quitFocusable = buttonContainer.GetChild(4).gameObject.GetComponent<UIFocusable>();
            
            UIFocusable editorFocusable = editorButton.GetComponent<UIFocusable>();
            editorFocusable.navTargetLeft = partyFocusable;
            editorFocusable.navTargetRight = settingsFocusable;
            editorFocusable.navTargetUp = coopFocusable;
            
            coopFocusable.navTargetDown = editorFocusable;
            partyFocusable.navTargetRight = editorFocusable;
            settingsFocusable.navTargetLeft = editorFocusable;
            quitFocusable.navTargetLeft = editorFocusable;

            if (instance == null)
            {
                instance = new MenuManager();
            }
            
            instance.levelEditorButton = editorFocusable;

            uiBundle.Unload(false);
            instance.CreateMenuUI();
        }

        [HarmonyPatch(typeof(UI.MainMenu.PartyMode.UIPartyModeLevels), "Awake")]
        [HarmonyPostfix]
        static void UpdatePartyLevelUI()
        {
            AssetBundle uiBundle = LevelLoader_Mod.GetAssetBundleFromResources("ui-bundle");

            // create party level select UI
            Image panelLevelSelect = FindInactiveGameObject<Image>("Panel_LevelSelect");
            panelLevelSelect.rectTransform.sizeDelta = new Vector2(panelLevelSelect.rectTransform.sizeDelta.x + 120, panelLevelSelect.rectTransform.sizeDelta.y);
            UIFocusableGroup partyLevelButtons = panelLevelSelect.transform.Find("LevelButtons").GetComponent<UIFocusableGroup>();
            UIFocusable customButton = Instantiate(partyLevelButtons.group.Last(), partyLevelButtons.group.Last().transform.parent);
            customButton.GetComponentInChildren<RawImage>().texture = uiBundle.LoadAsset<Texture>("ui_customlevel");

            // set navtargets for UI traversal
            customButton.navTargetLeft = partyLevelButtons.group[partyLevelButtons.group.Length - 1];
            partyLevelButtons.group[partyLevelButtons.group.Length - 1].navTargetRight = customButton;
            partyLevelButtons.group = partyLevelButtons.group.Append(customButton).ToArray();

            instance.customPartyLevelSelect = Instantiate(uiBundle.LoadAsset<GameObject>("CustomPartySelect"), panelLevelSelect.transform.parent);

            instance.customPartyLevelSelect.GetComponent<UIFocusable>().onCancel.AddListener(delegate
            {
                panelLevelSelect.gameObject.GetComponent<UITransitioner>().Transition_In_From_Left();
            });

            // change button event to open custom level select
            customButton.onSubmitSuccess = new UnityEngine.Events.UnityEvent();
            customButton.onSubmitSuccess.AddListener(delegate
            {
                panelLevelSelect.gameObject.GetComponent<UITransitioner>().Transition_Out_To_Left();
                instance.customPartyLevelSelect.GetComponent<UITransitioner>().Transition_In_From_Right();
            });

            UIFocusableGroup gameModeButtons = panelLevelSelect.transform.parent.Find("Panel_GameModeSelect/Buttons").GetComponent<UIFocusableGroup>();

            // Deathmatch
            gameModeButtons.group[0].onSubmitSuccess.AddListener(delegate
            {
                instance.UpdateCustomPartyLevels(LevelManager.LevelType.Deathmatch);
            });

            // Basketball
            gameModeButtons.group[1].onSubmitSuccess.AddListener(delegate
            {
                instance.UpdateCustomPartyLevels(LevelManager.LevelType.Basketball);
            });

            // Carrot Grab
            gameModeButtons.group[2].onSubmitSuccess.AddListener(delegate
            {
                instance.UpdateCustomPartyLevels(LevelManager.LevelType.CarrotGrab);
            });

            uiBundle.Unload(false);

            // update initially so UI is correct, deathmatch is default party mode
            instance.UpdateCustomPartyLevels(LevelManager.LevelType.Deathmatch);
            instance.customPartyLevelSelect.SetActive(false);
        }

        // custom level index (+5) exists beyond the bounds of the array, and LevelIsLocked tries to access an index outside of the array
        [HarmonyPatch(typeof(UI.MainMenu.PartyMode.PartyModeData), "LevelIsLocked")]
        [HarmonyPrefix]
        static bool PreventCustomLevelLocked(bool __result, int levelIndex)
        {
            // if custom level
            if (levelIndex >= 5)
            {
                __result = false;
                return false;
            }

            return true;
        }

        private void UpdateCustomPartyLevels(LevelManager.LevelType levelType)
        {
            List<Level> levels = new List<Level>();

            //TODO: FIX PARTY LEVEL IS LOCKED
            if (levelType == LevelManager.LevelType.Deathmatch)
            {
                foreach (World world in LevelLoader_Mod.worldsList)
                {
                    if (world.Name == "Deathmatch")
                    {
                        levels = world.levels;
                    }
                }
            }
            else if (levelType == LevelManager.LevelType.Basketball)
            {
                foreach (World world in LevelLoader_Mod.worldsList)
                {
                    if (world.Name == "Basketball")
                    {
                        levels = world.levels;
                    }
                }
            }
            else if (levelType == LevelManager.LevelType.CarrotGrab)
            {
                foreach (World world in LevelLoader_Mod.worldsList)
                {
                    if (world.Name == "Carrot Grab")
                    {
                        levels = world.levels;
                    }
                }
            }

            // set level buttons active
            int levelCount = levels.Count;
            UIFocusableGroup levelButtonGroup = instance.customPartyLevelSelect.transform.Find("LevelContainer").GetComponent<UIFocusableGroup>();

            UITransitioner panelSetup = instance.customPartyLevelSelect.transform.parent.Find("Panel_Setup").GetComponent<UITransitioner>();

            for (int i = 0; i < levelCount; i++)
            {
                if (i == 10)
                {
                    break;
                }

                levelButtonGroup.group[i].gameObject.SetActive(true);

                int index = i;

                levelButtonGroup.group[i].onSubmitSuccess = new UnityEngine.Events.UnityEvent();
                levelButtonGroup.group[i].onSubmitSuccess.AddListener(delegate
                {
                    instance.customPartyLevelSelect.GetComponent<UITransitioner>().Transition_Out_To_Right();
                    panelSetup.Transition_In_From_Left();
                    FindObjectOfType<UI.MainMenu.PartyMode.UIPartyModeLevels>().selectedLevelIndex = 5;
                    selectedPartyLevel = levels[index];
                });
            }

            for (int i = 0; i < levelButtonGroup.group.Length; i++)
            {
                if (i >= levelCount)
                {
                    levelButtonGroup.group[i].gameObject.SetActive(false);
                }
            }

            if (levelCount == 0)
            {
                levelButtonGroup.group[0].gameObject.SetActive(true);
            }
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
                instance.lastWorldSelected = 0;
            });

            //instance.CreateEditorWorldSelect();
            instance.CreateWorldSelects();
            //instance.CreateMenuUI();
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

        [HarmonyPatch(typeof(UI.MainMenu.StoryMode.UIStoryLevelButtons), "SetWorldIndex")]
        [HarmonyPrefix]
        static bool OverrideLevelButtons(UI.MainMenu.StoryMode.UIStoryLevelButtons __instance, int index)
        {
            UIFocusableGroup levelGroup = __instance.GetComponent<UIFocusableGroup>();

            if (index >= 5) //if custom world, adjust level numbers
            {
                __instance.worldIndex = index;
                int levelCount = LevelLoader_Mod.worldsList[index - 5].levels.Count;
                
                for (int i = 0; i < levelCount; i++)
                {
                    if (i == 10)
                    {
                        break;
                    }
                    
                    UI.MainMenu.StoryMode.UIStoryLevelButton levelButton = levelGroup.group[i].GetComponent<UI.MainMenu.StoryMode.UIStoryLevelButton>();

                    levelButton.levelLock.SetActive(false);
                    levelButton.SetLevelNumber(i + 1);

                    CustomLevelID levelID = levelButton.GetComponent<CustomLevelID>();
                    levelID.ID = LevelLoader_Mod.worldsList[index - 5].levels[i].levelPath;
                    levelID.world = LevelLoader_Mod.worldsList[index - 5];
                    levelID.GetComponent<CustomLevelID>().levelNumber = i+1;

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

        //TODO: add version for Party Levels multiplayer
        [HarmonyPatch(typeof(UI.MainMenu.StoryMode.UIStoryLevelButtons), "OnLevelButtonSubmit")]
        [HarmonyPrefix]
        static bool OverrideLevelSelect(UI.MainMenu.StoryMode.UIStoryLevelButton b)
        {
            if (instance.selectedWorldIndex >= 5) //if custom world selected, load custom level
            {
                CustomLevelID levelID = b.GetComponent<CustomLevelID>();
                if (levelID.ID == "NULL LEVEL") //if custom world has no levels, this button is the "No Levels" button - should not enter a level
                {
                    return false;
                }

                try
                {
                    if (SBM.Shared.Networking.NetworkSystem.IsInSession)
                    {
                        // send world identifier to all other players when in network. levelID.levelNumber - 1, since levelID.levelNumber starts at 1 rather than 0.
                        MultiplayerManager.SendCustomLevelData(levelID.world.WorldHash, levelID.levelNumber - 1, LevelManager.LevelType.Story);
                        LevelManager.instance.BeginLoadLevel(false, false, levelID.ID, levelID.levelNumber, LevelManager.LevelType.Story, levelID.world);
                    }
                    else
                    {
                        LevelManager.instance.BeginLoadLevel(false, false, levelID.ID, levelID.levelNumber, LevelManager.LevelType.Story, levelID.world);
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(SBM.UI.MainMenu.PartyMode.UIPartyModeLevels), "TransitionOutToLevel")]
        [HarmonyPrefix]
        static bool OverridePartyLevelSelect(UI.MainMenu.PartyMode.UIPartyModeLevels __instance)
        {
            if (__instance.screenFader.IsFading)
            {
                return false;
            }

            // if custom level
            if (__instance.selectedLevelIndex >= 5)
            {
                if (SBM.Shared.Networking.NetworkSystem.IsInSession && SBM.Shared.Networking.NetworkSystem.IsHost && SBM.Shared.Networking.NetworkSystem.RemoteUserCount <= 0)
                {
                    SBM.Shared.Networking.NetworkSystem.EndSession();
                    Debug.Log("Ended network session before starting party mode level (there were no remote users!).");
                }

                LevelManager.LevelType levelType;

                switch (SBM.Shared.GameMode.Current)
                {
                    case SBM.Shared.GameModeType.Basketball:
                        levelType = LevelManager.LevelType.Basketball;
                        break;
                    case SBM.Shared.GameModeType.Deathmatch:
                        levelType = LevelManager.LevelType.Deathmatch;
                        break;
                    case SBM.Shared.GameModeType.CarrotGrab:
                        levelType = LevelManager.LevelType.CarrotGrab;
                        break;
                    default:
                        levelType = LevelManager.LevelType.Deathmatch;
                        break;
                }

                //TODO: add list of party levels, can be renamed to differentiate
                try
                {
                    
                    if (SBM.Shared.Networking.NetworkSystem.IsInSession)
                    {
                        MultiplayerManager.SendCustomLevelData(instance.selectedPartyLevel.levelHash, 0, levelType);
                    }

                    __instance.onTransitionOutToLevel.Invoke();
                    LevelManager.instance.BeginLoadLevel(false, false, instance.selectedPartyLevel.levelPath, 0, levelType);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    instance.customPartyLevelSelect.GetComponent<UIFocusable>().onCancel.Invoke();
                }
                
                

                return false;
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
    }

    class CustomLevelID : MonoBehaviour
    {
        private string id;
        public int levelNumber;
        public World world;

        public string ID
        {
            get { return id; }
            set { id = value; }
        }
    }
}
