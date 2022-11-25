using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    internal class EditorManager : MonoBehaviour
    {
        public static EditorManager instance;

        public GameObject background;

        private static bool inEditor = false;

        public string selectedLevel;

        public bool selectTool = false;
        public bool moveTool = false;

        private bool mouseUp = false;

        private Vector2 snapVector = new Vector2(0, 0);
        public bool snapEnabled = false;

        public Camera editorCamera;

        private GameObject editorUI;

        public int worldStyle = 1;

        public static Mesh defaultCube;

        private GameObject carrot;
        public GameObject Carrot 
        {
            get
            {
                return carrot;
            }
            set
            {
                Destroy(carrot);

                carrot = value;
            }
        }

        public GameObject wormhole;

        public static bool InEditor 
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
        public List<EditorSelectable> copiedObjects = new List<EditorSelectable>();


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

            defaultCube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        }

        private void Update()
        {
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

            if (Input.GetKeyDown(KeyCode.C) && ctrlClicked) //copy objects
            {
                foreach (EditorSelectable editorSelectable in copiedObjects)
                {
                    Destroy(editorSelectable.gameObject);
                }

                copiedObjects.Clear();

                foreach (EditorSelectable editorSelectable in curSelected)
                {
                    if (editorSelectable.gameObject.name.Contains("Wormhole") || editorSelectable.gameObject.name.Contains("Carrot"))
                    {
                        continue; //dont copy paste wormhole or carrot (should only be one of each)
                    }

                    EditorSelectable newObject = Instantiate(editorSelectable);
                    copiedObjects.Add(newObject);
                    newObject.gameObject.SetActive(false);
                }
            }

            if (Input.GetKeyDown(KeyCode.V) && ctrlClicked) //paste objects
            {
                if (copiedObjects.Count > 0)
                {
                    foreach (EditorSelectable editorSelectable in curSelected)
                    {
                        editorSelectable.Selected = false;
                    }

                    curSelected.Clear();

                    foreach (EditorSelectable editorSelectable in copiedObjects)
                    {
                        EditorSelectable newObject = Instantiate(editorSelectable);
                        newObject.gameObject.SetActive(true);

                        newObject.GetComponent<Outline>().FixInstantiated();

                        newObject.Selected = true;
                        newObject.gameObject.name = newObject.gameObject.name.Replace("(Clone)", "");

                        curSelected.Add(newObject);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.X) && ctrlClicked) //cut objects
            {
                foreach (EditorSelectable editorSelectable in copiedObjects)
                {
                    Destroy(editorSelectable.gameObject);
                }

                copiedObjects.Clear();

                foreach (EditorSelectable editorSelectable in curSelected)
                {
                    EditorSelectable newObject = Instantiate(editorSelectable);
                    copiedObjects.Add(newObject);
                    newObject.gameObject.SetActive(false);

                    Destroy(editorSelectable.gameObject);
                }

                curSelected.Clear();
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                mouseUp = true;
            }

            if (Input.GetKeyDown(KeyCode.S) && ctrlClicked)
            {
                RecordLevel.RecordJSONLevel();

                //set save timestamp
                EditorUI.instance.lastSavedText.text = "Saved: " + DateTime.Now.ToString("HH:mm");
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                foreach (EditorSelectable editorSelectable in curSelected)
                {
                    Destroy(editorSelectable.gameObject);
                }

                curSelected.Clear();
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
                EditorSelectable hitSelectable;

                if (hit.collider.transform.root)
                {
                    hitSelectable = hit.collider.transform.root.GetComponent<EditorSelectable>();
                }
                else
                {
                    hitSelectable = hit.collider.GetComponent<EditorSelectable>();
                }
                 

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

        public void SetSnapX(float snapX)
        {
            snapVector.x = snapX;
            
        }

        public void SetSnapY(float snapY)
        {
            snapVector.y = snapY;
        }

        //resets editor so data doesnt carry over into new editor instances
        public void ResetEditor()
        {
            Debug.LogError("Editor reset!");
            wormhole = null;
            selectedLevel = null;
            curSelected = new List<EditorSelectable>();
            copiedObjects = new List<EditorSelectable>();
            snapEnabled = false;
            snapVector = new Vector2(0, 0);
            moveTool = false;
            selectTool = false;
            mouseUp = false;
            background = null;
            worldStyle = 1;
        }

        //loads EXISTING level in editor mode
        public static void LoadEditorLevel(string path)
        {
            InEditor = true;

            if (!Directory.Exists(LevelLoader_Mod.levelsPath))
            {
                Directory.CreateDirectory(LevelLoader_Mod.levelsPath);
            }

            if (!File.Exists(path))
            {
                return;
            }

            string rawText = File.ReadAllText(path).Remove(0, 1);

            ObjectContainer json = JsonConvert.DeserializeObject<ObjectContainer>(rawText);

            Vector3 spawnPos_1 = json.spawnPosition1.GetPosition();
            Vector3 spawnPos_2 = json.spawnPosition2.GetPosition();

            foreach (DefaultObject defaultObject in json.defaultObjects) //itearate through default objects, instantiate based on name
            {
                GameObject loadedObject;

                loadedObject = Instantiate(Resources.Load(defaultObject.objectName) as GameObject, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                loadedObject.transform.localScale = defaultObject.GetScale();

                loadedObject.AddComponent<Outline>();
                loadedObject.AddComponent<EditorSelectable>();

                if (loadedObject.layer == 10)
                {
                    loadedObject.layer = 0;
                }
            }

            foreach (WaterObject waterObject in json.waterObjects) //iterate through water objects, apply separate water logic (height, width via component)
            {
                GameObject loadedObject;

                loadedObject = Instantiate(LevelLoader_Mod.fakeWater, waterObject.GetPosition(), Quaternion.Euler(waterObject.GetRotation()));

                loadedObject.transform.localScale = new Vector3(waterObject.waterWidth, waterObject.waterHeight, 1);

                FakeWater fakeWater = loadedObject.GetComponent<FakeWater>();
                fakeWater.width = waterObject.waterWidth;
                fakeWater.height = waterObject.waterHeight;

                loadedObject.AddComponent<Outline>();
                loadedObject.AddComponent<EditorSelectable>();
            }

            GameObject playerSpawn_1 = new GameObject("PlayerSpawn_1", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_1.transform.position = spawnPos_1;

            GameObject playerSpawn_2 = new GameObject("PlayerSpawn_2", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_2.transform.position = spawnPos_2;
        }

        //creates a new base level in editor mode
        public static void LoadNewEditorLevel()
        {
            InEditor = true;

            GameObject carrot = Instantiate(Resources.Load(Path.Combine("prefabs", "level", "Carrot"))) as GameObject;
            carrot.AddComponent<Outline>();
            carrot.AddComponent<EditorSelectable>();

            GameObject playerSpawn_1 = new GameObject("PlayerSpawn_1", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_1.transform.position = new Vector3(0, 0, 0);

            GameObject playerSpawn_2 = new GameObject("PlayerSpawn_2", typeof(SBM.Shared.PlayerSpawnPoint));
            playerSpawn_2.transform.position = new Vector3(0.5f, 0, 0);

            /*SBM.Objects.Common.Cloud.CloudSpawner cloudSpawner = Instantiate(Resources.Load("prefabs/level/Cloud Spawner") as GameObject).GetComponent<SBM.Objects.Common.Cloud.CloudSpawner>();
            cloudSpawner.CloudPrefabs.Clear();
            cloudSpawner.CloudPrefabs.Add(Resources.Load("prefabs/level/world1/Cloud_W1") as GameObject);*/
        }

        public void InitializeEditor()
        {
            //create editor ui
            AssetBundle loadedBundle = LevelLoader_Mod.GetAssetBundleFromResources("ui-bundle");

            editorUI = Instantiate(loadedBundle.LoadAsset<GameObject>("EditorUI"));

            editorUI.AddComponent<EditorUI>();

            loadedBundle.Unload(false);
        }

        private static void OnEditor(bool editorEnabled)
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
