using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SplineMesh;
using SBM.UI.Components;

namespace SBM_CustomLevels
{
    internal class EditorUI : MonoBehaviour
    {
        public static EditorUI instance;

        public static GameObject keyframeUI;
        //NOTE: STOP NORMAL GAME UI FROM APPEARING WHILE IN EDITOR
        //MAKE NAME OF OBJECT APPEAR WHEN HOVERED IN UI
        public Text nameField;

        public InputField[] positionField;
        public InputField[] rotationField;
        public InputField[] scaleField;

        public Text lastSavedText;

        private GameObject uiBarBottom;
        private bool bottomBarEnabled = false;

        private Button[] worldButtons = new Button[6];
        private Button selectButton;
        private Button moveButton;
        private GameObject[] worldUIs = new GameObject[6];

        private GameObject inspectorUI;
        private GameObject waterUI;
        private GameObject railUI;

        public FakeWater curWater;
        private RectTransform keyframeContainer;
        private Vector2 defaultKeyframeContainerSize = new Vector2(100, 168);
        private readonly int maxWaterKeyframes = 16;

        public MinecartRailNode curRailNode;
        public InputField[] railPositionField;
        public InputField[] railDirectionField;
        public InputField[] railUpField;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                OnMoveButton();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                OnSelectButton();
            }

            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                OnWorldButton(5);
            }

            // dont change UI object button world if an inputfield is selected (so updating pos/rot/scale does not change UI window)
            if (EventSystem.current.currentSelectedGameObject)
            {
                if (EventSystem.current.currentSelectedGameObject.TryGetComponent<InputField>(out InputField field))
                {
                    if (field.isFocused)
                    {
                        return;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                OnWorldButton(0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                OnWorldButton(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                OnWorldButton(2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                OnWorldButton(3);
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                OnWorldButton(4);
            }
        }

        //initialize the editor ui, finding necessary gameobjects and applying respective functionality
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

            nameField = GameObject.Find("NameText").GetComponent<Text>();
            positionField = GameObject.Find("PositionContainer").GetComponentsInChildren<InputField>();
            rotationField = GameObject.Find("RotationContainer").GetComponentsInChildren<InputField>();
            scaleField = GameObject.Find("ScaleContainer").GetComponentsInChildren<InputField>();
           
            railPositionField = GameObject.Find("RailPositionContainer").GetComponentsInChildren<InputField>();
            railDirectionField = GameObject.Find("RailDirectionContainer").GetComponentsInChildren<InputField>();
            railUpField = GameObject.Find("RailUpContainer").GetComponentsInChildren<InputField>();

            inspectorUI = GameObject.Find("Inspector");
            waterUI = GameObject.Find("WaterContainer");
            railUI = GameObject.Find("RailContainer");
           
            keyframeContainer = GameObject.Find("KeyframeContainer").GetComponent<RectTransform>();

            GameObject inspectorDragBar = inspectorUI.transform.Find("DragBar").gameObject;
            inspectorDragBar.AddComponent<DraggableUI>().target = inspectorUI.transform;
           
            GameObject waterDragBar = waterUI.transform.Find("DragBar").gameObject;
            waterDragBar.AddComponent<DraggableUI>().target = waterUI.transform;

            GameObject railDragBar = railUI.transform.Find("DragBar").gameObject;
            railDragBar.AddComponent<DraggableUI>().target = railUI.transform;

            Button inspectorMinimizeButton = inspectorDragBar.transform.Find("MinimizeButton").GetComponent<Button>();
            Button waterMinimizeButton = waterDragBar.transform.Find("MinimizeButton").GetComponent<Button>();
            Button railMinimizeButton = railDragBar.transform.Find("MinimizeButton").GetComponent<Button>();
            
            inspectorMinimizeButton.onClick.AddListener(delegate
            {
                GameObject background = inspectorDragBar.transform.parent.Find("Background").gameObject;
                background.SetActive(!background.activeSelf);
            });

            waterMinimizeButton.onClick.AddListener(delegate
            {
                GameObject background = waterDragBar.transform.parent.Find("Background").gameObject;
                background.SetActive(!background.activeSelf);
            });

            railMinimizeButton.onClick.AddListener(delegate
            {
                GameObject background = railDragBar.transform.parent.Find("Background").gameObject;
                background.SetActive(!background.activeSelf);
            });

            moveButton = GameObject.Find("MoveButton").GetComponent<Button>();
            selectButton = GameObject.Find("SelectButton").GetComponent<Button>();
            Button deleteButton = GameObject.Find("DeleteButton").GetComponent<Button>();
            Button undoButton = GameObject.Find("UndoButton").GetComponent<Button>();
            Button redoButton = GameObject.Find("RedoButton").GetComponent<Button>();
            Button addKeyframeButton = GameObject.Find("AddKeyframeButton").GetComponent<Button>();
            Button addRailNodeButton = GameObject.Find("AddRailButton").GetComponent<Button>();
            Button removeKeyframeButton = GameObject.Find("RemoveKeyframeButton").GetComponent<Button>();
            Button removeRailNodeButton = GameObject.Find("RemoveRailButton").GetComponent<Button>();

            //movebutton functionality
            moveButton.onClick.AddListener(OnMoveButton);

            //selectbutton functionality
            selectButton.onClick.AddListener(OnSelectButton);

            deleteButton.onClick.AddListener(delegate
            {
                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                List<EditorSelectable> deletedObjects = new List<EditorSelectable>();

                foreach (EditorSelectable editorSelectable in EditorManager.instance.curSelected)
                {
                    // dont delete rail nodes, this should be managed by the minecart rail ui
                    if (editorSelectable.gameObject.name == "Node")
                    {
                        editorSelectable.Selected = false;
                        continue;
                    }

                    editorSelectable.gameObject.SetActive(false);
                    deletedObjects.Add(editorSelectable);
                }

                UndoManager.AddUndo(UndoManager.UndoType.Delete, deletedObjects);
                EditorManager.instance.curSelected.Clear();
            });

            undoButton.onClick.AddListener(delegate
            {
                UndoManager.Undo();
            });

            redoButton.onClick.AddListener(delegate
            {
                UndoManager.Redo();
            });

            addKeyframeButton.onClick.AddListener(delegate
            {
                if (curWater == null)
                {
                    return;
                }

                for (int i = 0; i < keyframeContainer.childCount; i++)
                {
                    if (!keyframeContainer.GetChild(i).gameObject.activeSelf)
                    {
                        keyframeContainer.GetChild(i).gameObject.SetActive(true);
                        curWater.keyframes.Add(new Keyframe(0, 0));

                        break;
                    }
                }

                if (curWater.keyframes.Count > 4 && keyframeContainer.sizeDelta.y != (42*maxWaterKeyframes))
                {
                    keyframeContainer.sizeDelta = new Vector2(keyframeContainer.sizeDelta.x, keyframeContainer.sizeDelta.y + 42);
                }
            });

            addRailNodeButton.onClick.AddListener(delegate
            {
                if (curRailNode)
                {
                    MinecartRailNode node = MinecartRailHelper.AddNodeAfterSelected(curRailNode.railSpline, curRailNode.node);
                    EditorSelectable editorSelectable = node.gameObject.GetComponent<EditorSelectable>();

                    UndoManager.AddUndo(UndoManager.UndoType.AddRailNode, new List<EditorSelectable>() { editorSelectable }, railNode: node);
                }                
            });

            removeKeyframeButton.onClick.AddListener(delegate
            {
                if (curWater)
                {
                    curWater.keyframes.RemoveAt(curWater.keyframes.Count - 1);
                    SetWaterKeyframes();
                }
            });

            removeRailNodeButton.onClick.AddListener(delegate
            {
                if (curRailNode)
                {
                    if (curRailNode.railSpline.nodes.Count > 2)
                    {
                        EditorSelectable editorSelectable = curRailNode.gameObject.GetComponent<EditorSelectable>();

                        UndoManager.AddUndo(UndoManager.UndoType.DeleteRailNode, new List<EditorSelectable>() { editorSelectable }, railNode: curRailNode);

                        curRailNode.railSpline.RemoveNode(curRailNode.node);

                        EditorManager.instance.curSelected.Remove(editorSelectable);

                        curRailNode.gameObject.SetActive(false);
                        curRailNode = null;
                    }
                }
            });

            railPositionField[0].onValueChanged.AddListener(delegate (string value)
            {
                if (!railPositionField[0].isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    Vector3 pos = new Vector3(result, curRailNode.node.Position.y, curRailNode.node.Position.z);
                    curRailNode.node.Position = pos;
                    curRailNode.transform.localPosition = pos;
                }
            });

            railPositionField[1].onValueChanged.AddListener(delegate (string value)
            {
                if (!railPositionField[1].isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    Vector3 pos = new Vector3(curRailNode.node.Position.x, result, curRailNode.node.Position.z);
                    curRailNode.node.Position = pos;
                    curRailNode.transform.localPosition = pos;
                }
            });

            railPositionField[2].onValueChanged.AddListener(delegate (string value)
            {
                if (!railPositionField[2].isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    Vector3 pos = new Vector3(curRailNode.node.Position.x, curRailNode.node.Position.y, result);
                    curRailNode.node.Position = pos;
                    curRailNode.transform.localPosition = pos;
                }
            });

            railUpField[0].onValueChanged.AddListener(delegate (string value)
            {
                if (!railUpField[0].isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    curRailNode.node.Up = new Vector3(result, curRailNode.node.Up.y, curRailNode.node.Up.z);
                }
            });

            railUpField[1].onValueChanged.AddListener(delegate (string value)
            {
                if (!railUpField[1].isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    curRailNode.node.Up = new Vector3(curRailNode.node.Up.x, result, curRailNode.node.Up.z);
                }
            });

            railUpField[2].onValueChanged.AddListener(delegate (string value)
            {
                if (!railUpField[2].isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    curRailNode.node.Up = new Vector3(curRailNode.node.Up.x, curRailNode.node.Up.y, result);
                }
            });

            // instantiate all water keyframes
            for (int i = 0; i < maxWaterKeyframes; i++)
            {
                GameObject newKeyframe = Instantiate(keyframeUI, keyframeContainer);
                newKeyframe.name = i.ToString();

                // [0] = time
                // [1] = value
                InputField[] inputFields = newKeyframe.GetComponentsInChildren<InputField>();
                inputFields[0].onValueChanged.AddListener(delegate (string value)
                {
                    if (!inputFields[0].isFocused)
                    {
                        return;
                    }

                    if (EditorManager.instance.curSelected.Count == 0)
                    {
                        return;
                    }

                    if (curWater)
                    {
                        if (float.TryParse(value, out float result))
                        {
                            int index = int.Parse(inputFields[0].transform.parent.parent.name);

                            curWater.keyframes[index] = new Keyframe(result, curWater.keyframes[index].value);
                        }
                    }
                });

                inputFields[1].onValueChanged.AddListener(delegate (string value)
                {
                    if (!inputFields[1].isFocused)
                    {
                        return;
                    }

                    if (EditorManager.instance.curSelected.Count == 0)
                    {
                        return;
                    }

                    if (curWater)
                    {
                        if (float.TryParse(value, out float result))
                        {
                            int index = int.Parse(inputFields[1].transform.parent.parent.name);

                            curWater.keyframes[index] = new Keyframe(curWater.keyframes[index].time, result);
                        }
                    }
                });

                newKeyframe.SetActive(false);
            }

            //save button saves when clicked
            lastSavedText = GameObject.Find("LastSavedText").GetComponent<Text>();
            Button saveButton = GameObject.Find("SaveButton").GetComponent<Button>();
            saveButton.onClick.AddListener(delegate
            {
                RecordLevel.RecordJSONLevel();

                //set save timestamp
                lastSavedText.text = "Saved: " + DateTime.Now.ToString("HH:mm");
            });
            
            //enable snapping
            Button snapEnableButton = GameObject.Find("SnapEnableButton").GetComponent<Button>();
            snapEnableButton.onClick.AddListener(delegate
            {
                EditorManager.instance.snapEnabled = !EditorManager.instance.snapEnabled;

                if (EditorManager.instance.snapEnabled)
                {
                    snapEnableButton.gameObject.GetComponent<Image>().color = Color.green;
                }
                else
                {
                    snapEnableButton.gameObject.GetComponent<Image>().color = Color.white;
                }
            });
            
            //set snap values when SnapUI text is submitted
            InputField snapFieldX = GameObject.Find("SnapFieldX").GetComponent<InputField>();
            snapFieldX.onValueChanged.AddListener(delegate (string value)
            {
                if (float.TryParse(value, out float result))
                {
                    EditorManager.instance.SetSnapX(result);
                }
            });
            
            InputField snapFieldY = GameObject.Find("SnapFieldY").GetComponent<InputField>();
            snapFieldY.onValueChanged.AddListener(delegate (string value)
            {
                if (float.TryParse(value, out float result))
                {
                    EditorManager.instance.SetSnapY(result);
                }
            });
            
            //adds submit events for inputfield UI in the inspector (when enter press, apply transformation to object based on text input)
            AddPositionInputEvent(positionField[0], 0);
            AddPositionInputEvent(positionField[1], 1);
            AddPositionInputEvent(positionField[2], 2);

            AddRotationInputEvent(rotationField[0], 0);
            AddRotationInputEvent(rotationField[1], 1);
            AddRotationInputEvent(rotationField[2], 2);

            AddScaleInputEvent(scaleField[0], 0);
            AddScaleInputEvent(scaleField[1], 1);
            AddScaleInputEvent(scaleField[2], 2);

            //finds all objects necessary for block selection
            uiBarBottom = GameObject.Find("UIBar_Bottom");

            worldButtons[0] = GameObject.Find("World1Button").GetComponent<Button>();
            worldButtons[1] = GameObject.Find("World2Button").GetComponent<Button>();
            worldButtons[2] = GameObject.Find("World3Button").GetComponent<Button>();
            worldButtons[3] = GameObject.Find("World4Button").GetComponent<Button>();
            worldButtons[4] = GameObject.Find("World5Button").GetComponent<Button>();
            worldButtons[5] = GameObject.Find("OptionsButton").GetComponent<Button>();

            worldUIs[0] = GameObject.Find("World1");
            worldUIs[1] = GameObject.Find("World2");
            worldUIs[2] = GameObject.Find("World3");
            worldUIs[3] = GameObject.Find("World4");
            worldUIs[4] = GameObject.Find("World5");
            worldUIs[5] = GameObject.Find("OptionsMenu");

            AddWorldUIEvent(0);
            AddWorldUIEvent(1);
            AddWorldUIEvent(2);
            AddWorldUIEvent(3);
            AddWorldUIEvent(4);
            AddWorldUIEvent(5);

            //world1-5 button functionality (opens block panel at bottom)
            foreach (Button button in GameObject.Find("UIBar_Bottom").GetComponentsInChildren<Button>())
            {
                //if water...
                if (button.gameObject.name.Contains("BG"))
                {
                    button.onClick.AddListener(delegate
                    {
                        Destroy(EditorManager.instance.background);

                        switch (button.gameObject.name)
                        {
                            case "WorldBG_1":
                                RenderSettings.skybox = LevelLoader_Mod.skyboxWorld1;
                                EditorManager.instance.worldStyle = 1;
                                break;
                            case "World_2_BG":
                                RenderSettings.skybox = LevelLoader_Mod.skyboxWorld2;
                                EditorManager.instance.worldStyle = 2;
                                break;
                            case "World3_BG":
                                RenderSettings.skybox = LevelLoader_Mod.skyboxWorld3;
                                EditorManager.instance.worldStyle = 3;
                                break;
                            case "World4_BG":
                                RenderSettings.skybox = LevelLoader_Mod.skyboxWorld4;
                                EditorManager.instance.worldStyle = 4;
                                break;
                            case "World5_BG":
                                RenderSettings.skybox = LevelLoader_Mod.skyboxWorld1;
                                EditorManager.instance.worldStyle = 5;
                                break;
                            default:
                                RenderSettings.skybox = LevelLoader_Mod.skyboxWorld1;
                                break;
                        }

                        RenderSettings.skybox.shader = Shader.Find("Skybox/Horizon With Sun Skybox");
                        EditorManager.instance.background = Instantiate(Resources.Load<GameObject>(RecordLevel.NameToPath(button.gameObject.name)));
                    });

                    continue;
                }

                button.onClick.AddListener(delegate
                {
                    GameObject spawnedObject;
                    Vector3 centerPos = EditorManager.instance.editorCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));

                    if (button.gameObject.name == "Water")
                    {
                        spawnedObject = Instantiate(EditorManager.fakeWater);

                        FakeWater fakeWater = spawnedObject.GetComponent<FakeWater>();
                        fakeWater.width = 3;
                        fakeWater.height = 2;
                        curWater = fakeWater;
                        ClearWaterKeyframes();
                        EnableWaterUI(true);
                    }
                    else if (button.gameObject.name == "IceSledSpikesGuide")
                    {
                        spawnedObject = Instantiate(EditorManager.iceSledSpikesGuide);
                    }
                    else if (button.gameObject.name == "Wormhole")
                    {
                        if (EditorManager.instance.wormhole)
                        {
                            EditorManager.instance.wormhole.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default
                            EditorManager.instance.wormhole.SetActive(true);

                            return;
                        }

                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;

                        EditorManager.instance.wormhole = spawnedObject;
                    }
                    else if (button.gameObject.name == "MinecartRail")
                    {
                        Vector3 pos = new Vector3(centerPos.x, centerPos.y, 0);

                        spawnedObject = MinecartRailHelper.SpawnNewRail(pos);
                    }
                    else if (button.gameObject.name == "PlayerSpawn")
                    {
                        if (!EditorManager.instance.spawn1.activeSelf)
                        {
                            EditorManager.instance.spawn1.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default
                            EditorManager.instance.spawn1.SetActive(true);

                            return;
                        }
                        else if (!EditorManager.instance.spawn2.activeSelf)
                        {
                            EditorManager.instance.spawn2.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default
                            EditorManager.instance.spawn2.SetActive(true);

                            return;
                        }

                        return;
                    }
                    else if (button.gameObject.name == "MinecartRail_Sleeper")
                    {
                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                        spawnedObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    }
                    else
                    {
                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                    }

                    EnableRailUI(false);
                    curRailNode = null;

                    spawnedObject.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default

                    if (spawnedObject.layer == 10)
                    {
                        spawnedObject.layer = 0;
                    }

                    if (button.gameObject.name == "CarrotDestroyer")
                    {
                        spawnedObject.transform.localScale = new Vector3(1, 1, 1);
                    }
                    else if (button.gameObject.name == "Carrot")
                    {
                        EditorManager.instance.Carrot = spawnedObject;
                    }
                        

                    if (spawnedObject.TryGetComponent(out MeshCollider meshCollider))
                    {
                        meshCollider.enabled = true;
                    }
                    else if (spawnedObject.GetComponent<Collider>())
                    {

                    }
                    else if (!spawnedObject.GetComponent<Collider>() && spawnedObject.GetComponent<MeshFilter>())
                    {
                        spawnedObject.AddComponent<MeshCollider>();
                    }
                    else
                    {
                        MeshCollider[] meshColliders = spawnedObject.GetComponentsInChildren<MeshCollider>();

                        if (meshColliders.Length != 0)
                        {
                            foreach (MeshCollider collider in meshColliders)
                            {
                                collider.enabled = true;
                            }
                        }
                        else
                        {
                            MeshFilter[] meshFilters = spawnedObject.GetComponentsInChildren<MeshFilter>();

                            if (meshFilters.Length != 0)
                            {
                                foreach (MeshFilter filter in meshFilters)
                                {
                                    filter.gameObject.AddComponent<MeshCollider>();
                                }
                            }
                        }
                    }

                    spawnedObject.AddComponent<Outline>();
                    EditorSelectable selectable = spawnedObject.AddComponent<EditorSelectable>();

                    foreach (EditorSelectable curSelectable in EditorManager.instance.curSelected)
                    {
                        curSelectable.Selected = false;
                    }

                    EditorManager.instance.curSelected.Clear();
                    EditorManager.instance.curSelected.Add(selectable);

                    selectable.Selected = true;

                    EditorUI.instance.EnableInspector(true);

                    UndoManager.AddUndo(UndoManager.UndoType.Place, new List<EditorSelectable> { selectable });
                });
            }

            DisableInitialObjects();
        }

        //objects must be enabled to be found by GameObject.Find(), objects that are not meant to initially be active are disabled
        private void DisableInitialObjects()
        {
            foreach (GameObject gameObject in worldUIs)
            {
                gameObject.SetActive(false);
            }

            inspectorUI.SetActive(false);
            waterUI.SetActive(false);
            railUI.SetActive(false);
            uiBarBottom.SetActive(false);
        }

        private void OnSelectButton()
        {
            EditorManager.instance.selectTool = !EditorManager.instance.selectTool;

            if (EditorManager.instance.selectTool)
            {
                EditorManager.instance.moveTool = false;
                selectButton.gameObject.GetComponent<Image>().color = Color.green;

                moveButton.gameObject.GetComponent<Image>().color = Color.white;
            }
            else
            {
                selectButton.gameObject.GetComponent<Image>().color = Color.white;
            }
        }

        private void OnMoveButton()
        {
            EditorManager.instance.moveTool = !EditorManager.instance.moveTool;

            if (EditorManager.instance.moveTool)
            {
                EditorManager.instance.selectTool = false;
                moveButton.gameObject.GetComponent<Image>().color = Color.green;

                selectButton.gameObject.GetComponent<Image>().color = Color.white;
            }
            else
            {
                moveButton.gameObject.GetComponent<Image>().color = Color.white;
            }
        }

        private void OnWorldButton(int buttonId)
        {
            if (bottomBarEnabled)
            {
                //if panel already open, disable when clicked
                if (worldUIs[buttonId].activeSelf)
                {
                    uiBarBottom.SetActive(false);
                    worldUIs[buttonId].SetActive(false);

                    bottomBarEnabled = false;
                }
                else
                {
                    worldUIs[buttonId].SetActive(true);
                }
            }
            else
            {
                uiBarBottom.SetActive(true);
                worldUIs[buttonId].SetActive(true);

                bottomBarEnabled = true;
            }

            foreach (GameObject ui in worldUIs)
            {
                if (ui != worldUIs[buttonId])
                {
                    ui.SetActive(false);
                }
            }
        }

        public void EnableInspector(bool enable)
        {
            inspectorUI.SetActive(enable);
        }

        public void EnableWaterUI(bool enable)
        {
            waterUI.SetActive(enable);
        }

        public void EnableRailUI(bool enable)
        {
            railUI.SetActive(enable);
        }

        public void SetWaterKeyframes()
        {
            keyframeContainer.sizeDelta = defaultKeyframeContainerSize;

            //disable existing keyframes ui
            ClearWaterKeyframes();

            if (curWater == null)
            {
                return;
            }

            int resizeCount = 0;

            for (int i = 0; i < curWater.keyframes.Count; i++)
            {
                if (i > maxWaterKeyframes)
                {
                    return;
                }

                GameObject keyframe = keyframeContainer.GetChild(i).gameObject;

                InputField[] inputFields = keyframe.GetComponentsInChildren<InputField>();
                inputFields[0].text = curWater.keyframes[i].time.ToString();
                inputFields[1].text = curWater.keyframes[i].value.ToString();

                keyframe.SetActive(true);

                //resize container for scrollrect to properly scroll
                if (i > 4)
                {
                    resizeCount++;
                }
            }

            keyframeContainer.sizeDelta = new Vector2(keyframeContainer.sizeDelta.x, keyframeContainer.sizeDelta.y + (42*(resizeCount+1)));
        }

        private void ClearWaterKeyframes()
        {
            for (int i = 0; i < keyframeContainer.childCount; i++)
            {
                GameObject keyframe = keyframeContainer.GetChild(i).gameObject;

                InputField[] inputFields = keyframe.GetComponentsInChildren<InputField>();
                inputFields[0].text = "";
                inputFields[1].text = "";

                keyframe.SetActive(false);
            }

            keyframeContainer.sizeDelta = defaultKeyframeContainerSize;
        }

        public void SetRailInformation()
        {
            if (curRailNode == null)
            {
                return;
            }

            railPositionField[0].text = curRailNode.node.position.x.ToString();
            railPositionField[1].text = curRailNode.node.position.y.ToString();
            railPositionField[2].text = curRailNode.node.position.z.ToString();

            railDirectionField[0].text = curRailNode.node.direction.x.ToString();
            railDirectionField[1].text = curRailNode.node.direction.y.ToString();
            railDirectionField[2].text = curRailNode.node.direction.z.ToString();

            railUpField[0].text = curRailNode.node.up.x.ToString();
            railUpField[1].text = curRailNode.node.up.y.ToString();
            railUpField[2].text = curRailNode.node.up.z.ToString();
        }

        // When given button pressed, open respective blocks panel and disable other block panels that might be open. Disables/enables bottom UI bar.
        private void AddWorldUIEvent(int buttonId)
        {
            worldButtons[buttonId].onClick.AddListener(delegate
            {
                OnWorldButton(buttonId);
            });
        }

        // When InputField text is submitted, apply position transform on currently selected objects based on text.
        private void AddPositionInputEvent(InputField inputField, int coordinate)
        {
            inputField.onValueChanged.AddListener(delegate (string value)
            {
                if (!inputField.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                UndoManager.AddUndo(UndoManager.UndoType.Move, new List<EditorSelectable>(EditorManager.instance.curSelected));

                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    Vector3 pos = selectable.transform.position;

                    if (float.TryParse(value, out float result))
                    {
                        pos[coordinate] = result;

                        selectable.transform.position = pos;
                    }
                }
            });
        }

        // When InputField text is submitted, apply rotation transform on currently selected objects based on text.
        private void AddRotationInputEvent(InputField inputField, int coordinate)
        {
            inputField.onValueChanged.AddListener(delegate (string value)
            {
                if (!inputField.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                UndoManager.AddUndo(UndoManager.UndoType.Rotate, new List<EditorSelectable>(EditorManager.instance.curSelected));

                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    Vector3 rot = selectable.transform.rotation.eulerAngles;

                    if (float.TryParse(value, out float result))
                    {
                        rot[coordinate] = result;

                        selectable.transform.rotation = Quaternion.Euler(rot);
                    }
                }
            });
        }

        // When InputField text is submitted, apply scale transform on currently selected objects based on text.
        private void AddScaleInputEvent(InputField inputField, int coordinate)
        {
            inputField.onValueChanged.AddListener(delegate (string value)
            {
                if (!inputField.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                UndoManager.AddUndo(UndoManager.UndoType.Scale, new List<EditorSelectable>(EditorManager.instance.curSelected));

                Debug.Log("Scale Added!");

                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    Vector3 scale = selectable.transform.localScale;

                    if (float.TryParse(value, out float result))
                    {
                        FakeWater fakeWater = selectable.gameObject.GetComponent<FakeWater>();

                        if (fakeWater)
                        {
                            if (coordinate == 0)
                            {
                                fakeWater.width = result;
                            }
                            else if (coordinate == 1)
                            {
                                fakeWater.height = result;
                            }
                        }

                        scale[coordinate] = result;

                        selectable.transform.localScale = scale;
                    }
                }
            });
        }

        //reset editor when UI is closed (when scene is left)
        private void OnDestroy()
        {
            EditorManager.instance.ResetEditor();
        }

        public void SetInspectorName(string name, bool multiple)
        {
            if (multiple)
            {
                nameField.text = "Multiple";
                return;
            }

            nameField.text = name;
        }

        public void SetInspectorPosition(Vector3 pos, bool multiple)
        {
            if (multiple)
            {
                positionField[0].text = "-";
                positionField[1].text = "-";
                positionField[2].text = "-";
                return;
            }

            positionField[0].text = pos.x.ToString();
            positionField[1].text = pos.y.ToString();
            positionField[2].text = pos.z.ToString();
        }

        public void SetInspectorRotation(Vector3 rot, bool multiple)
        {
            if (multiple)
            {
                rotationField[0].text = "-";
                rotationField[1].text = "-";
                rotationField[2].text = "-";
                return;
            }

            rotationField[0].text = rot.x.ToString();
            rotationField[1].text = rot.y.ToString();
            rotationField[2].text = rot.z.ToString();
        }

        public void SetInspectorScale(Vector3 scale, bool multiple)
        {
            if (multiple)
            {
                scaleField[0].text = "-";
                scaleField[1].text = "-";
                scaleField[2].text = "-";
                return;
            }

            scaleField[0].text = scale.x.ToString();
            scaleField[1].text = scale.y.ToString();
            scaleField[2].text = scale.z.ToString();
        }
    }
}
