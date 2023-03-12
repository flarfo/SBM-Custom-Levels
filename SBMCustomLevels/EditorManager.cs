using System;
using UnityEngine;
using UnityEngine.UI;
using SplineMesh;
using System.IO;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    internal class EditorManager : MonoBehaviour
    {
        public static EditorManager instance;

        public static Material outlineMask;
        public static Material outlineFill;

        public static GameObject fakeWater;
        public static GameObject iceSledSpikesGuide;
        public static GameObject playerSpawn;

        public List<EditorSelectable> selectableObjects = new List<EditorSelectable>();

        public GameObject background;

        public static bool inEditor = false;

        public string selectedLevel;

        public bool selectTool = false;
        public bool moveTool = false;
        private bool mouseUp = false;

        private Texture2D whiteTexture;
        private bool selectedUI;
        private bool dragSelect;
        private Vector3 dragPosition;

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
        public GameObject spawn1;
        public GameObject spawn2;

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

            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();

            defaultCube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        }

        private void Update()
        {
            if (!inEditor || !editorUI)
            {
                return;
            }

            bool ctrlClicked = false;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                ctrlClicked = true;
            }

            // copy
            if (Input.GetKeyDown(KeyCode.C) && ctrlClicked)
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
                        continue; // dont copy paste wormhole or carrot (should only be one of each)
                    }

                    EditorSelectable newObject = Instantiate(editorSelectable);
                    copiedObjects.Add(newObject);
                    newObject.gameObject.SetActive(false);
                }
            }

            // paste
            if (Input.GetKeyDown(KeyCode.V) && ctrlClicked)
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

                    UndoManager.AddUndo(UndoManager.UndoType.Place, new List<EditorSelectable>(curSelected));
                }
            }

            // cut
            if (Input.GetKeyDown(KeyCode.X) && ctrlClicked)
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

                    editorSelectable.gameObject.SetActive(false);
                }

                UndoManager.AddUndo(UndoManager.UndoType.Delete, new List<EditorSelectable>(curSelected));
                curSelected.Clear();
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                mouseUp = true;

                if (dragSelect == true)
                {
                    SelectWithinBounds(GetScreenRect(dragPosition, Input.mousePosition));
                }

                dragSelect = false;
            }

            // undo
            if (Input.GetKeyDown(KeyCode.Z) && ctrlClicked) 
            {
                UndoManager.Undo();
            }

            // redo
            if (Input.GetKeyDown(KeyCode.Y) && ctrlClicked)
            {
                UndoManager.Redo(); 
            }

            // save
            if (Input.GetKeyDown(KeyCode.S) && ctrlClicked)
            {
                RecordLevel.RecordJSONLevel();

                //set save timestamp
                EditorUI.instance.lastSavedText.text = "Saved: " + DateTime.Now.ToString("HH:mm");
            }

            // delete
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (curSelected.Count == 0)
                {
                    return;
                }

                List<EditorSelectable> deletedObjects = new List<EditorSelectable>();

                foreach (EditorSelectable editorSelectable in curSelected)
                {
                    // dont delete rail nodes, this should be managed by the minecart rail ui
                    if (editorSelectable.gameObject.name == "Node")
                    {
                        editorSelectable.Selected = false;
                        continue;
                    }

                    editorSelectable.gameObject.SetActive(false);   
                    selectableObjects.Remove(editorSelectable);
                    deletedObjects.Add(editorSelectable);
                }

                UndoManager.AddUndo(UndoManager.UndoType.Delete, deletedObjects);
                
                curSelected.Clear();
            }

            // if pointer is over UI, no need to check 3D world for input
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    selectedUI = true;
                }

                return;
            }

            // deselect all objects when right click
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (selectTool)
                {
                    foreach (EditorSelectable selectable in curSelected)
                    {
                        selectable.Selected = false;
                    }

                    curSelected.Clear();
                    EditorUI.instance.EnableInspector(false);
                }
            }

            // selects object under mouse when select tool is active
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (mouseUp && moveTool && curSelected.Count > 0)
                {
                    UndoManager.AddUndo(UndoManager.UndoType.Move, new List<EditorSelectable>(curSelected));
                }

                if (selectTool)
                {
                    selectedUI = false;

                    dragPosition = Input.mousePosition;

                    SelectObject(ctrlClicked);
                    return;
                }
            }

            // moves object under mouse when move tool is active
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (!selectedUI && (dragPosition - Input.mousePosition).magnitude > 10 && selectTool)
                {
                    dragSelect = true;
                }

                if (moveTool)
                {
                    MoveObject();
                    return;
                }
            }
        }

        // draw select box
        private void OnGUI()
        {
            if (dragSelect)
            {
                GUI.color = new Color32(255, 0, 203, 100);
                Rect rect = GetScreenRect(dragPosition, Input.mousePosition);
                GUI.DrawTexture(rect, whiteTexture);
            }
        }

        private Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
        {
            // Move origin from bottom left to top left
            screenPosition1.y = Screen.height - screenPosition1.y;
            screenPosition2.y = Screen.height - screenPosition2.y;
            // Calculate corners
            var topLeft = Vector3.Min(screenPosition1, screenPosition2);
            var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        private void SelectWithinBounds(Rect rect)
        {
            bool objSelected = false;

            foreach (EditorSelectable selectable in selectableObjects)
            {
                Vector3 screenPos = CameraController.camera.WorldToScreenPoint(selectable.transform.position);
                // since: rect y position has 0 at top, 1080 at bottom - camera y position is 1080 at top, 0 at bottom
                Vector3 adjustedScreenPos = new Vector3(screenPos.x, 1080 - screenPos.y, screenPos.z);
                
                if (rect.Contains(adjustedScreenPos))
                {
                    selectable.Selected = true;
                    curSelected.Add(selectable);

                    objSelected = true;
                }
            }

            EditorUI.instance.EnableInspector(objSelected);
        }

        private void SelectObject(bool ctrlClicked)
        {
            //raycast to select object EDITORSELECATBLE
            RaycastHit hit;

            Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                EditorSelectable hitSelectable;

                if (hit.collider.transform.root && hit.collider.gameObject.name != "Node")
                {
                    hitSelectable = hit.collider.transform.root.GetComponent<EditorSelectable>();
                }
                else
                {
                    hitSelectable = hit.collider.GetComponent<EditorSelectable>();
                }
                
                if (!hitSelectable)
                {
                    return;
                }

                bool activateWaterUI = false;
                bool activateRailUI = false;

                //check if water/minecart to enable custom UI
                if (hitSelectable.GetComponent<FakeWater>())
                {
                    activateWaterUI = true;
                }
                else if (hitSelectable.GetComponent<MinecartRailNode>())
                {
                    activateRailUI = true;
                }

                //if control pressed, already selected objects will be deselected, nonselected objects will be appened to selection
                if (ctrlClicked)
                {
                    if (hitSelectable.Selected)
                    {
                        curSelected.Remove(hitSelectable);
                        hitSelectable.Selected = false;

                        if (curSelected.Count == 0)
                        {
                            EditorUI.instance.EnableInspector(false);
                            EditorUI.instance.EnableWaterUI(false);
                            EditorUI.instance.EnableRailUI(false);

                            EditorUI.instance.curWater = null;
                            EditorUI.instance.curRailNode = null;

                            return;
                        }
                    }
                    else
                    {
                        EditorUI.instance.EnableInspector(true);

                        curSelected.Add(hitSelectable);
                        hitSelectable.Selected = true;
                    }

                    if (activateWaterUI)
                    {
                        EditorUI.instance.curWater = hitSelectable.GetComponent<FakeWater>();
                        EditorUI.instance.SetWaterKeyframes();
                        EditorUI.instance.EnableWaterUI(true);
                    }

                    if (activateRailUI)
                    {
                        EditorUI.instance.curRailNode = hitSelectable.GetComponent<MinecartRailNode>();
                        EditorUI.instance.SetRailInformation();
                        EditorUI.instance.EnableRailUI(true);
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

                if (activateWaterUI)
                {
                    EditorUI.instance.curWater = hitSelectable.GetComponent<FakeWater>();
                    EditorUI.instance.SetWaterKeyframes();
                }
                else
                {
                    EditorUI.instance.curWater = null;
                }

                if (activateRailUI)
                {
                    EditorUI.instance.curRailNode = hitSelectable.GetComponent<MinecartRailNode>();
                    EditorUI.instance.SetRailInformation();
                }
                else
                {
                    EditorUI.instance.curRailNode = null;
                }

                EditorUI.instance.EnableInspector(true);
                EditorUI.instance.EnableWaterUI(activateWaterUI);
                EditorUI.instance.EnableRailUI(activateRailUI);

            }
        }

        private void MoveObject()
        {
            foreach (EditorSelectable selectable in curSelected)
            {
                // add selectables to undo stack
                if (mouseUp)
                {
                    selectable.SetMouseOffset();
                }
                
                selectable.MoveObject(snapVector);

                selectable.SetInspectorInfo();
            }

            if (EditorUI.instance.curRailNode)
            {
                EditorUI.instance.SetRailInformation();
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
            Debug.Log("Editor reset!");
            InEditor = false;
            wormhole = null;
            spawn1 = null;
            spawn2 = null;
            selectedLevel = null;
            curSelected = new List<EditorSelectable>();
            selectableObjects = new List<EditorSelectable>();
            copiedObjects = new List<EditorSelectable>();
            snapEnabled = false;
            snapVector = new Vector2(0, 0);
            moveTool = false;
            selectTool = false;
            mouseUp = false;
            background = null;
            worldStyle = 1;

            UndoManager.Reset();
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

            string rawText = File.ReadAllText(path);
            instance.worldStyle = (int)char.GetNumericValue(rawText[0]);
            rawText = rawText.Remove(0, 1);

            instance.background = LevelManager.instance.CreateBackground(instance.worldStyle);

            ObjectContainer json = JsonConvert.DeserializeObject<ObjectContainer>(rawText);

            Vector3 spawnPos_1 = json.spawnPosition1.GetPosition();
            Vector3 spawnPos_2 = json.spawnPosition2.GetPosition();

            foreach (DefaultObject defaultObject in json.defaultObjects) //itearate through default objects, instantiate based on name
            {
                GameObject loadedObject;

                if (defaultObject.objectName == "prefabs\\level\\world2\\IceSledSpikesGuide")
                {
                    loadedObject = Instantiate(iceSledSpikesGuide, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                }
                else
                {
                    loadedObject = Instantiate(Resources.Load(defaultObject.objectName) as GameObject, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                }
                
                loadedObject.transform.localScale = defaultObject.GetScale();

                if (defaultObject.objectName == "prefabs\\level\\Carrot")
                {
                    instance.Carrot = loadedObject;
                }
                else if (defaultObject.objectName == "prefabs\\level\\Wormhole")
                {
                    instance.wormhole = loadedObject;
                }

                AddColliderToObject(loadedObject);

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

                loadedObject = Instantiate(EditorManager.fakeWater, waterObject.GetPosition(), Quaternion.Euler(waterObject.GetRotation()));

                loadedObject.transform.localScale = new Vector3(waterObject.waterWidth, waterObject.waterHeight, 1);

                Debug.Log(loadedObject.transform.localScale.ToString("F4"));

                FakeWater fakeWater = loadedObject.GetComponent<FakeWater>();
                fakeWater.width = waterObject.waterWidth;
                fakeWater.height = waterObject.waterHeight;
                fakeWater.keyframes = waterObject.keyframes;

                loadedObject.AddComponent<Outline>();
                loadedObject.AddComponent<EditorSelectable>();
            }

            foreach (RailObject railObject in json.railObjects)
            {
                GameObject loadedObject = MinecartRailHelper.CreateRailFromObject(railObject, true);

                loadedObject.AddComponent<Outline>();
                loadedObject.AddComponent<EditorSelectable>();
            }

            GameObject playerSpawn_1 = Instantiate(playerSpawn);
            playerSpawn_1.name = "PlayerSpawn_1";
            playerSpawn_1.transform.position = spawnPos_1;
            playerSpawn_1.transform.localScale = new Vector3(1, 2, 1);
            playerSpawn_1.AddComponent<SBM.Shared.PlayerSpawnPoint>();
            playerSpawn_1.AddComponent<Outline>();
            playerSpawn_1.AddComponent<EditorSelectable>();
            instance.spawn1 = playerSpawn_1;

            GameObject playerSpawn_2 = Instantiate(playerSpawn);
            playerSpawn_2.name = "PlayerSpawn_2";
            playerSpawn_2.transform.position = spawnPos_2;
            playerSpawn_2.transform.localScale = new Vector3(1, 2, 1);
            playerSpawn_2.AddComponent<SBM.Shared.PlayerSpawnPoint>();
            playerSpawn_2.AddComponent<Outline>();
            playerSpawn_2.AddComponent<EditorSelectable>();
            instance.spawn2 = playerSpawn_2;
        }

        //creates a new base level in editor mode
        public static void LoadNewEditorLevel()
        {
            InEditor = true;
            instance.worldStyle = 1;

            instance.background = LevelManager.instance.CreateBackground(1);

            GameObject playerSpawn_1 = Instantiate(playerSpawn);
            playerSpawn_1.name = "PlayerSpawn_1";
            playerSpawn_1.transform.position = new Vector3(0,0,0);
            playerSpawn_1.transform.localScale = new Vector3(1, 2, 1);
            playerSpawn_1.AddComponent<SBM.Shared.PlayerSpawnPoint>();
            playerSpawn_1.AddComponent<Outline>();
            playerSpawn_1.AddComponent<EditorSelectable>();
            instance.spawn1 = playerSpawn_1;

            GameObject playerSpawn_2 = Instantiate(playerSpawn);
            playerSpawn_2.name = "PlayerSpawn_2";
            playerSpawn_2.transform.position = new Vector3(1, 0, 0);
            playerSpawn_2.transform.localScale = new Vector3(1, 2, 1);
            playerSpawn_2.AddComponent<SBM.Shared.PlayerSpawnPoint>();
            playerSpawn_2.AddComponent<Outline>();
            playerSpawn_2.AddComponent<EditorSelectable>();
            instance.spawn2 = playerSpawn_2;
        }

        public void InitializeEditor()
        {
            //create editor ui
            AssetBundle loadedBundle = LevelLoader_Mod.GetAssetBundleFromResources("ui-bundle");

            EditorUI.keyframeUI = loadedBundle.LoadAsset<GameObject>("Keyframe");

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

        public static void AddColliderToObject(GameObject go)
        {
            //ensure that a spawned object has some form of collider to detect raycast (mouse click) for selection
            if (go.TryGetComponent(out MeshCollider meshCollider))
            {
                meshCollider.enabled = true;
            }
            else if (go.GetComponent<Collider>())
            {

            }
            else if (!go.GetComponent<Collider>() && go.GetComponent<MeshFilter>())
            {
                go.AddComponent<MeshCollider>();
            }
            else
            {
                MeshCollider[] meshColliders = go.GetComponentsInChildren<MeshCollider>();

                if (meshColliders.Length != 0)
                {
                    foreach (MeshCollider collider in meshColliders)
                    {
                        collider.enabled = true;
                    }
                }
                else
                {
                    MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();

                    if (meshFilters.Length != 0)
                    {
                        foreach (MeshFilter filter in meshFilters)
                        {
                            filter.gameObject.AddComponent<MeshCollider>();
                        }
                    }
                }
            }
        }
    }

    public static class UndoManager
    {
        private static Stack<UndoStruct> undoStack = new Stack<UndoStruct>();
        private static Stack<UndoStruct> redoStack = new Stack<UndoStruct>();

        public enum UndoType
        {
            Place,
            Delete,
            Move,
            Rotate,
            Scale,
            AddRailNode,
            DeleteRailNode
        }

        #region Undo
        /// <summary>
        /// Adds an undo event to the top of the undo stack.
        /// </summary
        public static void AddUndo(UndoType undoType, List<EditorSelectable> editorObjects, bool wasRedo = false, MinecartRailNode railNode = null)
        {
            if (!wasRedo)
            {
                redoStack.Clear(); // clear redo stack after a new action is performed
            }

            UndoStruct undo = new UndoStruct();
            undo.undoType = undoType;
            undo.undoObjects = new List<UndoObject>();
            undo.editorObjects = editorObjects;
            
            if (railNode)
            {
                undo.railNode = railNode;
                Debug.Log(railNode.railSpline.nodes.Count);
                undo.railNodeIndex = railNode.railSpline.nodes.IndexOf(railNode.node);
            }

            for (int i = 0; i < editorObjects.Count; i++)
            {
                UndoObject undoObject = new UndoObject();

                undoObject.position = editorObjects[i].gameObject.transform.position;
                undoObject.rotation = editorObjects[i].gameObject.transform.rotation.eulerAngles;
                undoObject.scale = editorObjects[i].gameObject.transform.localScale;

                undo.undoObjects.Add(undoObject);
            }

            undoStack.Push(undo);
        }

        /// <summary>
        /// Undoes the event at the top of the undo stack.
        /// </summary
        public static void Undo()
        {
            if (undoStack.Count == 0)
            {
                Debug.Log("Nothing to undo.");
                return;
            }

            UndoStruct undo = undoStack.Pop();

            switch (undo.undoType)
            {
                case UndoType.Place:
                    UndoPlace(undo);
                    break;
                case UndoType.Delete:
                    UndoDelete(undo);
                    break;
                case UndoType.Move:
                    UndoMove(undo);
                    break;
                case UndoType.Rotate:
                    UndoRotate(undo);
                    break;
                case UndoType.Scale:
                    UndoScale(undo);
                    break;
                case UndoType.AddRailNode:
                    UndoAddRailNode(undo);
                    break;
                case UndoType.DeleteRailNode:
                    UndoDeleteRailNode(undo);
                    break;
            }
        }

        private static void UndoPlace(UndoStruct undo)
        {
            AddRedo(UndoType.Place, new List<EditorSelectable>(undo.editorObjects));

            foreach (EditorSelectable editorObject in undo.editorObjects)
            {
                editorObject.gameObject.SetActive(false);
                EditorManager.instance.curSelected.Remove(editorObject);
                EditorManager.instance.selectableObjects.Remove(editorObject);
            }
        }

        private static void UndoDelete(UndoStruct undo)
        {
            AddRedo(UndoType.Delete, new List<EditorSelectable>(undo.editorObjects));

            foreach (EditorSelectable editorObject in undo.editorObjects)
            {
                editorObject.gameObject.SetActive(true);
                EditorManager.instance.selectableObjects.Add(editorObject);
            }
        }

        private static void UndoMove(UndoStruct undo)
        {
            AddRedo(UndoType.Move, new List<EditorSelectable>(undo.editorObjects));

            int i = 0;
            foreach (EditorSelectable editorObject in undo.editorObjects)
            {
                editorObject.transform.position = undo.undoObjects[i].position;
                i++;
            }
        }

        private static void UndoRotate(UndoStruct undo)
        {
            AddRedo(UndoType.Rotate, new List<EditorSelectable>(undo.editorObjects));

            int i = 0;
            foreach (EditorSelectable editorObject in undo.editorObjects)
            {
                editorObject.transform.rotation = Quaternion.Euler(undo.undoObjects[i].rotation);
                i++;
            }
        }

        private static void UndoScale(UndoStruct undo)
        {
            AddRedo(UndoType.Scale, new List<EditorSelectable>(undo.editorObjects));

            int i = 0;
            foreach (EditorSelectable editorObject in undo.editorObjects)
            {
                editorObject.transform.localScale = undo.undoObjects[i].scale;
                i++;
            }
        }

        private static void UndoAddRailNode(UndoStruct undo)
        {
            AddRedo(UndoType.AddRailNode, new List<EditorSelectable>(undo.editorObjects), undo.railNode);

            undo.railNode.gameObject.SetActive(false);
            undo.railNode.railSpline.RemoveNode(undo.railNode.node);
        }

        /// <summary>
        /// Undoes the deletion of a MinecartRailNode to the spline.
        /// The MinecartRailNode must be index 0 in the UndoStruct.editorObjects for this to work properly.
        /// </summary>
        private static void UndoDeleteRailNode(UndoStruct undo)
        {
            AddRedo(UndoType.DeleteRailNode, new List<EditorSelectable>(undo.editorObjects), undo.railNode);

            undo.railNode.gameObject.SetActive(true);
            
            if (undo.railNode.railSpline.nodes.Count <= undo.railNodeIndex)
            {
                undo.railNode.railSpline.AddNode(undo.railNode.node);
            }
            else
            {
                undo.railNode.railSpline.InsertNode(undo.railNodeIndex, undo.railNode.node);
            }
        }
        #endregion

        #region Redo
        /// <summary>
        /// Adds a redo event to the top of the redo stack.
        /// If the object is to be destroyed, call this method BEFORE the destruction.
        /// </summary
        private static void AddRedo(UndoType undoType, List<EditorSelectable> editorObjects, MinecartRailNode railNode = null)
        {
            UndoStruct redo = new UndoStruct();
            redo.undoType = undoType;
            redo.undoObjects = new List<UndoObject>();
            redo.editorObjects = editorObjects;

            if (railNode)
            {
                redo.railNode = railNode;
                redo.railNodeIndex = railNode.railSpline.nodes.IndexOf(railNode.node);
            }

            for (int i = 0; i < editorObjects.Count; i++)
            {
                UndoObject redoObject = new UndoObject();

                redoObject.position = editorObjects[i].gameObject.transform.position;
                redoObject.rotation = editorObjects[i].gameObject.transform.rotation.eulerAngles;
                redoObject.scale = editorObjects[i].gameObject.transform.localScale;

                redo.undoObjects.Add(redoObject);
            }

            redoStack.Push(redo);
        }

        /// <summary>
        /// Redoes the event at the top of the redo stack.
        /// </summary
        public static void Redo()
        {
            if (redoStack.Count == 0)
            {
                Debug.Log("Nothing to redo.");
                return;
            }

            UndoStruct redo = redoStack.Pop();

            switch (redo.undoType)
            {
                case UndoType.Place:
                    RedoPlace(redo);
                    break;
                case UndoType.Delete:
                    RedoDelete(redo);
                    break;
                case UndoType.Move:
                    RedoMove(redo);
                    break;
                case UndoType.Rotate:
                    RedoRotate(redo);
                    break;
                case UndoType.Scale:
                    RedoScale(redo);
                    break;
                case UndoType.AddRailNode:
                    RedoAddRailNode(redo);
                    break;
                case UndoType.DeleteRailNode:
                    RedoDeleteRailNode(redo);
                    break;
            }
        }

        private static void RedoPlace(UndoStruct redo)
        {
            AddUndo(UndoType.Place, new List<EditorSelectable>(redo.editorObjects), true);

            foreach (EditorSelectable editorObject in redo.editorObjects)
            {
                editorObject.gameObject.SetActive(true);
                EditorManager.instance.selectableObjects.Add(editorObject);
            }

            Debug.Log(EditorManager.instance.curSelected.Count);
        }

        private static void RedoDelete(UndoStruct redo)
        {
            AddUndo(UndoType.Delete, new List<EditorSelectable>(redo.editorObjects), true);

            foreach (EditorSelectable editorObject in redo.editorObjects)
            {
                editorObject.gameObject.SetActive(false);
                EditorManager.instance.curSelected.Remove(editorObject);
                EditorManager.instance.selectableObjects.Remove(editorObject);
            }
        }

        private static void RedoMove(UndoStruct redo)
        {
            AddUndo(UndoType.Move, new List<EditorSelectable>(redo.editorObjects), true);

            int i = 0;
            foreach (EditorSelectable editorObject in redo.editorObjects)
            {
                editorObject.transform.position = redo.undoObjects[i].position;
                i++;
            }
        }

        private static void RedoRotate(UndoStruct redo)
        {
            AddUndo(UndoType.Rotate, new List<EditorSelectable>(redo.editorObjects), true);

            int i = 0;
            foreach (EditorSelectable editorObject in redo.editorObjects)
            {
                editorObject.transform.rotation = Quaternion.Euler(redo.undoObjects[i].rotation);
                i++;
            }
        }

        private static void RedoScale(UndoStruct redo)
        {
            AddUndo(UndoType.Scale, new List<EditorSelectable>(redo.editorObjects), true);

            int i = 0;
            foreach (EditorSelectable editorObject in redo.editorObjects)
            {
                editorObject.transform.localScale = redo.undoObjects[i].scale;
                i++;
            }
        }

        private static void RedoAddRailNode(UndoStruct redo)
        {
            // spline cannot have fewer than two nodes
            if (redo.railNode.railSpline.nodes.Count > 2)
            {
                AddUndo(UndoType.AddRailNode, new List<EditorSelectable>(redo.editorObjects), true, redo.railNode);

                redo.railNode.gameObject.SetActive(true);
                redo.railNode.railSpline.InsertNode(redo.railNodeIndex, redo.railNode.node);
            }
        }

        private static void RedoDeleteRailNode(UndoStruct redo)
        {
            AddUndo(UndoType.DeleteRailNode, new List<EditorSelectable>(redo.editorObjects), true, redo.railNode);

            redo.railNode.gameObject.SetActive(false);
            redo.railNode.railSpline.RemoveNode(redo.railNode.node);
        }
        #endregion

        /// <summary>
        /// Clears all data stored in the UndoManager.
        /// </summary>
        public static void Reset()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        public struct UndoStruct
        {
            public UndoType undoType;
            public List<EditorSelectable> editorObjects;
            public List<UndoObject> undoObjects;

            // minecart raile specific, can be ignored otherwise
            public MinecartRailNode railNode;
            public int railNodeIndex;
        }

        public struct UndoObject
        {
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;
        }
    }
}
