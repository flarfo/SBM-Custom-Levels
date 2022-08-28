using UnityEngine;
using HarmonyLib;
using System.IO;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace SBM_CustomLevels
{
    internal class EditorManager : MonoBehaviour
    {
        public static EditorManager instance;

        private bool inEditor = false;

        public string selectedLevel;

        public bool selectTool = false;
        public bool moveTool = false;

        private bool mouseUp = false;

        private Vector2 snapVector = new Vector2(0, 0);
        public bool snapEnabled = false;

        public Camera editorCamera;

        private GameObject editorUI;

        public bool InEditor 
        {
            get
            {
                return inEditor;
            } 
            set 
            {
                inEditor = value;

                OnEditor(value);
            } 
        }

        public Material outlineMask;
        public Material outlineFill;

        //currently selected objects
        public List<EditorSelectable> curSelected = new List<EditorSelectable>();

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

            //load materials
            AssetBundle loadedBundle = LevelLoader_Mod.GetAssetBundleFromResources("sbm-bundle");

            //materials used for EditorSelectable outlines
            outlineMask = loadedBundle.LoadAsset<Material>("OutlineMask");
            outlineFill = loadedBundle.LoadAsset<Material>("OutlineFill");

            loadedBundle.Unload(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                RecordLevel.RecordJSONLevel();
            }

            if (!inEditor)
            {
                return;
            }

            //if pointer is over UI, no need to check 3D world for input
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            bool ctrlClicked = false;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                ctrlClicked = true;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                mouseUp = true;
            }

            //selects object under mouse when select tool is active
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (selectTool)
                {
                    SelectObject(ctrlClicked);
                    return;
                }
            }

            //moves object under mouse when move tool is active
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (moveTool)
                {
                    MoveObject();
                    return;
                }
            }
        }

        private void SelectObject(bool ctrlClicked)
        {
            //raycast to select object EDITORSELECATBLE
            RaycastHit hit;

            Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                EditorSelectable hitSelectable = hit.collider.GetComponent<EditorSelectable>();

                if (hitSelectable)
                {
                    //if control pressed, already selected objects will be deselected, nonselected objects will be appened to selection
                    if (ctrlClicked)
                    {
                        if (hitSelectable.Selected)
                        {
                            curSelected.Remove(hitSelectable);
                            hitSelectable.Selected = false;
                        }
                        else
                        {
                            curSelected.Add(hitSelectable);
                            hitSelectable.Selected = true;
                        }

                        return;
                    }

                    //disable all other selections
                    foreach (EditorSelectable selectable in curSelected)
                    {
                        selectable.Selected = false;
                    }

                    if (!hitSelectable.Selected)
                    {
                        curSelected = new List<EditorSelectable>() { hitSelectable };
                        hitSelectable.Selected = true;
                    }
                }
            }
        }

        private void MoveObject()
        {
            foreach (EditorSelectable selectable in curSelected)
            {
                if (mouseUp)
                {
                    selectable.SetMouseOffset();
                }
                
                selectable.MoveObject(snapVector);

                selectable.SetInspectorInfo();
            }

            //mouseup makes sure setmouseoffset isnt constantly running, making objects not move
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                mouseUp = false;
            }
        }

        //resets editor so data doesnt carry over into new editor instances
        public void ResetEditor()
        {
            Debug.LogError("Editor reset!");

            InEditor = false;

            selectedLevel = null;

            curSelected = new List<EditorSelectable>();
            snapVector = new Vector2(0, 0);

            moveTool = false;
            selectTool = false;

            mouseUp = false;
        }

        //loads EXISTING level in editor mode
        public static void LoadEditorLevel(string path)
        {
            instance.InEditor = true;

            if (!Directory.Exists(LevelLoader_Mod.levelsPath))
            {
                Directory.CreateDirectory(LevelLoader_Mod.levelsPath);
            }

            if (!File.Exists(path))
            {
                return;
            }

            string[] jsonLines = File.ReadAllLines(path);

            JsonObject jsonObject;

            Vector3 spawnPos_1 = (JsonUtility.FromJson(jsonLines[0], typeof(FloatObject)) as FloatObject).GetPosition();
            Vector3 spawnPos_2 = (JsonUtility.FromJson(jsonLines[1], typeof(FloatObject)) as FloatObject).GetPosition();

            for (int i = 2; i < jsonLines.Length; i++)
            {
                jsonObject = JsonUtility.FromJson(jsonLines[i], typeof(JsonObject)) as JsonObject;

                GameObject loadedObject = GameObject.Instantiate(Resources.Load(jsonObject.objectName) as GameObject, jsonObject.GetPosition(), Quaternion.Euler(jsonObject.GetRotation()));

                loadedObject.transform.localScale = jsonObject.GetScale();

                loadedObject.AddComponent<Outline>();

                loadedObject.AddComponent<EditorSelectable>();

                if (!loadedObject.GetComponent<Collider>())
                {
                    //current hotfix, add custom collider versions of certain objects (palm trees, signs, etc. just use thin box colliders or load mesh from bundle?
                    loadedObject.AddComponent<MeshCollider>();
                    //loadedObject.AddComponent<BoxCollider>();
                }
            }

            //set positions (first 2 lines of json file?)
            GameObject playerSpawn_1 = new GameObject("PlayerSpawn_1", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_1.transform.position = spawnPos_1;

            GameObject playerSpawn_2 = new GameObject("PlayerSpawn_2", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_2.transform.position = spawnPos_2;
        }

        //creates a new base level in editor mode
        public static void LoadNewEditorLevel()
        {
            instance.InEditor = true;

            Instantiate(Resources.Load("prefabs/level/Carrot"));

            GameObject playerSpawn_1 = new GameObject("PlayerSpawn_1", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_1.transform.position = new Vector3(0, 0, 0);

            GameObject playerSpawn_2 = new GameObject("PlayerSpawn_2", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_2.transform.position = new Vector3(0.5f, 0, 0);

            SBM.Objects.Common.Cloud.CloudSpawner cloudSpawner = Instantiate(Resources.Load("prefabs/level/Cloud Spawner") as GameObject).GetComponent<SBM.Objects.Common.Cloud.CloudSpawner>();
            cloudSpawner.CloudPrefabs.Clear();
            cloudSpawner.CloudPrefabs.Add(Resources.Load("prefabs/level/world1/Cloud_W1") as GameObject);
        }

        public void InitializeEditor()
        {
            Camera.main.gameObject.AddComponent<CameraController>();

            editorCamera = Camera.main;

            //create editor ui
            AssetBundle loadedBundle = LevelLoader_Mod.GetAssetBundleFromResources("ui-bundle");

            editorUI = Instantiate(loadedBundle.LoadAsset<GameObject>("EditorUI"));

            editorUI.AddComponent<EditorUI>();

            loadedBundle.Unload(false);
        }

        private void OnEditor(bool editorEnabled)
        {
            Physics.autoSimulation = !editorEnabled;

            Cursor.visible = editorEnabled;

            if (editorEnabled)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            
        }
    }
}
