using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using SBM_CustomLevels.Editor;
using SBM_CustomLevels.ObjectWrappers;
using SBM_CustomLevels.Objects;
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

        public static bool lastLevelWasEditor = false;
        public static bool loading = false;

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
        public World currentWorld;

        public string PreviousSceneName { get; private set; }

        public enum LevelType 
        { 
            Editor,
            Story,
            Basketball,
            Deathmatch,
            CarrotGrab,
            None
        }

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

        private void LoadLevel(bool isEditor, bool newLevel, string path, LevelType levelType)
        {
            lastLevelWasEditor = isEditor;

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
                SceneSystem.CurrentScene = sceneByName;

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
                        EditorManager.SetupLoadEditorLevel(path);
                    }

                    EditorManager.instance.InitializeEditor();

                    loading = false;
                }
                else
                {
                    // ensure a mismatch between remote user and local client, 255 tells us that this is a custom level
                    SceneSystem.TargetSceneIndex = 255;

                    lastWorldStyle = worldStyle;

                    string rawText = File.ReadAllText(path);

                    try
                    {
                        JSONObjectContainer json = JsonConvert.DeserializeObject<JSONObjectContainer>(rawText);
                        string version = json.Version;

                        // to determine style of loading for later incompatibilities: 
                        switch (version) 
                        {
                            case "1.4":
                                worldStyle = LoadJSONLevel(json, levelType);
                                break;
                            default:
                                worldStyle = LoadJSONLevel(json, levelType);
                                break;
                        }
                    }
                    catch
                    {
                        Debug.LogError("Legacy level loaded! Reverting to old loading method...");

                        worldStyle = (int)char.GetNumericValue(rawText[0]);
                        rawText = rawText.Remove(0, 1);

                        LegacyJSONObjectContainer json = JsonConvert.DeserializeObject<LegacyJSONObjectContainer>(rawText);
                        LegacyLoadJSONLevel(json, levelType, worldStyle);
                    }
                }

                switch (levelType)
                {
                    //must happen AFTER carrot is loaded in, otherwise some stuff is goofed^
                    case LevelType.Editor:
                    case LevelType.Story:
                        Instantiate(Resources.Load("prefabs/level/LevelPrefab_Story") as GameObject);
                        break;
                    case LevelType.Basketball:
                        Instantiate(Resources.Load("prefabs/level/LevelPrefab_Basketball") as GameObject);
                        break;
                    case LevelType.Deathmatch:
                        Instantiate(Resources.Load("prefabs/level/LevelPrefab_Deathmatch") as GameObject);
                        break;
                    case LevelType.CarrotGrab:
                        Instantiate(Resources.Load("prefabs/level/LevelPrefab_CarrotGrab") as GameObject);
                        break;
                }

                if (isEditor)
                {
                    //add camera controller, (camera loaed in LevelPrefab_Story), moved from InitializeEditor as camera didnt exist previously...)
                    Camera.main.gameObject.AddComponent<CameraController>();

                    EditorManager.instance.editorCamera = Camera.main;

                    /*Destroy(GameObject.Find("Player 1"));
                    GameObject player2 = GameObject.Find("Player 2");
                    if (player2)
                    {
                        Destroy(player2);
                    }*/

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

                    SBM.Shared.Audio.AudioSystem.ScheduleSong(song, playIntro);
                }
            };

            // random GO called "Button Blocker" that blocks some of the UI buttons (don't know why it even exists)
            GameObject buttonBlocker = GameObject.Find("Button Blocker");
            if (buttonBlocker)
            {
                buttonBlocker.SetActive(false);
            }
        }

        // New loading - version 1.4 and later
        private int LoadJSONLevel(JSONObjectContainer json, LevelType levelType)
        {
            bool carrot = false;
            bool wormhole = false;

            if (!Directory.Exists(LevelLoader_Mod.levelsPath))
            {
                Directory.CreateDirectory(LevelLoader_Mod.levelsPath);
            }

            CreateBackground(json.worldType);

            foreach (var objPair in json.objects)
            {
                DefaultObject obj = objPair.Value;

                if (obj.isChild)
                {
                    continue;
                }

                string name = EditorManager.SpawnObject(obj, json.objects).name;
                
                if (name == "Carrot(Clone)")
                {
                    carrot = true;
                }
                else if (name == "Wormhole(Clone)")
                {
                    wormhole = true;
                }
            }

            // ensure that a carrot and wormhole are loaded for story levels
            if (!carrot && levelType == LevelType.Story)
            {
                Instantiate(Resources.Load(RecordLevel.NameToPath("Carrot")));
            }

            if (!wormhole && levelType == LevelType.Story)
            {
                var wh = Instantiate(Resources.Load(RecordLevel.NameToPath("Wormhole")) as GameObject);
                wh.transform.position = new Vector3(5, 0, 0);
            }

            #region Spawns
            Vector3 spawnPos_1;
            Vector3 spawnPos_2;
            Vector3 spawnPos_3;
            Vector3 spawnPos_4;

            try
            {
                spawnPos_1 = json.spawnPosition1.GetPosition();
            }
            catch
            {
                Debug.LogError("Missing spawnPos_1 in json! Setting to (0,0,0).");
                spawnPos_1 = new Vector3(0, 0, 0);
            }

            try
            {
                spawnPos_2 = json.spawnPosition2.GetPosition();
            }
            catch
            {
                Debug.LogError("Missing spawnPos_2 in json! Setting to (1,0,0).");
                spawnPos_2 = new Vector3(1, 0, 0);
            }

            try
            {
                spawnPos_3 = json.spawnPosition3.GetPosition();
            }
            catch
            {
                Debug.LogError("Missing spawnPos_3 in json! Setting to NULL.");
                spawnPos_3 = new Vector3(0, 0, -999); // indicate a non-set spawn
            }

            try
            {
                spawnPos_4 = json.spawnPosition4.GetPosition();
            }
            catch
            {
                Debug.LogError("Missing spawnPos_4 in json! Setting to NULL.");
                spawnPos_4 = new Vector3(0, 0, -999); // indicate a non-set spawn
            }

            GameObject playerSpawn_1 = new GameObject("PlayerSpawn_1", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_1.transform.position = spawnPos_1;

            GameObject playerSpawn_2 = new GameObject("PlayerSpawn_2", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_2.transform.position = spawnPos_2;

            if (spawnPos_3 != new Vector3(0, 0, -999))
            {
                GameObject playerSpawn_3 = new GameObject("PlayerSpawn_3", typeof(SBM.Shared.PlayerSpawnPoint));
                playerSpawn_3.transform.position = spawnPos_3;
            }

            if (spawnPos_4 != new Vector3(0, 0, -999))
            {
                GameObject playerSpawn_4 = new GameObject("PlayerSpawn_4", typeof(SBM.Shared.PlayerSpawnPoint));
                playerSpawn_4.transform.position = spawnPos_4;
            }
            #endregion

            if (SBM.Shared.Networking.NetworkSystem.IsInSession)
            {
                // setup all profiles for CO-OP play, modified version of SBM.UI.Components.UIPlayerRoster.ConfigureCoopPlayersForNetworkPlay()

                /*if (SBM.Shared.Networking.NetworkSystem.IsHost)
                {
                    var localProfile = SBM.Shared.PlayerRoster.GetProfile(1);
                    var localUserId = SBM.Shared.Networking.NetworkSystem.LocalUserId;
                    string localUsername = SBM.Shared.Networking.NetworkSystem.LocalUsername;

                    localProfile.Overwrite(0, 0, SBM.Shared.Team.Red, localUserId, true, localUsername);

                    for (int i = 2; i < MultiplayerManager.playerCount; i++)
                    {
                        Debug.Log("Creating Profile... " + i);

                        var profile = SBM.Shared.PlayerRoster.GetProfile(i);
                        var remoteUserId = SBM.Shared.Networking.NetworkSystem.GetRemoteUserId(0);
                        string username = SBM.Shared.Networking.NetworkSystem.GetUsername(remoteUserId);

                        profile.Overwrite(0, 0, SBM.Shared.Team.Red, remoteUserId, false, username);
                    }
                }*/

                SBM.Shared.Networking.NetworkSystem.instance.OnSceneEvent(SBM.Shared.SceneEvent.LoadComplete, SceneManager.GetSceneByName("base level"));
                SBM.Shared.Level.LevelSystem.instance.OnSceneEvent(SBM.Shared.SceneEvent.LoadComplete, SceneManager.GetSceneByName("base level"));
            }

            loading = false;

            return json.worldType;
        }

        // Legacy - version 1.3 and earlier
        private int LegacyLoadJSONLevel(LegacyJSONObjectContainer json, LevelType levelType, int worldStyle)
        {
            if (!Directory.Exists(LevelLoader_Mod.levelsPath))
            {
                Directory.CreateDirectory(LevelLoader_Mod.levelsPath);
            }

            CreateBackground(worldStyle);

            Vector3 spawnPos_1;
            Vector3 spawnPos_2;
            Vector3 spawnPos_3;
            Vector3 spawnPos_4;

            try
            {
                spawnPos_1 = json.spawnPosition1.GetPosition();
            }
            catch
            {
                Debug.LogError("Missing spawnPos_1 in json! Setting to (0,0,0).");
                spawnPos_1 = new Vector3(0, 0, 0);
            }

            try
            {
                spawnPos_2 = json.spawnPosition2.GetPosition();
            }
            catch
            {
                Debug.LogError("Missing spawnPos_2 in json! Setting to (1,0,0).");
                spawnPos_2 = new Vector3(1, 0, 0);
            }

            try
            {
                spawnPos_3 = json.spawnPosition3.GetPosition();
            }
            catch
            {
                Debug.LogError("Missing spawnPos_3 in json! Setting to NULL.");
                spawnPos_3 = new Vector3(0, 0, -999); // indicate a non-set spawn
            }

            try
            {
                spawnPos_4 = json.spawnPosition4.GetPosition();
            }
            catch
            {
                Debug.LogError("Missing spawnPos_4 in json! Setting to NULL.");
                spawnPos_4 = new Vector3(0, 0, -999); // indicate a non-set spawn
            }

            try
            {
                bool carrotLoaded = false;
                bool wormholeLoaded = false;

                foreach (DefaultObject defaultObject in json.defaultObjects) //itearate through default objects, instantiate based on name
                {
                    GameObject loadedObject;

                    if (defaultObject.objectName == "ScaffoldingBlock")
                    {
                        loadedObject = Instantiate(EditorManager.scaffoldingBlock, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                    }
                    else if (defaultObject.objectName == "ScaffoldingCorner")
                    {
                        loadedObject = Instantiate(EditorManager.scaffoldingCorner, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                    }
                    else if (defaultObject.objectName == "ScaffoldPanelBlack")
                    {
                        loadedObject = Instantiate(EditorManager.scaffoldPanelBlack, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                    }
                    else if (defaultObject.objectName == "ScaffoldPanelBrown")
                    {
                        loadedObject = Instantiate(EditorManager.scaffoldPanelBrown, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                    }
                    else
                    {
                        loadedObject = Instantiate(Resources.Load(defaultObject.objectName) as GameObject, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                    }
                    
                    loadedObject.transform.localScale = defaultObject.GetScale();

                    if (loadedObject.name.Contains("Carrot"))
                    {
                        carrotLoaded = true;
                    }

                    if (loadedObject.name.Contains("Wormhole"))
                    {
                        wormholeLoaded = true;
                    }
                }

                // ensure that wormhole and carrot always exist to avoid crash (only important for StoryLevels)
                if (levelType == LevelType.Story)
                {
                    if (!wormholeLoaded)
                    {
                        Debug.LogError("Missing wormhole in json! Instantiating default wormhole!");

                        GameObject wormhole = Instantiate(Resources.Load(RecordLevel.NameToPath("Wormhole"))) as GameObject;
                        wormhole.transform.position = new Vector3(5, 0, 0);
                    }

                    if (!carrotLoaded)
                    {
                        Debug.LogError("Missing carrot in json! Instantiating default carrot!");
                        Instantiate(Resources.Load(RecordLevel.NameToPath("Carrot")));
                    }
                }
            }
            catch
            {
                Debug.LogError("Missing defaultObjects in json! Instantiating default carrot and wormhole!");

                Instantiate(Resources.Load(RecordLevel.NameToPath("Carrot")));
                GameObject wormhole = Instantiate(Resources.Load(RecordLevel.NameToPath("Wormhole"))) as GameObject;
                wormhole.transform.position = new Vector3(5, 0, 0);
            }
            try
            {
                foreach (WaterObject waterObject in json.waterObjects) //iterate through water objects, apply separate water logic (height, width via component)
                {
                    SBM.Shared.Utilities.Water.Water loadedObject;

                    curWaterHeight = waterObject.waterHeight;
                    curWaterWidth = waterObject.waterWidth;

                    if (waterObject.w5)
                    {
                        //Destroy(loadedObject.transform.Find("Water_W5").gameObject);
                        loadedObject = Instantiate(Resources.Load(waterObject.objectName) as GameObject, waterObject.GetPosition(), Quaternion.Euler(waterObject.GetRotation())).GetComponentInChildren<SBM.Shared.Utilities.Water.Water>();

                        var meshSlice = loadedObject.transform.root.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();

                        meshSlice.Size = new Vector3(waterObject.waterWidth, waterObject.waterHeight, 1.15f);
                        meshSlice.Regenerate();
                    }
                    else
                    {
                        loadedObject = Instantiate(Resources.Load(waterObject.objectName) as GameObject, waterObject.GetPosition(), Quaternion.Euler(waterObject.GetRotation())).GetComponent<SBM.Shared.Utilities.Water.Water>();
                    }

                    if (waterObject.keyframes.Count > 0)
                    {
                        loadedObject.AnimateWaterHeight = true;
                        loadedObject.WaterHeightVsTime = new AnimationCurve(waterObject.keyframes.ToArray());
                    }

                    //further logic in Patches.InitializeWaterValues
                }
            }
            catch
            {
                Debug.LogError("Missing waterObjects in json!");
            }

            try
            {
                foreach (MeshSliceObject meshSliceObject in json.meshSliceObjects) //iterate through mesh objects, apply logic (height, width via MeshSliceData component)
                {
                    GameObject loadedObject;

                    loadedObject = Instantiate(Resources.Load(meshSliceObject.objectName) as GameObject, meshSliceObject.GetPosition(), Quaternion.Euler(meshSliceObject.GetRotation()));

                    Vector3 meshSize = new Vector3(meshSliceObject.meshWidth, meshSliceObject.meshHeight, meshSliceObject.meshDepth);

                    if (meshSliceObject.objectName.Contains("SeeSaw"))
                    {
                        var seeSaw = loadedObject.GetComponent<SBM.Objects.World5.SeeSaw>();
                        seeSaw.platformSize = meshSize;
                        seeSaw.Regenerate();
                    }

                    var meshSlice = loadedObject.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();

                    meshSlice.Size = meshSize;
                    meshSlice.Regenerate();

                    if (meshSliceObject.objectName.Contains("SlipNSlide"))
                    {
                        var colliderSizers = loadedObject.GetComponentsInChildren<Catobyte.Utilities.BoxColliderAutoSizer>();
                        foreach (var sizer in colliderSizers)
                        {
                            sizer.ResizeBoxCollider();
                        }

                        var edgeRail1 = loadedObject.transform.GetChild(1);
                        edgeRail1.localScale = new Vector3(edgeRail1.localScale.x, meshSliceObject.meshWidth, edgeRail1.localScale.y);
                        edgeRail1.localPosition = new Vector3(-0.5f * meshSliceObject.meshWidth, edgeRail1.localPosition.y, edgeRail1.localPosition.z);

                        var edgeRail2 = loadedObject.transform.GetChild(2);
                        edgeRail2.localScale = new Vector3(edgeRail2.localScale.x, meshSliceObject.meshWidth, edgeRail2.localScale.y);
                        edgeRail2.localPosition = new Vector3(-0.5f * meshSliceObject.meshWidth, edgeRail2.localPosition.y, edgeRail2.localPosition.z);
                    }
                }
            }
            catch
            {
                Debug.LogError("Missing meshSliceObjects in json!");
            }

            try
            {
                foreach (SeeSawObject seeSawObject in json.seeSawObjects)
                {
                    GameObject loadedObject;

                    loadedObject = Instantiate(Resources.Load(seeSawObject.objectName) as GameObject, seeSawObject.GetPosition(), Quaternion.Euler(seeSawObject.GetRotation()));

                    Vector3 meshSize = new Vector3(seeSawObject.meshWidth, seeSawObject.meshHeight, seeSawObject.meshDepth);

                    var seeSaw = loadedObject.GetComponent<SBM.Objects.World5.SeeSaw>();
                    seeSaw.platformSize = meshSize;
                    seeSaw.Regenerate();

                    var meshSlice = loadedObject.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();

                    meshSlice.Size = meshSize;
                    meshSlice.Regenerate();

                    loadedObject.GetComponent<SBM.Objects.World5.SeeSaw>().cj.anchor = new Vector3(seeSawObject.pivotPos[0], seeSawObject.pivotPos[1], seeSawObject.pivotPos[2]);
                }
            }
            catch
            {
                Debug.LogError("Missing seeSawObjects in json!");
            }

            try
            {
                foreach (FlipBlockObject flipBlockObject in json.flipBlockObjects)
                {
                    GameObject loadedObject;

                    loadedObject = Instantiate(Resources.Load(flipBlockObject.objectName) as GameObject, flipBlockObject.GetPosition(), Quaternion.Euler(flipBlockObject.GetRotation()));

                    Vector3 meshSize = new Vector3(flipBlockObject.meshWidth, flipBlockObject.meshHeight, flipBlockObject.meshDepth);

                    var flipBlock = loadedObject.GetComponent<SBM.Objects.World5.FlipBlock>();
                    flipBlock.spikesEnabled = flipBlockObject.spikesEnabled;

                    for (int i = 0; i < flipBlock.spikesEnabled.Length; i++)
                    {
                        flipBlock.spikesEnabled = flipBlockObject.spikesEnabled;
                        flipBlock.spikes[i].SetActive(flipBlock.spikesEnabled[i]);
                    }

                    for (int i = 0; i < flipBlock.spikes.Length; i++)
                    {
                        // oscillate between 0, 1, 0, -1 using Sin to determine the x position of the current spike, where 1 = right, -1 = left
                        int xDir = (int)Math.Sin((Math.PI * i) / 2); // 0 1 0 -1
                        int yDir = (int)Math.Cos((Math.PI * i) / 2); // 1 0 -1 0

                        GameObject curSpike = flipBlock.spikes[i];
                        curSpike.transform.localPosition = new Vector3((xDir * flipBlockObject.meshWidth) / 2, (yDir * flipBlockObject.meshHeight) / 2, curSpike.transform.localPosition.z);
                    }

                    flipBlock.timeBetweenFlips = flipBlockObject.flipTime;
                    flipBlock.degreesPerFlip = flipBlockObject.flipDegrees;
                    flipBlock.direction = flipBlockObject.direction ? SBM.Objects.World5.FlipBlock.FlipDirection.Right : SBM.Objects.World5.FlipBlock.FlipDirection.Left;

                    var meshSlice = loadedObject.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();
                    meshSlice.Size = meshSize;
                    meshSlice.Regenerate();
                }
            }
            catch
            {
                Debug.LogError("Missing flipBlockObjects in json!");
            }

            try
            {
                foreach (PistonObject pistonObject in json.pistonObjects)
                {
                    GameObject loadedObject;

                    loadedObject = Instantiate(Resources.Load(pistonObject.objectName) as GameObject, pistonObject.GetPosition(), Quaternion.Euler(pistonObject.GetRotation()));

                    MeshSliceData meshData = loadedObject.transform.root.gameObject.AddComponent<MeshSliceData>();
                    meshData.width = pistonObject.meshWidth;
                    meshData.height = pistonObject.meshHeight;
                    meshData.depth = pistonObject.meshDepth;

                    Vector3 meshSize = new Vector3(pistonObject.meshWidth, pistonObject.meshHeight, pistonObject.meshDepth);

                    var pistonPlatform = loadedObject.GetComponent<SBM.Objects.World5.PistonPlatform>();
                    pistonPlatform.pistonMaxTravel = pistonObject.pistonMaxTravel;
                    pistonPlatform.extraShaftLength = pistonObject.pistonShaftLength;

                    if (pistonObject.keyframes.Count > 0)
                    {
                        pistonPlatform.movement = true;
                        pistonPlatform.normalizedLengthVsTime = new AnimationCurve(pistonObject.keyframes.ToArray());
                    }
                    else
                    {
                        pistonPlatform.movement = false;
                    }

                    pistonPlatform.regenerateNow = true;
                    pistonPlatform.OnValidate();

                    var meshSlice = loadedObject.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();
                    meshSlice.Size = meshSize;
                    meshSlice.Regenerate();
                }
            }
            catch
            {
                Debug.LogError("Missing pistonObjects in json!");
            }

            try
            {
                foreach (RailObject railObject in json.railObjects)
                {
                    GameObject rail = MinecartRailHelper.CreateRailFromObject(railObject, false);
                }
            }
            catch
            {
                Debug.LogError("Missing railObjects in json!");
            }

            try
            {
                foreach (SplineObject splineObject in json.splineObjects)
                {
                    GameObject loadedObject = SplineMakerHelper.CreateSplineFromObject(splineObject, false);
                }
            }
            catch
            {
                Debug.LogError("Missing splineObjects in json!");
            }

            try
            {
                foreach (ColorBlockObject colorBlockObject in json.colorBlockObjects)
                {
                    GameObject loadedObject;

                    if (colorBlockObject.isCorner)
                    {
                        loadedObject = Instantiate(EditorManager.colorBlockCorner, colorBlockObject.GetPosition(), Quaternion.Euler(colorBlockObject.GetRotation()));
                    }
                    else
                    {
                        loadedObject = Instantiate(EditorManager.colorBlock, colorBlockObject.GetPosition(), Quaternion.Euler(colorBlockObject.GetRotation()));
                    }

                    loadedObject.transform.localScale = colorBlockObject.GetScale();
                    loadedObject.GetComponent<MeshRenderer>().material.color = new Color32((byte)colorBlockObject.r, (byte)colorBlockObject.g, (byte)colorBlockObject.b, 255);
                }
            }
            catch
            {
                Debug.LogError("Missing colorBlockObjects in json!");
            }

            GameObject playerSpawn_1 = new GameObject("PlayerSpawn_1", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_1.transform.position = spawnPos_1;

            GameObject playerSpawn_2 = new GameObject("PlayerSpawn_2", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_2.transform.position = spawnPos_2;

            if (spawnPos_3 != new Vector3(0, 0, -999))
            {
                GameObject playerSpawn_3 = new GameObject("PlayerSpawn_3", typeof(SBM.Shared.PlayerSpawnPoint));
                playerSpawn_3.transform.position = spawnPos_3;
            }

            if (spawnPos_4 != new Vector3(0, 0, -999))
            {
                GameObject playerSpawn_4 = new GameObject("PlayerSpawn_4", typeof(SBM.Shared.PlayerSpawnPoint));
                playerSpawn_4.transform.position = spawnPos_4;
            }
            
            if (SBM.Shared.Networking.NetworkSystem.IsInSession)
            {
                // setup all profiles for CO-OP play, modified version of SBM.UI.Components.UIPlayerRoster.ConfigureCoopPlayersForNetworkPlay()

                /*if (SBM.Shared.Networking.NetworkSystem.IsHost)
                {
                    var localProfile = SBM.Shared.PlayerRoster.GetProfile(1);
                    var localUserId = SBM.Shared.Networking.NetworkSystem.LocalUserId;
                    string localUsername = SBM.Shared.Networking.NetworkSystem.LocalUsername;

                    localProfile.Overwrite(0, 0, SBM.Shared.Team.Red, localUserId, true, localUsername);

                    for (int i = 2; i < MultiplayerManager.playerCount; i++)
                    {
                        Debug.Log("Creating Profile... " + i);

                        var profile = SBM.Shared.PlayerRoster.GetProfile(i);
                        var remoteUserId = SBM.Shared.Networking.NetworkSystem.GetRemoteUserId(0);
                        string username = SBM.Shared.Networking.NetworkSystem.GetUsername(remoteUserId);

                        profile.Overwrite(0, 0, SBM.Shared.Team.Red, remoteUserId, false, username);
                    }
                }*/
                
                SBM.Shared.Networking.NetworkSystem.instance.OnSceneEvent(SBM.Shared.SceneEvent.LoadComplete, SceneManager.GetSceneByName("base level"));
                SBM.Shared.Level.LevelSystem.instance.OnSceneEvent(SBM.Shared.SceneEvent.LoadComplete, SceneManager.GetSceneByName("base level"));
            }

            loading = false;

            return worldStyle;
        }

        public void BeginLoadLevel(bool isEditor, bool newLevel, string path, int level, LevelType levelType, World world = null)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Level does not exist!");
            }

            if (File.ReadAllBytes(path).Length == 0 && !isEditor)
            {
                throw new Exception("Empty level attempted to load! Make sure to save in editor first.");
            }

            if (loading)
            {
                return;
            }

            loading = true;

            PreviousSceneName = SceneManager.GetActiveScene().name;

            if (PreviousSceneName == "Systems")
            {
                PreviousSceneName = "";
            }

            if (!isEditor)
            {
                if (world != null)
                {
                    currentWorld = world;
                }

                currentLevel = path;

                if (!LevelManager.InLevel && levelType == LevelType.Story)
                {
                    SBM.Shared.Cameras.MenuCamera menuCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SBM.Shared.Cameras.MenuCamera>();
                    menuCamera.targetPos = new Vector3(-0.715f, 4.05f, 21.96f);
                    menuCamera.targetLookAt = new Vector3(-0.63f, 5.76f, 27.03f);

                    SBM.UI.Utilities.Focus.UIFocusable.FocusedObject.gameObject.transform.parent.GetComponent<SBM.UI.Utilities.Transitioner.UITransitioner>().Transition_Out_To_Right(); //parent of level button is level selector
                    SBM.UI.Utilities.Focus.UIFocusable.ReleaseFocusedObject();
                    wormholeManager.WormholeAnimation(path, level, levelType);
                }
                else
                {
                    InLevel = true;

                    SceneSystem.Unload(PreviousSceneName).completed += delegate (AsyncOperation o)
                    {
                        LoadLevel(isEditor, newLevel, path, levelType);
                    };

                }   
            }
            else
            {
                SceneSystem.Unload(PreviousSceneName).completed += delegate (AsyncOperation o)
                {
                    LoadLevel(isEditor, newLevel, path, LevelManager.LevelType.Editor);
                };
            }
        }

        public string GetNextLevel()
        {
            string nextLevel = "";

            if (currentLevel != null)
            {
                if (currentWorld.levels.Count - 1 > levelNumber-1) // if next level exists... levelNumber-1 since level number is + 1.
                {
                    nextLevel = currentWorld.levels[levelNumber].levelPath;
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
                    RenderSettings.skybox = LevelLoader_Mod.skyboxWorld5;
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
            private LevelType levelType;

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

            public void WormholeAnimation(string _path, int _level, LevelType _levelType)
            {
                ragdolls = GameObject.Find("Ragdolls (Story Mode)").GetComponent<SBM.UI.MainMenu.StoryMode.UIStoryModeRagdolls>();

                path = _path;
                level = _level;
                levelType = _levelType;

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

                                LevelManager.instance.LoadLevel(false, false, transitionManager.path, transitionManager.levelType);
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
