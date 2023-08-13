﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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
        public static GameObject scaffoldingBlock;
        public static GameObject scaffoldingCorner;
        public static GameObject scaffoldPanelBlack;
        public static GameObject scaffoldPanelBrown;
        public static GameObject colorBlock;
        public static GameObject colorBlockCorner;

        public List<EditorSelectable> selectableObjects = new List<EditorSelectable>();

        public GameObject background;

        public static bool inEditor = false;

        public string selectedLevel;

        public bool selectTool = false;
        public bool moveTool = false;
        public bool stampTool = false;
        private bool mouseUp = false;

        private Texture2D whiteTexture;
        private bool selectedUI;
        private bool dragSelect;
        private Vector3 dragPosition;

        private Vector2 snapVector = new Vector2(0, 0);
        public bool snapEnabled = false;

        public EditorSelectable ghostItem = null;

        public Camera editorCamera;

        private GameObject editorUI;

        public int worldStyle = 1;

        public static Mesh defaultCube;

        public GameObject carrot;

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
            // debug
            /*if (Input.GetKeyDown(KeyCode.PageDown))
            {
                if (!SBM.Shared.Networking.NetworkSystem.IsInSession)
                {
                    return;
                }

                if (SBM.Shared.Networking.NetworkSystem.IsHost)
                {
                    SBM.Shared.Networking.NetworkSystem.InviteViaServiceOverlay();

                    if (MultiplayerManager.playerCount > 1)
                    {
                        // SBM.Shared.Networking.NetworkSystem.InviteViaServiceOverlay();
                    }
                }
            }
            
            // debug
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                foreach (var player in SBM.Shared.PlayerRoster.profiles)
                {
                    Debug.Log($"{player.BaseUsername} {player.PlayerNumber}: Device {player.InputDeviceIndex}");
                }

                Debug.Log("Player Count: " + MultiplayerManager.playerCount);
            }
            // end*/

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
                    if (editorSelectable.gameObject.name.Contains("Wormhole") || editorSelectable.gameObject.name.Contains("Carrot") || editorSelectable.gameObject.name.Contains("PlayerSpawn"))
                    {
                        continue; // dont copy paste wormhole, carrot, or playerspawn (should only be one/two of each)
                    }

                    // dont delete rail nodes, this should be managed by the minecart rail ui
                    if (editorSelectable.gameObject.name.Contains("Node"))
                    {
                        continue; // dont copy paste rail node, these should be managed by railnode UI
                    }

                    editorSelectable.Selected = false;

                    EditorSelectable newObject = Instantiate(editorSelectable);

                    editorSelectable.Selected = true;

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
                    // dont delete rail nodes, this should be managed by the minecart rail ui
                    if (editorSelectable.gameObject.name.Contains("Node") || editorSelectable.gameObject.name.Contains("Wormhole") || editorSelectable.gameObject.name.Contains("Carrot"))
                    {
                        editorSelectable.Selected = false;
                        continue;
                    }

                    editorSelectable.Selected = false;
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

                if (ghostItem)
                {
                    if (ghostItem.isActiveAndEnabled)
                    {

                        ghostItem.Selected = false;
                        curSelected.Remove(ghostItem);
                        selectableObjects.Remove(ghostItem);
                        Destroy(ghostItem.gameObject);

                        ghostItem = null;
                    }
                }

                foreach (EditorSelectable editorSelectable in curSelected)
                {
                    // dont delete rail nodes, this should be managed by the minecart rail ui
                    if (editorSelectable.gameObject.name.Contains("Node"))
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
                    EditorUI.instance.EnableObjSettingsUI(false);
                    EditorUI.instance.EnableRailUI(false);
                    EditorUI.instance.EnableWaterUI(false);
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

                if (stampTool)
                {
                    // place current prefab
                    if (ghostItem)
                    {
                        EditorSelectable newSelectable;
                        ghostItem.Selected = false;
                        
                        // minecart rails must be spawned using helper function, otherwise they won't work as intended
                        if (ghostItem.name == "MinecartRail")
                        {
                             GameObject newRail = MinecartRailHelper.SpawnNewRail(ghostItem.transform.position);
                             newRail.AddComponent<Outline>();
                             newSelectable = newRail.AddComponent<EditorSelectable>();
                        }
                        else
                        {
                            newSelectable = Instantiate(ghostItem);
                        }

                        if (ctrlClicked)
                        {
                            curSelected.Add(newSelectable);
                            newSelectable.Selected = true;
                        }

                        ghostItem.Selected = true;

                        UndoManager.AddUndo(UndoManager.UndoType.Place, new List<EditorSelectable> { newSelectable });
                    }

                    return;
                }
            }

            // move ghostItem with mouse cursor
            if (ghostItem)
            {
                ghostItem.MoveObjectToMouse(snapVector);

                if (stampTool)
                {
                    ghostItem.SetInspectorInfo(true, false, false);
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

            foreach (EditorSelectable selectable in curSelected)
            {
                selectable.Selected = false;
            }

            curSelected.Clear();

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

                if (hit.collider.transform.root && !hit.collider.gameObject.name.Contains("Node"))
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
                bool activateSplineUI = false;
                bool activatePistonUI = false;
                bool activateObjSettingsUI = false;
                bool activateColorUI = false;

                //check if water/minecart/flipblock etc to enable custom UI
                if (hitSelectable.GetComponent<WaterDataContainer>())
                {
                    activateWaterUI = true;
                }
                else if (hitSelectable.GetComponent<PistonDataContainer>())
                {
                    activatePistonUI = true;
                    activateObjSettingsUI = true;
                }
                else if (hitSelectable.GetComponent<SplineMeshNodeData>() && hitSelectable.gameObject.name == "RailNode")
                {
                    activateRailUI = true;
                }
                else if (hitSelectable.GetComponent<SplineMakerNodeData>() && hitSelectable.gameObject.name == "SplineNode")
                { 
                    activateSplineUI = true;
                }
                else if (hitSelectable.GetComponent<MeshSliceData>())
                {
                    activateObjSettingsUI = true;
                }
                else if (hitSelectable.GetComponent<ColorData>())
                {
                    activateColorUI = true;
                }

                //if control pressed, already selected objects will be deselected, nonselected objects will be appended to selection
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
                            EditorUI.instance.EnablePistonUI(false);
                            EditorUI.instance.EnableRailUI(false);
                            EditorUI.instance.EnableObjSettingsUI(false);
                            EditorUI.instance.EnableColorUI(false);

                            EditorUI.instance.curWater = null;
                            EditorUI.instance.curPiston = null;
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
                        EditorUI.instance.curWater = hitSelectable.GetComponent<WaterDataContainer>();
                        EditorUI.instance.SetWaterKeyframes();
                        EditorUI.instance.EnableWaterUI(true);
                    }

                    if (activatePistonUI)
                    {
                        EditorUI.instance.curPiston = hitSelectable.GetComponent<PistonDataContainer>();
                        EditorUI.instance.SetPistonKeyframes();
                        EditorUI.instance.EnablePistonUI(true);
                    }

                    if (activateRailUI)
                    {
                        EditorUI.instance.curRailNode = hitSelectable.GetComponent<SplineMeshNodeData>();
                        EditorUI.instance.SetRailInformation();
                        EditorUI.instance.EnableRailUI(true);
                    }

                    if (activateSplineUI)
                    {
                        EditorUI.instance.curSplineNode = hitSelectable.GetComponent<SplineMakerNodeData>();
                        EditorUI.instance.SetSplineInformation();
                    }
                    
                    if (activateObjSettingsUI)
                    {
                        EditorUI.instance.EnableObjSettingsUI(true);
                        EditorUI.instance.SetObjSettingsInformation(hitSelectable.gameObject);
                    }

                    if (activateColorUI)
                    {
                        EditorUI.instance.EnableColorUI(true);
                        EditorUI.instance.SetColorInformation(hitSelectable.gameObject);
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
                    EditorUI.instance.curWater = hitSelectable.GetComponent<WaterDataContainer>();
                    EditorUI.instance.SetWaterKeyframes();
                }
                else
                {
                    EditorUI.instance.curWater = null;
                }

                if (activatePistonUI)
                {
                    EditorUI.instance.curPiston = hitSelectable.GetComponent<PistonDataContainer>();
                    EditorUI.instance.SetPistonKeyframes();
                }
                else
                {
                    EditorUI.instance.curPiston = null;
                }

                if (activateRailUI)
                {
                    EditorUI.instance.curRailNode = hitSelectable.GetComponent<SplineMeshNodeData>();
                    EditorUI.instance.SetRailInformation();
                }
                else
                {
                    EditorUI.instance.curRailNode = null;
                }

                if (activateSplineUI)
                {
                    EditorUI.instance.curSplineNode = hitSelectable.GetComponent<SplineMakerNodeData>();
                    EditorUI.instance.SetSplineInformation();
                }
                else
                {
                    EditorUI.instance.curSplineNode = null;
                }

                if (activateObjSettingsUI)
                {
                    EditorUI.instance.SetObjSettingsInformation(hitSelectable.gameObject);
                }

                if (activateColorUI)
                {
                    EditorUI.instance.SetColorInformation(hitSelectable.gameObject);
                }

                EditorUI.instance.EnableInspector(true);
                EditorUI.instance.EnableWaterUI(activateWaterUI);
                EditorUI.instance.EnablePistonUI(activatePistonUI);
                EditorUI.instance.EnableRailUI(activateRailUI);
                EditorUI.instance.EnableSplineUI(activateSplineUI);
                EditorUI.instance.EnableObjSettingsUI(activateObjSettingsUI);
                EditorUI.instance.EnableColorUI(activateColorUI);
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

            Vector3 spawnPos_1;
            Vector3 spawnPos_2;

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
                foreach (DefaultObject defaultObject in json.defaultObjects) //itearate through default objects, instantiate based on name
                {
                    GameObject loadedObject;

                    // certain objects need to be recreated from a dummy object, loaded by bundle
                    if (defaultObject.objectName == "prefabs\\level\\world2\\IceSledSpikesGuide")
                    {
                        loadedObject = Instantiate(iceSledSpikesGuide, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));
                    }
                    // certain objects have no meshrenderer, must be added with material for colors
                    else if (defaultObject.objectName == "prefabs\\level\\KillBounds")
                    {
                        loadedObject = Instantiate(Resources.Load(defaultObject.objectName) as GameObject, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));

                        Material mat = new Material(Shader.Find("Standard"));
                        mat.color = Color.red;
                        loadedObject.AddComponent<MeshRenderer>().material = mat;
                    }
                    else if (defaultObject.objectName == "prefabs\\level\\world3\\BoulderDestroyer")
                    {
                        loadedObject = Instantiate(Resources.Load(defaultObject.objectName) as GameObject, defaultObject.GetPosition(), Quaternion.Euler(defaultObject.GetRotation()));

                        Material mat = new Material(Shader.Find("Standard"));
                        mat.color = new Color(0.5f, 0, 0);
                        loadedObject.AddComponent<MeshRenderer>().material = mat;
                    }
                    else if (defaultObject.objectName == "ScaffoldingBlock")
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

                    if (defaultObject.objectName == "prefabs\\level\\Carrot")
                    {
                        instance.carrot = loadedObject;
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
            }
            catch
            {
                Debug.LogError("Missing defaultObjects in json!");
            }

            try
            {
                foreach (WaterObject waterObject in json.waterObjects) //iterate through water objects, apply separate water logic (height, width via component)
                {
                    // if WaterTank, must work around MeshSliceAndStretch
                    if (waterObject.w5)
                    {
                        Catobyte.Utilities.MeshSliceAndStretch loadedMeshSlice;
                        loadedMeshSlice = Instantiate(Resources.Load(waterObject.objectName) as GameObject, waterObject.GetPosition(), Quaternion.Euler(waterObject.GetRotation())).GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();

                        Destroy(loadedMeshSlice.transform.root.Find("Water_W5").gameObject);

                        WaterDataContainer fakeWaterTank = loadedMeshSlice.transform.root.gameObject.AddComponent<WaterDataContainer>();

                        fakeWaterTank.w5 = true;
                        fakeWaterTank.width = waterObject.waterWidth;
                        fakeWaterTank.height = waterObject.waterHeight;
                        fakeWaterTank.keyframes = waterObject.keyframes;

                        MeshSliceData meshData = loadedMeshSlice.transform.root.gameObject.AddComponent<MeshSliceData>();
                        meshData.width = waterObject.waterWidth;
                        meshData.height = waterObject.waterHeight;
                        meshData.depth = 1;

                        loadedMeshSlice.Size = new Vector3(waterObject.waterWidth, waterObject.waterHeight, 1.15f);
                        loadedMeshSlice.Regenerate();

                        loadedMeshSlice.transform.root.gameObject.AddComponent<Outline>();
                        loadedMeshSlice.transform.root.gameObject.AddComponent<EditorSelectable>();

                        continue;
                    }

                    GameObject loadedObject;
                    loadedObject = Instantiate(EditorManager.fakeWater, waterObject.GetPosition(), Quaternion.Euler(waterObject.GetRotation()));

                    loadedObject.transform.localScale = new Vector3(waterObject.waterWidth, waterObject.waterHeight, 1);


                    WaterDataContainer fakeWater = loadedObject.GetComponent<WaterDataContainer>();
                    fakeWater.width = waterObject.waterWidth;
                    fakeWater.height = waterObject.waterHeight;
                    fakeWater.keyframes = waterObject.keyframes;

                    loadedObject.AddComponent<Outline>();
                    loadedObject.AddComponent<EditorSelectable>();
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
                    Catobyte.Utilities.MeshSliceAndStretch loadedObject;

                    loadedObject = Instantiate(Resources.Load(meshSliceObject.objectName) as GameObject, meshSliceObject.GetPosition(), Quaternion.Euler(meshSliceObject.GetRotation())).GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();

                    MeshSliceData meshData = loadedObject.transform.root.gameObject.AddComponent<MeshSliceData>();
                    meshData.width = meshSliceObject.meshWidth;
                    meshData.height = meshSliceObject.meshHeight;
                    meshData.depth = meshSliceObject.meshDepth;

                    loadedObject.Size = new Vector3(meshSliceObject.meshWidth, meshSliceObject.meshHeight, meshSliceObject.meshDepth);
                    loadedObject.Regenerate();

                    loadedObject.transform.root.gameObject.AddComponent<Outline>();
                    loadedObject.transform.root.gameObject.AddComponent<EditorSelectable>();
                }
            }
            catch
            {
                Debug.LogError("Missing meshSliceObjects in json!");
            }
            
            try
            {
                foreach (FlipBlockObject flipBlockObject in json.flipBlockObjects)
                {
                    GameObject loadedObject;

                    loadedObject = Instantiate(Resources.Load(flipBlockObject.objectName) as GameObject, flipBlockObject.GetPosition(), Quaternion.Euler(flipBlockObject.GetRotation()));

                    MeshSliceData meshData = loadedObject.transform.root.gameObject.AddComponent<MeshSliceData>();
                    meshData.width = flipBlockObject.meshWidth;
                    meshData.height = flipBlockObject.meshHeight;
                    meshData.depth = flipBlockObject.meshDepth;

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

                    loadedObject.transform.root.gameObject.AddComponent<Outline>();
                    loadedObject.transform.root.gameObject.AddComponent<EditorSelectable>();
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

                    PistonDataContainer pistonData = loadedObject.AddComponent<PistonDataContainer>();
                    pistonData.keyframes = pistonObject.keyframes;

                    pistonPlatform.regenerateNow = true;
                    pistonPlatform.OnValidate();

                    var meshSlice = loadedObject.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();
                    meshSlice.Size = meshSize;
                    meshSlice.Regenerate();

                    loadedObject.transform.root.gameObject.AddComponent<Outline>();
                    loadedObject.transform.root.gameObject.AddComponent<EditorSelectable>();
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
                    GameObject loadedObject = MinecartRailHelper.CreateRailFromObject(railObject, true);

                    loadedObject.AddComponent<Outline>();
                    loadedObject.AddComponent<EditorSelectable>();
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
                    GameObject loadedObject = SplineMakerHelper.CreateSplineFromObject(splineObject, true);

                    loadedObject.AddComponent<Outline>();
                    loadedObject.AddComponent<EditorSelectable>();
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
                        loadedObject = Instantiate(colorBlockCorner, colorBlockObject.GetPosition(), Quaternion.Euler(colorBlockObject.GetRotation()));
                    }
                    else
                    {
                        loadedObject = Instantiate(colorBlock, colorBlockObject.GetPosition(), Quaternion.Euler(colorBlockObject.GetRotation()));
                    }

                    Color32 color = new Color32((byte)colorBlockObject.r, (byte)colorBlockObject.g, (byte)colorBlockObject.b, 255);

                    ColorData colorData = loadedObject.GetComponent<ColorData>();
                    colorData.color = color;

                    loadedObject.transform.localScale = colorBlockObject.GetScale();
                    loadedObject.GetComponent<MeshRenderer>().material.color = color;

                    loadedObject.AddComponent<Outline>();
                    loadedObject.AddComponent<EditorSelectable>();
                }
            }
            catch
            {
                Debug.LogError("Missing colorBlockObjects in json!");
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
            DeleteRailNode,
            AddSplineNode,
            DeleteSplineNode
        }

        #region Undo
        /// <summary>
        /// Adds an undo event to the top of the undo stack.
        /// </summary
        public static void AddUndo(UndoType undoType, List<EditorSelectable> editorObjects, bool wasRedo = false, 
            SplineMeshNodeData railNode = null, SplineMakerNodeData splineNode = null)
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
                // Debug.Log(railNode.spline.nodes.Count);
                undo.nodeIndex = railNode.spline.nodes.IndexOf(railNode.node);
            }

            if (splineNode)
            {
                undo.splineNode = splineNode;
                undo.nodeIndex = splineNode.splineParent.nodes.IndexOf(splineNode);
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
                case UndoType.AddSplineNode:
                    UndoAddSplineNode(undo);
                    break;
                case UndoType.DeleteSplineNode:
                    UndoDeleteSplineNode(undo);
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
                // EditorManager.instance.selectableObjects.Add(editorObject);
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
            undo.railNode.spline.RemoveNode(undo.railNode.node);
        }

        /// <summary>
        /// Undoes the deletion of a MinecartRailNode to the spline.
        /// The MinecartRailNode must be index 0 in the UndoStruct.editorObjects for this to work properly.
        /// </summary>
        private static void UndoDeleteRailNode(UndoStruct undo)
        {
            AddRedo(UndoType.DeleteRailNode, new List<EditorSelectable>(undo.editorObjects), undo.railNode);

            undo.railNode.gameObject.SetActive(true);
            
            if (undo.railNode.spline.nodes.Count <= undo.nodeIndex)
            {
                undo.railNode.spline.AddNode(undo.railNode.node);
            }
            else
            {
                undo.railNode.spline.InsertNode(undo.nodeIndex, undo.railNode.node);
            }
        }

        private static void UndoAddSplineNode(UndoStruct undo)
        {
            AddRedo(UndoType.AddSplineNode, new List<EditorSelectable>(undo.editorObjects), splineNode: undo.splineNode);

            undo.splineNode.gameObject.SetActive(false);
            undo.splineNode.RemoveNode();
        }

        /// <summary>
        /// Undoes the deletion of a SplineNode to the spline.
        /// The SplineNode must be index 0 in the UndoStruct.editorObjects for this to work properly.
        /// </summary>
        private static void UndoDeleteSplineNode(UndoStruct undo)
        {
            AddRedo(UndoType.DeleteSplineNode, new List<EditorSelectable>(undo.editorObjects), splineNode: undo.splineNode);

            undo.splineNode.gameObject.SetActive(true);

            if (undo.splineNode.splineParent.nodes.Count <= undo.nodeIndex)
            {
                undo.splineNode.splineParent.nodes.Add(undo.splineNode);

                List<Vector3> newAnchorPoints = undo.splineNode.splineParent.spline.anchorPoints.ToList();
                newAnchorPoints.Add(undo.splineNode.transform.localPosition);
                undo.splineNode.splineParent.spline.anchorPoints = newAnchorPoints.ToArray();

                undo.splineNode.splineParent.spline.UpdatePoints();
            }
            else
            {
                undo.splineNode.splineParent.nodes.Insert(undo.nodeIndex, undo.splineNode);

                List<Vector3> newAnchorPoints = undo.splineNode.splineParent.spline.anchorPoints.ToList();
                newAnchorPoints.Insert(undo.nodeIndex, undo.splineNode.transform.localPosition);
                undo.splineNode.splineParent.spline.anchorPoints = newAnchorPoints.ToArray();

                undo.splineNode.splineParent.spline.UpdatePoints();
            }
        }
        #endregion

        #region Redo
        /// <summary>
        /// Adds a redo event to the top of the redo stack.
        /// If the object is to be destroyed, call this method BEFORE the destruction.
        /// </summary
        private static void AddRedo(UndoType undoType, List<EditorSelectable> editorObjects, SplineMeshNodeData railNode = null, SplineMakerNodeData splineNode = null)
        {
            UndoStruct redo = new UndoStruct();
            redo.undoType = undoType;
            redo.undoObjects = new List<UndoObject>();
            redo.editorObjects = editorObjects;

            if (railNode)
            {
                redo.railNode = railNode;
                redo.nodeIndex = railNode.spline.nodes.IndexOf(railNode.node);
            }

            if (splineNode)
            {
                redo.splineNode = splineNode;
                redo.nodeIndex = splineNode.splineParent.nodes.IndexOf(splineNode);
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

            Debug.Log(redo.undoType.ToString());
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
                case UndoType.AddSplineNode:
                    RedoAddSplineNode(redo);
                    break;
                case UndoType.DeleteSplineNode:
                    RedoDeleteSplineNode(redo);
                    break;
            }
        }

        private static void RedoPlace(UndoStruct redo)
        {
            AddUndo(UndoType.Place, new List<EditorSelectable>(redo.editorObjects), true);

            foreach (EditorSelectable editorObject in redo.editorObjects)
            {
                editorObject.gameObject.SetActive(true);
                // EditorManager.instance.selectableObjects.Add(editorObject);
            }
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
            // non-functional, InsertNode seems to use faulty logic or doesn't support this usecase (*try to fix*)
            // spline cannot have fewer than two nodes
            /*if (redo.railNode.spline.nodes.Count >= 2)
            {
                AddUndo(UndoType.AddRailNode, new List<EditorSelectable>(redo.editorObjects), true, redo.railNode);

                redo.railNode.gameObject.SetActive(true);
                redo.railNode.spline.InsertNode(redo.nodeIndex, redo.railNode.node); 
            }*/
        }

        private static void RedoDeleteRailNode(UndoStruct redo)
        {
            AddUndo(UndoType.DeleteRailNode, new List<EditorSelectable>(redo.editorObjects), true, redo.railNode);

            redo.railNode.gameObject.SetActive(false);
            redo.railNode.spline.RemoveNode(redo.railNode.node);
        }

        private static void RedoAddSplineNode(UndoStruct redo)
        {
            if (redo.splineNode.splineParent.nodes.Count >= 2)
            {
                AddUndo(UndoType.AddSplineNode, new List<EditorSelectable>(redo.editorObjects), true, splineNode: redo.splineNode);
                
                redo.splineNode.gameObject.SetActive(true);
                
                redo.splineNode.splineParent.nodes.Insert(redo.nodeIndex, redo.splineNode);

                List<Vector3> newAnchorPoints = redo.splineNode.splineParent.spline.anchorPoints.ToList();
                newAnchorPoints.Insert(redo.nodeIndex, redo.splineNode.transform.localPosition);
                redo.splineNode.splineParent.spline.anchorPoints = newAnchorPoints.ToArray();

                redo.splineNode.splineParent.spline.UpdatePoints();
            }
        }

        private static void RedoDeleteSplineNode(UndoStruct redo)
        {
            AddUndo(UndoType.DeleteSplineNode, new List<EditorSelectable>(redo.editorObjects), true, splineNode: redo.splineNode);

            redo.splineNode.gameObject.SetActive(false);
            redo.splineNode.RemoveNode();
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

            // minecart rail specific, can be ignored otherwise
            public SplineMeshNodeData railNode;
            // spline node specific, can be ignored otherwise
            public SplineMakerNodeData splineNode;
            public int nodeIndex;
        }

        public struct UndoObject
        {
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;
        }
    }
}