using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using SplineMesh;
using SceneSystem = SBM.Shared.SceneSystem;
using Systems = SBM.Shared.Systems;

namespace SBM_CustomLevels
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance;
        public static WormholeTransitionManager wormholeManager;

        public float curWaterHeight = 0;
        public float curWaterWidth = 0;

        private int lastWorldStyle = 1;

        private static bool inLevel;

        public static bool InLevel
        {
            get
            {
                return inLevel;
            }
            set
            {
                inLevel = value;
            }
        }

        public int levelNumber;
        public string currentLevel;

        public string PreviousSceneName { get; private set; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;

                GameObject wormholeManagerGO = new GameObject("WormholeManager", typeof(WormholeTransitionManager));
                wormholeManagerGO.hideFlags = HideFlags.HideAndDontSave;
            }
            else if (instance != this)
            {
                Debug.Log($"{GetType().Name} already exists, destroying object!");
                Destroy(this);
            }
        }

        private void LoadLevel(bool isEditor, bool newLevel, string path)
        {
            //create a base scene in assetbundle
            AssetBundle sceneBundle = LevelLoader_Mod.GetAssetBundleFromResources("scene-bundle");

            LoadSceneParameters loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive);

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("base level", loadSceneParameters);

            // load level after scene is loaded
            asyncOperation.completed += delegate (AsyncOperation o)
            {
                SceneSystem.SetActiveScene("base level");

                sceneBundle.Unload(true);

                Scene sceneByName = SceneManager.GetSceneByName("base level");

                foreach (GameObject gameObject in SceneManager.GetSceneByName("Systems").GetRootGameObjects())
                {
                    if (!Systems.GameObjectIsSystem(gameObject))
                    {
                        SceneManager.MoveGameObjectToScene(gameObject, sceneByName);
                    }
                }

                int worldStyle = 1;

                if (isEditor)
                {
                    if (newLevel)
                    {
                        EditorManager.LoadNewEditorLevel();
                    }
                    else
                    {
                        EditorManager.LoadEditorLevel(path);
                    }

                    EditorManager.instance.InitializeEditor();
                }
                else
                {
                    lastWorldStyle = worldStyle;
                    worldStyle = LoadJSONLevel(path);
                }

                Instantiate(Resources.Load("prefabs/level/LevelPrefab_Story") as GameObject); //must happen AFTER carrot is loaded in, otherwise some stuff is goofed^

                if (isEditor)
                {
                    //add camera controller, (camera loaed in LevelPrefab_Story), moved from InitializeEditor as camera didnt exist previously...)
                    Camera.main.gameObject.AddComponent<CameraController>();

                    EditorManager.instance.editorCamera = Camera.main;

                    Destroy(GameObject.Find("Player 1"));
                    GameObject player2 = GameObject.Find("Player 2");
                    if (player2)
                    {
                        Destroy(player2);
                    }

                    SBM.Shared.Audio.AudioSystem.FadeOutMusicVolume();
                }
                else
                {
                    //play correct music for world style (based on world background selected)
                    SBM.Shared.Audio.Song song = SBM.Shared.Level.LevelSystem.GetWorld(worldStyle - 1).Song;

                    bool playIntro = true;

                    // if last world style is different, play intro
                    // if last world style is same, dont play intro
                    if (lastWorldStyle == worldStyle)
                    {
                        playIntro = false;
                    }

                    SBM.Shared.Audio.AudioSystem.instance.ScheduleSong(song, playIntro);
                }
            };
        }

        /*private void LoadLevel(bool isEditor, bool newLevel, string path)
        {
            LoadLevelScene(path, isEditor);

            int worldStyle = 1;

            if (isEditor)
            {
                if (newLevel)
                {
                    EditorManager.LoadNewEditorLevel();
                }
                else
                {
                    EditorManager.LoadEditorLevel(path);
                }

                EditorManager.instance.InitializeEditor();
            }
            else
            {
                lastWorldStyle = worldStyle;
                worldStyle = LoadJSONLevel(path);
            }

            Instantiate(Resources.Load("prefabs/level/LevelPrefab_Story") as GameObject); //must happen AFTER carrot is loaded in, otherwise some stuff is goofed^

            if (isEditor)
            {
                //add camera controller, (camera loaed in LevelPrefab_Story), moved from InitializeEditor as camera didnt exist previously...)
                Camera.main.gameObject.AddComponent<CameraController>();

                EditorManager.instance.editorCamera = Camera.main;

                Destroy(GameObject.Find("Player 1"));
                GameObject player2 = GameObject.Find("Player 2");
                if (player2)
                {
                    Destroy(player2);
                }

                SBM.Shared.Audio.AudioSystem.FadeOutMusicVolume();
            }
            else
            {
                //play correct music for world style (based on world background selected)
                SBM.Shared.Audio.Song song = SBM.Shared.Level.LevelSystem.GetWorld(worldStyle - 1).Song;

                bool playIntro = true;

                // if last world style is different, play intro
                // if last world style is same, dont play intro
                if (lastWorldStyle == worldStyle)
                {
                    playIntro = false;
                }

                SBM.Shared.Audio.AudioSystem.instance.ScheduleSong(song, playIntro);
            }
        }*/

        private int LoadJSONLevel(string path)
        {
            Debug.Log(path);

            if (!Directory.Exists(LevelLoader_Mod.levelsPath))
            {
                Directory.CreateDirectory(LevelLoader_Mod.levelsPath);
            }

            if (!File.Exists(path))
            {
                return -1;
            }

            string rawText = File.ReadAllText(path);
            int worldStyle = (int)char.GetNumericValue(rawText[0]);
            rawText = rawText.Remove(0, 1);

            CreateBackground(worldStyle);

            ObjectContainer json = JsonConvert.DeserializeObject<ObjectContainer>(rawText);

            Vector3 spawnPos_1 = json.spawnPosition1.GetPosition();
            Vector3 spawnPos_2 = json.spawnPosition2.GetPosition();

            foreach (DefaultObject defaultObject in json.defaultObjects) //itearate through default objects, instantiate based on name
            {
                GameObject loadedObject;

                loadedObject = Instantiate(Resources.Load(defaultObject.objectName) as GameObject, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                loadedObject.transform.localScale = defaultObject.GetScale();
            }

            foreach (WaterObject waterObject in json.waterObjects) //iterate through water objects, apply separate water logic (height, width via component)
            {
                SBM.Shared.Utilities.Water.Water loadedObject;

                curWaterHeight = waterObject.waterHeight;
                curWaterWidth = waterObject.waterWidth;

                loadedObject = Instantiate(Resources.Load(waterObject.objectName) as GameObject, waterObject.GetPosition(), Quaternion.Euler(waterObject.GetRotation())).GetComponent<SBM.Shared.Utilities.Water.Water>();

                if (waterObject.keyframes.Count > 0)
                {
                    loadedObject.AnimateWaterHeight = true;
                    loadedObject.WaterHeightVsTime = new AnimationCurve(waterObject.keyframes.ToArray());
                }
                //further logic in Patches.InitializeWaterValues
            }

            foreach (RailObject railObject in json.railObjects)
            {
                GameObject rail = MinecartRailHelper.CreateRailFromObject(railObject, false);
            }

            GameObject playerSpawn_1 = new GameObject("PlayerSpawn_1", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_1.transform.position = spawnPos_1;

            GameObject playerSpawn_2 = new GameObject("PlayerSpawn_2", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_2.transform.position = spawnPos_2;

            return worldStyle;
        }

        public void BeginLoadLevel(bool isEditor, bool newLevel, string path, int level)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Level does not exist!");
            }

            if (File.ReadAllBytes(path).Length == 0 && !isEditor)
            {
                throw new Exception("Empty level attempted to load! Make sure to save in editor first.");
            }

            PreviousSceneName = SceneManager.GetActiveScene().name;

            if (PreviousSceneName == "Systems")
            {
                PreviousSceneName = "";
            }

            if (!isEditor)
            {
                currentLevel = path;

                if (!LevelManager.InLevel)
                {
                    SBM.Shared.Cameras.MenuCamera menuCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SBM.Shared.Cameras.MenuCamera>();
                    menuCamera.targetPos = new Vector3(-0.715f, 4.05f, 21.96f);
                    menuCamera.targetLookAt = new Vector3(-0.63f, 5.76f, 27.03f);

                    SBM.UI.Utilities.Focus.UIFocusable.FocusedObject.gameObject.transform.parent.GetComponent<SBM.UI.Utilities.Transitioner.UITransitioner>().Transition_Out_To_Right(); //parent of level button is level selector
                    SBM.UI.Utilities.Focus.UIFocusable.ReleaseFocusedObject();
                    wormholeManager.WormholeAnimation(path, level);
                }
                else
                {
                    InLevel = true;

                    SceneSystem.Unload(PreviousSceneName).completed += delegate (AsyncOperation o)
                    {
                        LoadLevel(isEditor, newLevel, path);
                    };

                }   
            }
            else
            {
                SceneSystem.Unload(PreviousSceneName).completed += delegate (AsyncOperation o)
                {
                    LoadLevel(isEditor, newLevel, path);
                };
            }
        }

        public string GetNextLevel()
        {
            string nextLevel = "";

            if (currentLevel != null)
            {
                FileInfo[] levelPaths = Directory.GetParent(currentLevel).GetFiles("*.sbm");

                if (levelPaths.Length-1 > levelNumber-1) // if next level exists... levelNumber-1 since level number is + 1.
                {
                    nextLevel = levelPaths[levelNumber].ToString();
                }

                levelNumber++;
            }

            return nextLevel;
        }

        public GameObject CreateBackground(int worldStyle)
        {
            GameObject bg;

            switch (worldStyle)
            {
                case 1:
                    RenderSettings.skybox = LevelLoader_Mod.skyboxWorld1;
                    bg = Instantiate(Resources.Load<GameObject>(Path.Combine("prefabs", "level", "world1", "WorldBG_1")));
                    break;
                case 2:
                    RenderSettings.skybox = LevelLoader_Mod.skyboxWorld2;
                    bg = Instantiate(Resources.Load<GameObject>(Path.Combine("prefabs", "level", "world2", "World_2_BG")));
                    break;
                case 3:
                    RenderSettings.skybox = LevelLoader_Mod.skyboxWorld3;
                    bg = Instantiate(Resources.Load<GameObject>(Path.Combine("prefabs", "level", "world3", "World3_BG")));
                    break;
                case 4:
                    RenderSettings.skybox = LevelLoader_Mod.skyboxWorld4;
                    bg = Instantiate(Resources.Load<GameObject>(Path.Combine("prefabs", "level", "world4", "World4_BG")));
                    break;
                case 5:
                    RenderSettings.skybox = LevelLoader_Mod.skyboxWorld1;
                    bg = Instantiate(Resources.Load<GameObject>(Path.Combine("prefabs", "level", "world5", "World5_BG")));
                    break;
                default:
                    RenderSettings.skybox = LevelLoader_Mod.skyboxWorld1;
                    bg = Instantiate(Resources.Load<GameObject>(Path.Combine("prefabs", "level", "world1", "WorldBG_1")));
                    break;
            }

            RenderSettings.skybox.shader = Shader.Find("Skybox/Horizon With Sun Skybox");

            return bg;
        }

        public static void FadeOutCustomScene(Color start, Color end, SBM.UI.Components.ScreenFader screenFader)
        {
            screenFader.gameObject.SetActive(true);
            screenFader.startColor = start;
            screenFader.endColor = end;
            screenFader.fadeStartTime = Time.realtimeSinceStartup;

            PropertyInfo property = typeof(SBM.UI.Components.ScreenFader).GetProperty("IsFading"); //access private setter for "IsFading"
            property.DeclaringType.GetProperty("IsFading");
            property.GetSetMethod(true).Invoke(screenFader, new object[] { true });

            SBM.Shared.Audio.AudioSystem.FadeOutMusicVolume();
        }

        public class WormholeTransitionManager : MonoBehaviour
        {
            private SBM.UI.MainMenu.StoryMode.UIStoryModeRagdolls ragdolls;

            private int level;
            private string path;

            private void Awake()
            {
                if (wormholeManager == null)
                {
                    wormholeManager = this;
                }
                else if (instance != this)
                {
                    Debug.Log($"{GetType().Name} already exists, destroying object!");
                    Destroy(this);
                }
            }

            public void WormholeAnimation(string _path, int _level)
            {
                ragdolls = GameObject.Find("Ragdolls (Story Mode)").GetComponent<SBM.UI.MainMenu.StoryMode.UIStoryModeRagdolls>();

                path = _path;
                level = _level;

                if (ragdolls.cTransitionOutToSelectedLevel != null)
                {
                    ragdolls.StopCoroutine(ragdolls.cTransitionOutToSelectedLevel);
                }
                ragdolls.cTransitionOutToSelectedLevel = StartCoroutine(GetRoutine());
            }

            private bool AllRagdollsAre(SBM.UI.MainMenu.StoryMode.UIStoryModeRagdolls.RagdollState state, bool includeInactive)
            {
                for (int i = 0; i < ragdolls.states.Length; i++)
                {
                    if ((includeInactive || ragdolls.ragdolls[i].gameObject.activeSelf) && ragdolls.states[i] != state)
                    {
                        return false;
                    }
                }
                return true;
            }

            private bool BeginTransition_1()
            {
                return AllRagdollsAre(SBM.UI.MainMenu.StoryMode.UIStoryModeRagdolls.RagdollState.LerpedIn, false);
            }

            private bool BeginTransition_2()
            {
                return !ragdolls.ragdollIsRotatingToFaceForward[0] && !ragdolls.ragdollIsRotatingToFaceForward[1];
            }

            private bool BeginTransition_3()
            {
                return ragdolls.wormhole.IsSuckingIn;
            }

            private bool BeginTransition_4()
            {
                return !ragdolls.wormhole.IsClosing && !ragdolls.wormhole.ClosePending;
            }

            private bool BeginTransition_5()
            {
                return ragdolls.screenFader == null || ragdolls.screenFader.IsFading;
            }

            private IEnumerator GetFaceRagdollForward(int ragdollIndex)
            {
                FaceRagdollForward ragdollForward = new FaceRagdollForward(0);
                ragdollForward.ragdollIndex = ragdollIndex;
                return ragdollForward;
            }

            private IEnumerator GetRoutine()
            {
                TransitionOutToLevel transitionOut = new TransitionOutToLevel(0);
                return transitionOut;
            }

            public sealed class TransitionOutToLevel : IEnumerator
            {
                private WormholeTransitionManager transitionManager;

                public int ragdollCount;
                public int field;
                public int state;
                public object current;
                public object Current
                {
                    get
                    {
                        return current;
                    }
                }

                public TransitionOutToLevel(int _state)
                {
                    transitionManager = wormholeManager;
                    state = _state;
                }

                public bool MoveNext()
                {
                    int num = state;

                    switch (num)
                    {
                        case 0:
                            state = -1;
                            transitionManager.ragdolls.isTransitioningOutToSelectedLevel = true;
                            ragdollCount = SBM.Shared.GameMode.Current == SBM.Shared.GameModeType.CoopStory ? 2 : 1;
                            transitionManager.ragdolls.LerpInRagdolls();
                            current = new WaitUntil(new Func<bool>(transitionManager.BeginTransition_1));
                            state = 1;
                            return true;
                        case 1:
                            state = -1;
                            for (int i = 0; i < ragdollCount; i++)
                            {
                                transitionManager.ragdolls.ragdolls[i].gameObject.SetActive(true);
                                transitionManager.ragdolls.ragdolls[i].DanceController.SetDanceMove(SBM.Objects.Menu.Ragdoll.DanceType.Panic);
                            }
                            for (int j = 0; j < ragdollCount; j++)
                            {
                                transitionManager.ragdolls.ragdolls[j].TorsoCollider.enabled = false;
                            }
                            transitionManager.ragdolls.wormhole.Open();
                            break;
                        case 2:
                            state = -1;
                            break;
                        case 3:
                            state = -1;
                            for (int k = 0; k < ragdollCount; k++)
                            {
                                transitionManager.ragdolls.ragdolls[k].DanceController.SetDanceMove(SBM.Objects.Menu.Ragdoll.DanceType.Panic);
                            }
                            current = new WaitForSeconds(0.4f);
                            state = 4;
                            return true;
                        case 4:
                            state = -1;
                            for (int l = 0; l < ragdollCount; l++)
                            {
                                transitionManager.ragdolls.ragdolls[l].TorsoCollider.enabled = true;
                            }
                            field = ragdollCount - 1;
                            goto IL_2E6;
                        case 5:
                            state = -1;
                            goto IL_2D4;
                        case 6:
                            state = -1;
                            transitionManager.ragdolls.wormhole.Close();
                            current = new WaitUntil(new Func<bool>(transitionManager.BeginTransition_4));
                            state = 7;
                            return true;
                        case 7:
                            state = -1;
                            FadeOutCustomScene(transitionManager.ragdolls.screenFader.transparent, transitionManager.ragdolls.screenFader.black, transitionManager.ragdolls.screenFader); //update here
                            current = new WaitWhile(new Func<bool>(transitionManager.BeginTransition_5));
                            state = 8;
                            return true;
                        case 8:
                            state = -1;
                            transitionManager.ragdolls.isTransitioningOutToSelectedLevel = false;

                            SceneSystem.Unload(LevelManager.instance.PreviousSceneName).completed += delegate (AsyncOperation o)
                            {
                                LevelManager.InLevel = true;
                                LevelManager.instance.levelNumber = transitionManager.level;

                                LevelManager.instance.LoadLevel(false, false, transitionManager.path);
                            };

                            return false;
                        default:
                            return false;
                    }

                    if (!transitionManager.ragdolls.wormhole.IsOpening)
                    {
                        for (int m = 0; m < ragdollCount; m++)
                        {
                            if (transitionManager.ragdolls.cRagdollsFacingForwards[m] != null)
                            {
                                transitionManager.ragdolls.StopCoroutine(transitionManager.ragdolls.cRagdollsFacingForwards[m]);
                            }

                            transitionManager.ragdolls.cRagdollsFacingForwards[m] = transitionManager.ragdolls.StartCoroutine(transitionManager.GetFaceRagdollForward(m));
                        }
                        current = new WaitUntil(new Func<bool>(transitionManager.BeginTransition_2));
                        state = 3;
                        return true;
                    }
                    current = null;
                    state = 2;
                    return true;

                    IL_2D4:
                    int num2 = field;
                    field = num2 - 1;

                    IL_2E6:
                    if (field < 0)
                    {
                        current = new WaitWhile(new Func<bool>(transitionManager.BeginTransition_3));
                        state = 6;
                        return true;
                    }

                    SBM.Objects.Menu.Ragdoll.RagdollDancer ragdollDancer = transitionManager.ragdolls.ragdolls[field];
                    ragdollDancer.PlayVoiceSound();
                    ragdollDancer.FadeVoiceVolume(1f, 0f, 1f);
                    ragdollDancer.DanceController.JumpingEnabled = false;
                    ragdollDancer.Torso.isKinematic = false;

                    if (ragdollCount > 1)
                    {
                        current = new WaitForSeconds(0.35f);
                        state = 5;
                        return true;
                    }
                    goto IL_2D4;
                }

                public void Reset()
                {
                    state = -1;
                }
            }

            public sealed class FaceRagdollForward : IEnumerator
            {
                private WormholeTransitionManager transitionManager;

                public Quaternion startRot;
                public SBM.Objects.Menu.Ragdoll.RagdollDancer ragdollDancer;
                public int ragdollIndex;
                float elapsed;
                public int state;
                public object current;
                public object Current
                {
                    get
                    {
                        return current;
                    }
                }

                public FaceRagdollForward(int _state)
                {
                    transitionManager = wormholeManager;
                    state = _state;
                }

                public bool MoveNext()
                {
                    int num = state;

                    if (num != 0)
                    {
                        if (num != 1)
                        {
                            return false;
                        }
                        state = -1;
                        if (elapsed >= transitionManager.ragdolls.RagdollFaceForwardDuration)
		                {
                            transitionManager.ragdolls.ragdollIsRotatingToFaceForward[ragdollIndex] = false;
                            return false;
                        }
                    }
                    else
                    {
                        state = -1;
                        transitionManager.ragdolls.ragdollIsRotatingToFaceForward[ragdollIndex] = true;
                        ragdollDancer = transitionManager.ragdolls.ragdolls[ragdollIndex];
                        startRot = ragdollDancer.transform.rotation;
                        elapsed = 0f;
                    }

                    elapsed += Time.deltaTime;
                    float num2 = elapsed / transitionManager.ragdolls.RagdollFaceForwardDuration;
                    ragdollDancer.transform.rotation = Quaternion.Slerp(startRot, transitionManager.ragdolls.ragdollStartRot, num2);
                    current = null;
                    state = 1;
                    return true;
                }

                public void Reset()
                {
                    state = -1;
                }
            }
        }
    }
}
