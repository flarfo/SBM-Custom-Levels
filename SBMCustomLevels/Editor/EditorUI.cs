using System;
using System.Linq;
using System.IO;
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
        public InputField[] sizeField;

        public Text lastSavedText;

        private GameObject uiBarBottom;
        private bool bottomBarEnabled = false;

        private Button[] worldButtons = new Button[6];
        private Button selectButton;
        private Button moveButton;
        private Button stampButton;
        private GameObject[] worldUIs = new GameObject[6];

        private GameObject inspectorUI;
        private GameObject waterUI;
        private GameObject pistonUI;
        private GameObject railUI;
        private GameObject objSettingsUI;
        private GameObject splineUI;

        public WaterDataContainer curWater;
        public PistonDataContainer curPiston;
        private RectTransform waterKeyframeContainer;
        private RectTransform pistonKeyframeContainer;
        private Vector2 defaultKeyframeContainerSize = new Vector2(100, 168);
        private readonly int maxKeyframes = 16;

        public SplineMeshNodeData curRailNode;
        public InputField[] railPositionField;
        public InputField[] railDirectionField;
        public InputField[] railUpField;

        public GameObject flipBlockSettingsContainer;
        public Toggle flipBlockSpikes;
        public InputField flipBlockDegrees;
        public InputField flipBlockTime;
        public Button[] flipBlockDirection;
        private GameObject[] flipBlockCheckmarks = new GameObject[2];
        private Toggle[] flipBlockSpikeToggles;

        public GameObject pistonSettingsContainer;
        public InputField pistonMaxTravel;
        public InputField pistonShaftLength;

        public SplineMakerNodeData curSplineNode;
        public InputField[] splinePositionField;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                OnStampButton();
            }

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

            flipBlockSettingsContainer = GameObject.Find("FlipBlockSettingsContainer");
            pistonSettingsContainer = GameObject.Find("PistonSettingsContainer");

            nameField = GameObject.Find("NameText").GetComponent<Text>();
            positionField = GameObject.Find("PositionContainer").GetComponentsInChildren<InputField>();
            rotationField = GameObject.Find("RotationContainer").GetComponentsInChildren<InputField>();
            scaleField = GameObject.Find("ScaleContainer").GetComponentsInChildren<InputField>();
            sizeField = GameObject.Find("SizeContainer").GetComponentsInChildren<InputField>();
           
            railPositionField = GameObject.Find("RailPositionContainer").GetComponentsInChildren<InputField>();
            railDirectionField = GameObject.Find("RailDirectionContainer").GetComponentsInChildren<InputField>();
            railUpField = GameObject.Find("RailUpContainer").GetComponentsInChildren<InputField>();

            splinePositionField = GameObject.Find("SplineNodePositionContainer").GetComponentsInChildren<InputField>();

            inspectorUI = GameObject.Find("Inspector");
            waterUI = GameObject.Find("WaterContainer");
            pistonUI = GameObject.Find("PistonContainer");
            railUI = GameObject.Find("RailContainer");
            objSettingsUI = GameObject.Find("ObjectSettingsContainer");
            splineUI = GameObject.Find("SplineContainer");

            waterKeyframeContainer = GameObject.Find("WaterKeyframeContainer").GetComponent<RectTransform>();
            pistonKeyframeContainer = GameObject.Find("PistonKeyframeContainer").GetComponent<RectTransform>();

            GameObject inspectorDragBar = inspectorUI.transform.Find("DragBar").gameObject;
            inspectorDragBar.AddComponent<DraggableUI>().target = inspectorUI.transform;
           
            GameObject waterDragBar = waterUI.transform.Find("DragBar").gameObject;
            waterDragBar.AddComponent<DraggableUI>().target = waterUI.transform;

            GameObject pistonDragBar = pistonUI.transform.Find("DragBar").gameObject;
            pistonDragBar.AddComponent<DraggableUI>().target = pistonUI.transform;

            GameObject railDragBar = railUI.transform.Find("DragBar").gameObject;
            railDragBar.AddComponent<DraggableUI>().target = railUI.transform;

            GameObject objSettingsDragBar = objSettingsUI.transform.Find("DragBar").gameObject;
            objSettingsDragBar.AddComponent<DraggableUI>().target = objSettingsUI.transform;

            GameObject splineDragBar = splineUI.transform.Find("DragBar").gameObject;
            splineDragBar.AddComponent<DraggableUI>().target = splineUI.transform;

            Button inspectorMinimizeButton = inspectorDragBar.transform.Find("MinimizeButton").GetComponent<Button>();
            Button waterMinimizeButton = waterDragBar.transform.Find("MinimizeButton").GetComponent<Button>();
            Button pistonMinimizeButton = pistonDragBar.transform.Find("MinimizeButton").GetComponent<Button>();
            Button railMinimizeButton = railDragBar.transform.Find("MinimizeButton").GetComponent<Button>();
            Button objSettingsMinimizeButton = objSettingsDragBar.transform.Find("MinimizeButton").GetComponent<Button>();
            Button splineMinimizeButton = splineDragBar.transform.Find("MinimizeButton").GetComponent<Button>();

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

            pistonMinimizeButton.onClick.AddListener(delegate
            {
                GameObject background = pistonDragBar.transform.parent.Find("Background").gameObject;
                background.SetActive(!background.activeSelf);
            });

            railMinimizeButton.onClick.AddListener(delegate
            {
                GameObject background = railDragBar.transform.parent.Find("Background").gameObject;
                background.SetActive(!background.activeSelf);
            });

            objSettingsMinimizeButton.onClick.AddListener(delegate
            {
                GameObject background = objSettingsDragBar.transform.parent.Find("Background").gameObject;
                background.SetActive(!background.activeSelf);
            });

            splineMinimizeButton.onClick.AddListener(delegate
            {
                GameObject background = splineDragBar.transform.parent.Find("Background").gameObject;
                background.SetActive(!background.activeSelf);
            });

            moveButton = GameObject.Find("MoveButton").GetComponent<Button>();
            selectButton = GameObject.Find("SelectButton").GetComponent<Button>();
            stampButton = GameObject.Find("StampButton").GetComponent<Button>();
            Button deleteButton = GameObject.Find("DeleteButton").GetComponent<Button>();
            Button undoButton = GameObject.Find("UndoButton").GetComponent<Button>();
            Button redoButton = GameObject.Find("RedoButton").GetComponent<Button>();
            Button addWaterKeyframeButton = GameObject.Find("AddWaterKeyframeButton").GetComponent<Button>();
            Button addPistonKeyframeButton = GameObject.Find("AddPistonKeyframeButton").GetComponent<Button>();
            Button addRailNodeButton = GameObject.Find("AddRailButton").GetComponent<Button>();
            Button addSplineNodeButton = GameObject.Find("AddSplineNodeButton").GetComponent<Button>();
            Button removeWaterKeyframeButton = GameObject.Find("RemoveWaterKeyframeButton").GetComponent<Button>();
            Button removePistonKeyframeButton = GameObject.Find("RemovePistonKeyframeButton").GetComponent<Button>();
            Button removeRailNodeButton = GameObject.Find("RemoveRailButton").GetComponent<Button>();
            Button removeSplineNodeButton = GameObject.Find("RemoveSplineNodeButton").GetComponent<Button>();

            // movebutton functionality
            moveButton.onClick.AddListener(OnMoveButton);

            // selectbutton functionality
            selectButton.onClick.AddListener(OnSelectButton);

            // stampbutton functionality
            stampButton.onClick.AddListener(OnStampButton);

            deleteButton.onClick.AddListener(delegate
            {
                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                List<EditorSelectable> deletedObjects = new List<EditorSelectable>();

                if (EditorManager.instance.ghostItem)
                {
                    if (EditorManager.instance.ghostItem.isActiveAndEnabled)
                    {

                        EditorManager.instance.ghostItem.Selected = false;
                        EditorManager.instance.curSelected.Remove(EditorManager.instance.ghostItem);
                        EditorManager.instance.selectableObjects.Remove(EditorManager.instance.ghostItem);
                        Destroy(EditorManager.instance.ghostItem.gameObject);

                        EditorManager.instance.ghostItem = null;
                    }
                }

                foreach (EditorSelectable editorSelectable in EditorManager.instance.curSelected)
                {
                    // dont delete rail nodes, this should be managed by the minecart rail ui
                    if (editorSelectable.gameObject.name.Contains("Node"))
                    {
                        editorSelectable.Selected = false;
                        continue;
                    }

                    editorSelectable.gameObject.SetActive(false);
                    EditorManager.instance.selectableObjects.Remove(editorSelectable);
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

            addWaterKeyframeButton.onClick.AddListener(delegate
            {
                if (curWater == null)
                {
                    return;
                }

                for (int i = 0; i < waterKeyframeContainer.childCount; i++)
                {
                    if (!waterKeyframeContainer.GetChild(i).gameObject.activeSelf)
                    {
                        waterKeyframeContainer.GetChild(i).gameObject.SetActive(true);
                        curWater.keyframes.Add(new Keyframe(0, 0));

                        break;
                    }
                }

                if (curWater.keyframes.Count > 4 && waterKeyframeContainer.sizeDelta.y != (42*maxKeyframes))
                {
                    waterKeyframeContainer.sizeDelta = new Vector2(waterKeyframeContainer.sizeDelta.x, waterKeyframeContainer.sizeDelta.y + 42);
                }
            });

            addPistonKeyframeButton.onClick.AddListener(delegate
            {
                if (curPiston == null)
                {
                    return;
                }

                for (int i = 0; i < pistonKeyframeContainer.childCount; i++)
                {
                    if (!pistonKeyframeContainer.GetChild(i).gameObject.activeSelf)
                    {
                        pistonKeyframeContainer.GetChild(i).gameObject.SetActive(true);
                        curPiston.keyframes.Add(new Keyframe(0, 0));

                        break;
                    }
                }

                if (curPiston.keyframes.Count > 4 && pistonKeyframeContainer.sizeDelta.y != (42 * maxKeyframes))
                {
                    pistonKeyframeContainer.sizeDelta = new Vector2(pistonKeyframeContainer.sizeDelta.x, pistonKeyframeContainer.sizeDelta.y + 42);
                }
            });

            addRailNodeButton.onClick.AddListener(delegate
            {
                if (curRailNode)
                {
                    SplineMeshNodeData node = curRailNode.AddNodeAfter();
                    EditorSelectable editorSelectable = node.gameObject.GetComponent<EditorSelectable>();

                    UndoManager.AddUndo(UndoManager.UndoType.AddRailNode, new List<EditorSelectable>() { editorSelectable }, railNode: node);
                }                
            });

            addSplineNodeButton.onClick.AddListener(delegate
            {
                if (curSplineNode)
                {
                    SplineMakerNodeData node = curSplineNode.AddNodeAfter();
                    EditorSelectable editorSelectable = node.gameObject.GetComponent<EditorSelectable>();

                    UndoManager.AddUndo(UndoManager.UndoType.AddSplineNode, new List<EditorSelectable>() { editorSelectable }, splineNode: node);
                }
            });

            removeWaterKeyframeButton.onClick.AddListener(delegate
            {
                if (curWater)
                {
                    curWater.keyframes.RemoveAt(curWater.keyframes.Count - 1);
                    SetWaterKeyframes();
                }
            });

            removePistonKeyframeButton.onClick.AddListener(delegate
            {
                if (curPiston)
                {
                    curPiston.keyframes.RemoveAt(curPiston.keyframes.Count - 1);
                    SetPistonKeyframes();
                }
            });

            removeRailNodeButton.onClick.AddListener(delegate
            {
                if (curRailNode)
                {
                    if (curRailNode.spline.nodes.Count > 2)
                    {
                        EditorSelectable editorSelectable = curRailNode.gameObject.GetComponent<EditorSelectable>();

                        UndoManager.AddUndo(UndoManager.UndoType.DeleteRailNode, new List<EditorSelectable>() { editorSelectable }, railNode: curRailNode);

                        curRailNode.spline.RemoveNode(curRailNode.node);

                        EditorManager.instance.curSelected.Remove(editorSelectable);
                        EditorManager.instance.selectableObjects.Remove(editorSelectable);

                        curRailNode.gameObject.SetActive(false);
                        curRailNode = null;
                    }
                }
            });

            removeSplineNodeButton.onClick.AddListener(delegate
            {
                if (curSplineNode)
                {
                    if (curSplineNode.splineParent.spline.anchorPoints.Length > 2)
                    {
                        EditorSelectable editorSelectable = curSplineNode.gameObject.GetComponent<EditorSelectable>();

                        UndoManager.AddUndo(UndoManager.UndoType.DeleteSplineNode, new List<EditorSelectable>() { editorSelectable }, splineNode: curSplineNode);

                        curSplineNode.RemoveNode();

                        EditorManager.instance.curSelected.Remove(editorSelectable);
                        EditorManager.instance.selectableObjects.Remove(editorSelectable);

                        curSplineNode.gameObject.SetActive(false);
                        curSplineNode = null;
                    }
                }
            });

            splinePositionField[0].onValueChanged.AddListener(delegate (string value)
            {
                if (!splinePositionField[0].isFocused)
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    Vector3 pos = new Vector3(result, curSplineNode.transform.localPosition.y, curSplineNode.transform.localPosition.z);
                    curRailNode.transform.localPosition = pos;
                }
            });

            splinePositionField[1].onValueChanged.AddListener(delegate (string value)
            {
                if (!splinePositionField[1].isFocused)
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    Vector3 pos = new Vector3(curSplineNode.transform.localPosition.x, result, curSplineNode.transform.localPosition.z);
                    curRailNode.transform.localPosition = pos;
                }
            });

            splinePositionField[2].onValueChanged.AddListener(delegate (string value)
            {
                if (!splinePositionField[2].isFocused)
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    Vector3 pos = new Vector3(curSplineNode.transform.localPosition.x, curSplineNode.transform.localPosition.y, result);
                    curRailNode.transform.localPosition = pos;
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
            for (int i = 0; i < maxKeyframes; i++)
            {
                GameObject newKeyframe = Instantiate(keyframeUI, waterKeyframeContainer);
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

            // instantiate all piston keyframes
            for (int i = 0; i < maxKeyframes; i++)
            {
                GameObject newKeyframe = Instantiate(keyframeUI, pistonKeyframeContainer);
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

                    if (curPiston)
                    {
                        if (float.TryParse(value, out float result))
                        {
                            int index = int.Parse(inputFields[0].transform.parent.parent.name);

                            curPiston.keyframes[index] = new Keyframe(result, curPiston.keyframes[index].value);
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

                    if (curPiston)
                    {
                        if (float.TryParse(value, out float result))
                        {
                            int index = int.Parse(inputFields[1].transform.parent.parent.name);

                            curPiston.keyframes[index] = new Keyframe(curPiston.keyframes[index].time, result);
                        }
                    }
                });

                newKeyframe.SetActive(false);
            }

            // flip block object UI
            flipBlockSpikes = flipBlockSettingsContainer.transform.Find("Spikes").GetComponentInChildren<Toggle>();
            flipBlockDegrees = flipBlockSettingsContainer.transform.Find("FlipDegrees").GetComponentInChildren<InputField>();
            flipBlockTime = flipBlockSettingsContainer.transform.Find("Flip Time").GetComponentInChildren<InputField>();
            flipBlockDirection = flipBlockSettingsContainer.transform.Find("Direction").GetComponentsInChildren<Button>();
            flipBlockCheckmarks[0] = flipBlockDirection[0].transform.Find("Left Checkmark").gameObject;
            flipBlockCheckmarks[1] = flipBlockDirection[1].transform.Find("Right Checkmark").gameObject;

            flipBlockSpikeToggles = flipBlockSettingsContainer.transform.Find("Spikes").GetComponentsInChildren<Toggle>();
            for (int i = 0; i < flipBlockSpikeToggles.Length; i++)
            {
                int index = i;
                flipBlockSpikeToggles[i].onValueChanged.AddListener(delegate
                {
                    if (EditorManager.instance.curSelected.Count == 0)
                    {
                        return;
                    }

                    foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                    {
                        if (selectable.TryGetComponent(out SBM.Objects.World5.FlipBlock flipBlock))
                        {
                            if (index <= flipBlock.spikes.Length - 1)
                            {
                                flipBlock.spikes[index].SetActive(flipBlockSpikeToggles[index].isOn);
                                flipBlock.spikesEnabled[index] = flipBlockSpikeToggles[index].isOn;
                            }
                        }
                    }
                });
            }
            

            flipBlockDegrees.onValueChanged.AddListener(delegate (string value)
            {
                if (!flipBlockDegrees.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                    {
                        if (selectable.TryGetComponent(out SBM.Objects.World5.FlipBlock flipBlock))
                        {
                            flipBlock.degreesPerFlip = result;
                        }
                    }
                }
            });

            flipBlockTime.onValueChanged.AddListener(delegate (string value)
            {
                if (!flipBlockTime.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                    {
                        if (selectable.TryGetComponent(out SBM.Objects.World5.FlipBlock flipBlock))
                        {
                            flipBlock.timeBetweenFlips = result;
                        }
                    }
                }
            });

            flipBlockDirection[0].onClick.AddListener(delegate
            {
                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    if (selectable.TryGetComponent(out SBM.Objects.World5.FlipBlock flipBlock))
                    {
                        flipBlock.direction = SBM.Objects.World5.FlipBlock.FlipDirection.Left;
                        flipBlockCheckmarks[0].SetActive(true);
                        flipBlockCheckmarks[1].SetActive(false);
                    }
                }
            });

            flipBlockDirection[1].onClick.AddListener(delegate
            {
                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    if (selectable.TryGetComponent(out SBM.Objects.World5.FlipBlock flipBlock))
                    {
                        flipBlock.direction = SBM.Objects.World5.FlipBlock.FlipDirection.Right;
                        flipBlockCheckmarks[0].SetActive(false);
                        flipBlockCheckmarks[1].SetActive(true);
                    }
                }
            });

            flipBlockSettingsContainer.SetActive(false);

            sizeField[0].onValueChanged.AddListener(delegate (string value)
            {
                if (!sizeField[0].isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                //UndoManager.AddUndo(UndoManager.UndoType.Move, new List<EditorSelectable>(EditorManager.instance.curSelected));

                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    if (selectable.gameObject.TryGetComponent(out MeshSliceData meshData))
                    {
                        var meshSlice = selectable.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();

                        if (float.TryParse(value, out float result) && meshSlice)
                        {
                            meshData.width = result;
                            Vector3 scale = new Vector3(meshData.width, meshData.height, meshData.depth);

                            if (selectable.TryGetComponent(out SBM.Objects.World5.PistonPlatform pistonPlatform))
                            {
                                pistonPlatform.platformSize = scale;
                            }
                            else if (selectable.TryGetComponent(out SBM.Objects.World5.FlipBlock flipBlock))
                            {
                                for (int i = 0; i < flipBlock.spikes.Length; i++)
                                {
                                    // oscillate between 0, 1, 0, -1 using Sin to determine the x position of the current spike, where 1 = right, -1 = left
                                    int xDir = (int)Math.Sin((Math.PI * i) / 2); // 0 1 0 -1

                                    GameObject curSpike = flipBlock.spikes[i];
                                    curSpike.transform.localPosition = new Vector3((xDir * result) / 2, curSpike.transform.localPosition.y, curSpike.transform.localPosition.z);
                                }
                            }

                            meshSlice.Size = scale;
                            meshSlice.Regenerate();
                        }
                    }
                }
            });

            sizeField[1].onValueChanged.AddListener(delegate (string value)
            {
                if (!sizeField[1].isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                //UndoManager.AddUndo(UndoManager.UndoType.Move, new List<EditorSelectable>(EditorManager.instance.curSelected));

                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    if (selectable.gameObject.TryGetComponent(out MeshSliceData meshData))
                    {
                        var meshSlice = selectable.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();

                        if (float.TryParse(value, out float result) && meshSlice)
                        {
                            meshData.height = result;
                            Vector3 scale = new Vector3(meshData.width, meshData.height, meshData.depth);

                            if (selectable.TryGetComponent(out SBM.Objects.World5.PistonPlatform pistonPlatform))
                            {
                                pistonPlatform.platformSize = scale;
                            }
                            else if (selectable.TryGetComponent(out SBM.Objects.World5.FlipBlock flipBlock))
                            {
                                for (int i = 0; i < flipBlock.spikes.Length; i++)
                                {
                                    // oscillate between 1, 0, -1, 0 using Cos to determine the y position of the current spike, where 1 = up, -1 = down
                                    int yDir = (int)Math.Cos((Math.PI * i) / 2); // 1 0 -1 0

                                    GameObject curSpike = flipBlock.spikes[i];
                                    curSpike.transform.localPosition = new Vector3(curSpike.transform.localPosition.x, (yDir * result) / 2, curSpike.transform.localPosition.z);
                                }
                            }

                            meshSlice.Size = scale;
                            meshSlice.Regenerate();
                        }
                    }
                }
            });

            sizeField[2].onValueChanged.AddListener(delegate (string value)
            {
                if (!sizeField[2].isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                //UndoManager.AddUndo(UndoManager.UndoType.Move, new List<EditorSelectable>(EditorManager.instance.curSelected));

                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    if (selectable.gameObject.TryGetComponent(out MeshSliceData meshData))
                    {
                        var meshSlice = selectable.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();

                        if (float.TryParse(value, out float result) && meshSlice)
                        {
                            meshData.depth = result;
                            Vector3 scale = new Vector3(meshData.width, meshData.height, meshData.depth);

                            if (selectable.TryGetComponent(out SBM.Objects.World5.PistonPlatform pistonPlatform))
                            {
                                pistonPlatform.platformSize = scale;
                            }

                            meshSlice.Size = scale;
                            meshSlice.Regenerate();
                        }
                    }
                }
            });

            // piston platform object UI
            pistonMaxTravel = pistonSettingsContainer.transform.Find("MaxTravel").GetComponentInChildren<InputField>();
            pistonShaftLength = pistonSettingsContainer.transform.Find("ShaftLength").GetComponentInChildren<InputField>();

            pistonMaxTravel.onValueChanged.AddListener(delegate (string value)
            {
                if (!pistonMaxTravel.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                    {
                        if (selectable.TryGetComponent(out SBM.Objects.World5.PistonPlatform pistonPlatform))
                        {
                            pistonPlatform.pistonMaxTravel = result;

                            pistonPlatform.regenerateNow = true;
                            pistonPlatform.OnValidate();
                        }
                    }
                }
            });

            pistonShaftLength.onValueChanged.AddListener(delegate (string value)
            {
                if (!pistonShaftLength.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

                if (EditorManager.instance.curSelected.Count == 0)
                {
                    return;
                }

                if (float.TryParse(value, out float result))
                {
                    foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                    {
                        if (selectable.TryGetComponent(out SBM.Objects.World5.PistonPlatform pistonPlatform))
                        {
                            pistonPlatform.extraShaftLength = result;

                            pistonPlatform.regenerateNow = true;
                            pistonPlatform.OnValidate();
                        }
                    }
                }
            });

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
                                RenderSettings.skybox = LevelLoader_Mod.skyboxWorld5;
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
                    
                    switch (button.gameObject.name)
                    {
                        case "Water_W4":
                            spawnedObject = Instantiate(EditorManager.fakeWater);
                            WaterDataContainer fakeWater = spawnedObject.GetComponent<WaterDataContainer>();
                            fakeWater.width = 3;
                            fakeWater.height = 2;
                            curWater = fakeWater;
                            ClearWaterKeyframes();
                            DisableOtherUIs("water");
                            break;
                        case "FlipBlock":
                        case "SeeSaw":
                        case "StiffRod":
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            MeshSliceData meshData = spawnedObject.AddComponent<MeshSliceData>();
                            DisableOtherUIs("mesh");
                            SetObjSettingsInformation(spawnedObject);
                            break;
                        case "PistonPlatform":
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            MeshSliceData meshData1 = spawnedObject.AddComponent<MeshSliceData>();
                            DisableOtherUIs("mesh");
                            PistonDataContainer pistonData = spawnedObject.AddComponent<PistonDataContainer>();
                            curPiston = pistonData;
                            ClearPistonKeyframes();
                            DisableOtherUIs("piston");
                            SetObjSettingsInformation(spawnedObject);
                            break;
                        case "WaterTank":
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            MeshSliceData meshData2 = spawnedObject.AddComponent<MeshSliceData>();
                            meshData2.width = 1;
                            meshData2.height = 1;
                            meshData2.depth = 1;
                            var meshSlice = spawnedObject.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();
                            meshSlice.Size = new Vector3(1, 1, 1);
                            meshSlice.Regenerate();
                            Destroy(spawnedObject.transform.Find("Water_W5").gameObject);
                            WaterDataContainer fakeWater1 = spawnedObject.AddComponent<WaterDataContainer>();
                            fakeWater1.width = 1;
                            fakeWater1.height = 1;
                            fakeWater1.w5 = true;
                            curWater = fakeWater1;
                            ClearWaterKeyframes();
                            DisableOtherUIs("water");
                            break;
                        case "IceSledSpikesGuide":
                            spawnedObject = Instantiate(EditorManager.iceSledSpikesGuide);
                            break;
                        case "Wormhole":
                            DisableOtherUIs("inspector");
                            if (EditorManager.instance.wormhole)
                            {
                                EditorManager.instance.wormhole.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default
                                EditorManager.instance.wormhole.SetActive(true);

                                return;
                            }
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            EditorManager.instance.wormhole = spawnedObject;
                            break;
                        case "Carrot":
                            DisableOtherUIs("inspector");
                            if (EditorManager.instance.carrot)
                            {
                                EditorManager.instance.carrot.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default
                                EditorManager.instance.carrot.SetActive(true);

                                return;
                            }
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            EditorManager.instance.carrot = spawnedObject;
                            break;
                        case "MinecartRail":
                            DisableOtherUIs("rail");
                            Vector3 pos = new Vector3(centerPos.x, centerPos.y, 0);
                            spawnedObject = MinecartRailHelper.SpawnNewRail(pos);
                            break;
                        case "SplinePlatform":
                            DisableOtherUIs("inspector");
                            Vector3 pos1 = new Vector3(centerPos.x, centerPos.y, 0);
                            spawnedObject = SplineMakerHelper.SpawnNewSpline(pos1);
                            break;
                        case "ScaffoldingBlock":
                            spawnedObject = Instantiate(EditorManager.scaffoldingBlock);
                            break;
                        case "ScaffoldingCorner":
                            spawnedObject = Instantiate(EditorManager.scaffoldingCorner);
                            break;
                        case "ScaffoldPanelBlack":
                            spawnedObject = Instantiate(EditorManager.scaffoldPanelBlack);
                            break;
                        case "ScaffoldPanelBrown":
                            spawnedObject = Instantiate(EditorManager.scaffoldPanelBrown);
                            break;
                        case "FloppyRod":
                            DisableOtherUIs("inspector");
                            flipBlockSettingsContainer.SetActive(false);
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            break;
                        case "PlayerSpawn":
                            DisableOtherUIs("inspector");
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
                        case "MinecartRail_Sleeper":
                            DisableOtherUIs("inspector");
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            spawnedObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                            break;
                        case "KillBounds":
                            DisableOtherUIs("inspector");
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            Material mat = new Material(Shader.Find("Standard"));
                            mat.color = Color.red;
                            spawnedObject.AddComponent<MeshRenderer>().material = mat;
                            break;
                        case "BoulderDestroyer":
                            DisableOtherUIs("inspector");
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            Material mat1 = new Material(Shader.Find("Standard"));
                            mat1.color = new Color(0.5f, 0, 0);
                            spawnedObject.AddComponent<MeshRenderer>().material = mat1;
                            break;
                        default:
                            DisableOtherUIs("inspector");
                            spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                            break;
                    }

                    /*if (button.gameObject.name == "Water_W4")
                    {
                        spawnedObject = Instantiate(EditorManager.fakeWater);

                        WaterDataContainer fakeWater = spawnedObject.GetComponent<WaterDataContainer>();
                        fakeWater.width = 3;
                        fakeWater.height = 2;
                        curWater = fakeWater;
                        ClearWaterKeyframes();
                        DisableOtherUIs("water");
                    }
                    else if (button.gameObject.name == "FlipBlock" || button.gameObject.name == "SeeSaw" || button.gameObject.name == "StiffRod" || button.gameObject.name == "PistonPlatform" || button.gameObject.name == "WaterTank")
                    {
                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;

                        MeshSliceData meshData = spawnedObject.AddComponent<MeshSliceData>();

                        if (button.gameObject.name != "WaterTank")
                        {
                            DisableOtherUIs("mesh");

                            if (button.gameObject.name == "PistonPlatform")
                            {
                                PistonDataContainer pistonData = spawnedObject.AddComponent<PistonDataContainer>();
                                curPiston = pistonData;
                                ClearPistonKeyframes();

                                DisableOtherUIs("piston");
                            }

                            SetObjSettingsInformation(spawnedObject);
                        }
                        else
                        {
                            meshData.width = 1;
                            meshData.height = 1;
                            meshData.depth = 1;

                            var meshSlice = spawnedObject.GetComponentInChildren<Catobyte.Utilities.MeshSliceAndStretch>();
                            meshSlice.Size = new Vector3(1, 1, 1);
                            meshSlice.Regenerate();

                            Destroy(spawnedObject.transform.Find("Water_W5").gameObject);

                            WaterDataContainer fakeWater = spawnedObject.AddComponent<WaterDataContainer>();
                            fakeWater.width = 1;
                            fakeWater.height = 1;
                            fakeWater.w5 = true;
                            curWater = fakeWater;
                            ClearWaterKeyframes();
                            DisableOtherUIs("water");
                        }

                        // ALSO: add ui for modifying size, replace water ui with general purpose ui
                    }
                    else if (button.gameObject.name == "IceSledSpikesGuide")
                    {
                        spawnedObject = Instantiate(EditorManager.iceSledSpikesGuide);
                    }
                    else if (button.gameObject.name == "Wormhole")
                    {
                        DisableOtherUIs("inspector");

                        if (EditorManager.instance.wormhole)
                        {
                            EditorManager.instance.wormhole.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default
                            EditorManager.instance.wormhole.SetActive(true);

                            return;
                        }

                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                        EditorManager.instance.wormhole = spawnedObject;
                    }
                    else if (button.gameObject.name == "Carrot")
                    {
                        DisableOtherUIs("inspector");

                        if (EditorManager.instance.carrot)
                        {
                            EditorManager.instance.carrot.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default
                            EditorManager.instance.carrot.SetActive(true);

                            return;
                        }

                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                        EditorManager.instance.carrot = spawnedObject;
                    }
                    else if (button.gameObject.name == "MinecartRail")
                    {
                        DisableOtherUIs("rail");

                        Vector3 pos = new Vector3(centerPos.x, centerPos.y, 0);
                        spawnedObject = MinecartRailHelper.SpawnNewRail(pos);
                    }
                    else if (button.gameObject.name == "SplinePlatform")
                    {
                        DisableOtherUIs("inspector");

                        Vector3 pos = new Vector3(centerPos.x, centerPos.y, 0);
                        spawnedObject = SplineMakerHelper.SpawnNewSpline(pos);
                    }
                    else if (button.gameObject.name == "ScaffoldingBlock")
                    {

                    }
                    else if (button.gameObject.name == "ScaffoldingCorner")
                    {

                    }
                    else if (button.gameObject.name == "FloppyRod")
                    {
                        DisableOtherUIs("inspector");
                        //objSettingsUI.SetActive(true);
                        flipBlockSettingsContainer.SetActive(false);
                        //floppyRodContainer.SetActive(true);

                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                        //SetObjSettingsInformation(spawnedObject);
                    }
                    /*else if (button.gameObject.name == "ConveyorBelt")
                    {
                        Vector3 pos = new Vector3(centerPos.x, centerPos.y, 0);

                        spawnedObject = ConveyorBeltHelper.SpawnNewConveyor(pos);
                    }
                    else if (button.gameObject.name == "PlayerSpawn")
                    {
                        DisableOtherUIs("inspector");

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
                        DisableOtherUIs("inspector");

                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                        spawnedObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    }
                    else if (button.gameObject.name == "KillBounds")
                    {
                        DisableOtherUIs("inspector");

                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;

                        Material mat = new Material(Shader.Find("Standard"));
                        mat.color = Color.red;
                        spawnedObject.AddComponent<MeshRenderer>().material = mat;
                    }
                    else if (button.gameObject.name == "BoulderDestroyer")
                    {
                        DisableOtherUIs("inspector");

                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;

                        Material mat = new Material(Shader.Find("Standard"));
                        mat.color = new Color(0.5f, 0, 0);
                        spawnedObject.AddComponent<MeshRenderer>().material = mat;
                    }
                    else
                    {
                        DisableOtherUIs("inspector");

                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                    }
                    */

                    EnableRailUI(false);
                    EnableSplineUI(false);
                    curSplineNode = null;
                    curRailNode = null;

                    spawnedObject.transform.position = new Vector3(centerPos.x, centerPos.y, 0); // z:0 = point at which objects exist by default

                    if (button.gameObject.name == "ScaffoldPipeExtra" || button.gameObject.name == "ScaffoldPanelBlack")
                    {
                        spawnedObject.transform.position += new Vector3(0, 0, -0.45f);
                    }

                    if (spawnedObject.layer == 10)
                    {
                        spawnedObject.layer = 0;
                    }

                    if (button.gameObject.name == "CarrotDestroyer")
                    {
                        spawnedObject.transform.localScale = new Vector3(1, 1, 1);
                    }

                    // attempt to add collision if necessary
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

                    Outline outline = spawnedObject.AddComponent<Outline>();
                    EditorSelectable selectable = spawnedObject.AddComponent<EditorSelectable>();
                    selectable.outline = outline;

                    foreach (EditorSelectable curSelectable in EditorManager.instance.curSelected)
                    {
                        curSelectable.Selected = false;
                    }

                    EditorManager.instance.curSelected.Clear();
                    EditorManager.instance.curSelected.Add(selectable);

                    selectable.Selected = true;

                    // add an object to the ghostItem, so a prefab can be displayed when stamping tool is enabled
                    if (EditorManager.instance.stampTool)
                    {
                        if (EditorManager.instance.ghostItem)
                        {
                            Destroy(EditorManager.instance.ghostItem.gameObject);
                        }
                        
                        EditorManager.instance.ghostItem = selectable;
                    }
                    else
                    {
                        UndoManager.AddUndo(UndoManager.UndoType.Place, new List<EditorSelectable> { selectable });
                    }

                    EditorUI.instance.EnableInspector(true);
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
            pistonUI.SetActive(false);
            railUI.SetActive(false);
            objSettingsUI.SetActive(false);
            splineUI.SetActive(false);
            uiBarBottom.SetActive(false);
        }

        private void OnStampButton()
        {
            EditorManager.instance.stampTool = !EditorManager.instance.stampTool;

            if (EditorManager.instance.ghostItem)
            {
                EditorManager.instance.ghostItem.gameObject.SetActive(EditorManager.instance.stampTool);
            }
            
            if (EditorManager.instance.stampTool)
            {
                // set ghostItem to be currently selected object
                if (EditorManager.instance.ghostItem)
                {
                    foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                    {
                        selectable.Selected = false;
                    }

                    EditorManager.instance.curSelected.Clear();
                    EditorManager.instance.curSelected.Add(EditorManager.instance.ghostItem);
                    EditorManager.instance.ghostItem.Selected = true;
                }

                EditorManager.instance.moveTool = false;
                EditorManager.instance.selectTool = false;

                stampButton.gameObject.GetComponent<Image>().color = Color.green;
                selectButton.gameObject.GetComponent<Image>().color = Color.white;
                moveButton.gameObject.GetComponent<Image>().color = Color.white;
            }
            else
            {
                stampButton.gameObject.GetComponent<Image>().color = Color.white;
            }
        }

        private void OnSelectButton()
        {
            EditorManager.instance.selectTool = !EditorManager.instance.selectTool;

            if (EditorManager.instance.selectTool)
            {
                EditorManager.instance.moveTool = false;
                EditorManager.instance.stampTool = false;

                if (EditorManager.instance.ghostItem)
                {
                    EditorManager.instance.ghostItem.gameObject.SetActive(false);
                }
                
                selectButton.gameObject.GetComponent<Image>().color = Color.green;
                moveButton.gameObject.GetComponent<Image>().color = Color.white;
                stampButton.gameObject.GetComponent<Image>().color = Color.white;
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
                EditorManager.instance.stampTool = false;

                if (EditorManager.instance.ghostItem)
                {
                    EditorManager.instance.ghostItem.gameObject.SetActive(false);
                }

                moveButton.gameObject.GetComponent<Image>().color = Color.green;
                selectButton.gameObject.GetComponent<Image>().color = Color.white;
                stampButton.gameObject.GetComponent<Image>().color = Color.white;
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

        public void EnablePistonUI(bool enable)
        {
            pistonUI.SetActive(enable);
        }

        public void EnableRailUI(bool enable)
        {
            railUI.SetActive(enable);
        }

        public void EnableSplineUI(bool enable)
        {
            splineUI.SetActive(enable);
        }

        public void EnableObjSettingsUI(bool enable)
        {
            objSettingsUI.SetActive(enable);
        }

        public void DisableOtherUIs(string toEnable)
        {
            switch (toEnable)
            {
                case "inspector":
                    EnableInspector(true);
                    EnableWaterUI(false);
                    EnablePistonUI(false);
                    EnableRailUI(false);
                    EnableSplineUI(false);
                    EnableObjSettingsUI(false);
                    break;
                case "water":
                    EnableInspector(true);
                    EnableWaterUI(true);
                    EnablePistonUI(false);
                    EnableRailUI(false);
                    EnableSplineUI(false);
                    EnableObjSettingsUI(false);
                    break;
                case "piston":
                    EnableInspector(true);
                    EnableWaterUI(false);
                    EnablePistonUI(true);
                    EnableRailUI(false);
                    EnableSplineUI(false);
                    EnableObjSettingsUI(true);
                    break;
                case "rail":
                    EnableInspector(true);
                    EnableWaterUI(false);
                    EnablePistonUI(false);
                    EnableRailUI(true);
                    EnableSplineUI(false);
                    EnableObjSettingsUI(false);
                    break;
                case "spline":
                    EnableInspector(true);
                    EnableWaterUI(false);
                    EnablePistonUI(false);
                    EnableRailUI(true);
                    EnableSplineUI(true);
                    EnableObjSettingsUI(false);
                    break;
                case "mesh":
                    EnableInspector(true);
                    EnableWaterUI(false);
                    EnablePistonUI(false);
                    EnableRailUI(false);
                    EnableSplineUI(false);
                    EnableObjSettingsUI(true);
                    break;
                default:
                    EnableInspector(false);
                    EnableWaterUI(false);
                    EnablePistonUI(false);
                    EnableRailUI(false);
                    EnableSplineUI(false);
                    EnableObjSettingsUI(false);
                    break;
            }
        }

        public void SetWaterKeyframes()
        {
            waterKeyframeContainer.sizeDelta = defaultKeyframeContainerSize;

            //disable existing keyframes ui
            ClearWaterKeyframes();

            if (curWater == null)
            {
                return;
            }

            int resizeCount = 0;

            for (int i = 0; i < curWater.keyframes.Count; i++)
            {
                if (i > maxKeyframes)
                {
                    return;
                }

                GameObject keyframe = waterKeyframeContainer.GetChild(i).gameObject;

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

            waterKeyframeContainer.sizeDelta = new Vector2(waterKeyframeContainer.sizeDelta.x, waterKeyframeContainer.sizeDelta.y + (42*(resizeCount+1)));
        }

        private void ClearWaterKeyframes()
        {
            for (int i = 0; i < waterKeyframeContainer.childCount; i++)
            {
                GameObject keyframe = waterKeyframeContainer.GetChild(i).gameObject;

                InputField[] inputFields = keyframe.GetComponentsInChildren<InputField>();
                inputFields[0].text = "";
                inputFields[1].text = "";

                keyframe.SetActive(false);
            }

            waterKeyframeContainer.sizeDelta = defaultKeyframeContainerSize;
        }

        public void SetPistonKeyframes()
        {
            pistonKeyframeContainer.sizeDelta = defaultKeyframeContainerSize;

            //disable existing keyframes ui
            ClearPistonKeyframes();

            if (curPiston == null)
            {
                return;
            }

            int resizeCount = 0;

            for (int i = 0; i < curPiston.keyframes.Count; i++)
            {
                if (i > maxKeyframes)
                {
                    return;
                }

                GameObject keyframe = pistonKeyframeContainer.GetChild(i).gameObject;

                InputField[] inputFields = keyframe.GetComponentsInChildren<InputField>();
                inputFields[0].text = curPiston.keyframes[i].time.ToString();
                inputFields[1].text = curPiston.keyframes[i].value.ToString();

                keyframe.SetActive(true);

                //resize container for scrollrect to properly scroll
                if (i > 4)
                {
                    resizeCount++;
                }
            }

            pistonKeyframeContainer.sizeDelta = new Vector2(pistonKeyframeContainer.sizeDelta.x, pistonKeyframeContainer.sizeDelta.y + (42 * (resizeCount + 1)));
        }

        private void ClearPistonKeyframes()
        {
            for (int i = 0; i < pistonKeyframeContainer.childCount; i++)
            {
                GameObject keyframe = pistonKeyframeContainer.GetChild(i).gameObject;

                InputField[] inputFields = keyframe.GetComponentsInChildren<InputField>();
                inputFields[0].text = "";
                inputFields[1].text = "";

                keyframe.SetActive(false);
            }

            pistonKeyframeContainer.sizeDelta = defaultKeyframeContainerSize;
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

        public void SetSplineInformation()
        {
            if (curSplineNode == null)
            {
                return;
            }

            splinePositionField[0].text = curSplineNode.transform.localPosition.x.ToString();
            splinePositionField[1].text = curSplineNode.transform.localPosition.y.ToString();
            splinePositionField[2].text = curSplineNode.transform.localPosition.z.ToString();
        }

        public void SetObjSettingsInformation(GameObject gameObject)
        {
            MeshSliceData meshData = null;

            if (gameObject.TryGetComponent<MeshSliceData>(out MeshSliceData component))
            {
                meshData = component;
                EditorUI.instance.SetInspectorSize(meshData);
            }

            if (gameObject.TryGetComponent(out SBM.Objects.World5.FlipBlock flipBlock))
            {
                flipBlockDegrees.text = flipBlock.degreesPerFlip.ToString();
                flipBlockTime.text = flipBlock.timeBetweenFlips.ToString();

                if (flipBlock.direction == SBM.Objects.World5.FlipBlock.FlipDirection.Left)
                {
                    flipBlockCheckmarks[0].SetActive(true);
                    flipBlockCheckmarks[1].SetActive(false);
                }
                else
                {
                    flipBlockCheckmarks[0].SetActive(false);
                    flipBlockCheckmarks[1].SetActive(true);
                }

                for (int i = 0; i < flipBlock.spikesEnabled.Length; i++)
                {
                    flipBlockSpikeToggles[i].isOn = flipBlock.spikesEnabled[i];
                }

                flipBlockSettingsContainer.SetActive(true);
                pistonSettingsContainer.SetActive(false);
            }
            else if (gameObject.TryGetComponent(out SBM.Objects.World5.PistonPlatform pistonPlatform))
            {
                pistonMaxTravel.text = pistonPlatform.pistonMaxTravel.ToString();
                pistonShaftLength.text = pistonPlatform.extraShaftLength.ToString();

                pistonSettingsContainer.SetActive(true);
                flipBlockSettingsContainer.SetActive(false);
            }
            else
            {
                pistonSettingsContainer.SetActive(false);
                flipBlockSettingsContainer.SetActive(false);
            }
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

                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    Vector3 scale = selectable.transform.localScale;

                    if (float.TryParse(value, out float result))
                    {
                        scale[coordinate] = result;

                        WaterDataContainer fakeWater = selectable.gameObject.GetComponent<WaterDataContainer>();

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

        public void SetInspectorSize(MeshSliceData meshData)
        {
            sizeField[0].text = meshData.width.ToString();
            sizeField[1].text = meshData.height.ToString();
            sizeField[2].text = meshData.depth.ToString();
        }
    }
}
