using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SBM.UI.Components;

namespace SBM_CustomLevels
{
    internal class EditorUI : MonoBehaviour
    {
        public static EditorUI instance;
        //NOTE: STOP NORMAL GAME UI FROM APPEARING WHILE IN EDITOR
        //MAKE NAME OF OBJECT APPEAR WHEN HOVERED IN UI
        public Text nameField;

        public InputField[] positionField;
        public InputField[] rotationField;
        public InputField[] scaleField;

        private Text lastSavedText;

        private GameObject uiBarBottom;
        private bool bottomBarEnabled = false;

        private Button[] worldButtons = new Button[6];
        private GameObject[] worldUIs = new GameObject[6];

        //initialize the editor ui, finding necessary gameobjects and applying respective functionality
        void Awake()
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

            AddUVScroll(GameObject.Find("UIBar_Top"), -2.2f);
            AddUVScroll(GameObject.Find("UIBar_Right"), -2.2f);
            AddUVScroll(GameObject.Find("UIBar_Bottom"), -2.2f);

            Button moveButton = GameObject.Find("MoveButton").GetComponent<Button>();
            Button selectButton = GameObject.Find("SelectButton").GetComponent<Button>();

            //movebutton functionality
            moveButton.onClick.AddListener(delegate
            {
                EditorManager.instance.moveTool = !EditorManager.instance.moveTool;

                if (EditorManager.instance.moveTool)
                {
                    EditorManager.instance.selectTool = false;
                    moveButton.gameObject.GetComponent<RawImage>().color = Color.green;

                    selectButton.gameObject.GetComponent<RawImage>().color = Color.white;
                }
                else
                {
                    moveButton.gameObject.GetComponent<RawImage>().color = Color.white;
                }
            });

            //selectbutton functionality
            selectButton.onClick.AddListener(delegate
            {
                EditorManager.instance.selectTool = !EditorManager.instance.selectTool;

                if (EditorManager.instance.selectTool)
                {
                    EditorManager.instance.moveTool = false;
                    selectButton.gameObject.GetComponent<RawImage>().color = Color.green;

                    moveButton.gameObject.GetComponent<RawImage>().color = Color.white;
                }
                else
                {
                    selectButton.gameObject.GetComponent<RawImage>().color = Color.white;
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
                    snapEnableButton.gameObject.GetComponent<RawImage>().color = Color.green;
                }
                else
                {
                    snapEnableButton.gameObject.GetComponent<RawImage>().color = Color.white;
                }
            });
            
            //set snap values when SnapUI text is submitted
            InputField snapFieldX = GameObject.Find("SnapFieldX").GetComponent<InputField>();
            snapFieldX.onSubmit.AddListener(delegate (string value)
            {
                EditorManager.instance.SetSnapX(float.Parse(value));
            });
            
            InputField snapFieldY = GameObject.Find("SnapFieldY").GetComponent<InputField>();
            snapFieldY.onSubmit.AddListener(delegate (string value)
            {
                EditorManager.instance.SetSnapY(float.Parse(value));
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

                button.onClick.AddListener(delegate
                {
                    GameObject spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;

                    spawnedObject.AddComponent<Outline>();
                    EditorSelectable selectable = spawnedObject.AddComponent<EditorSelectable>();

                    Vector3 centerPos = EditorManager.instance.editorCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));

                    spawnedObject.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default

                    if (!spawnedObject.GetComponent<Collider>())
                    {
                        //current hotfix, add custom collider versions of certain objects (palm trees, signs, etc. just use thin box colliders or load mesh from bundle?
                        spawnedObject.AddComponent<MeshCollider>();
                        //loadedObject.AddComponent<BoxCollider>();
                    }

                    foreach (EditorSelectable curSelectable in EditorManager.instance.curSelected)
                    {
                        curSelectable.Selected = false;
                    }

                    EditorManager.instance.curSelected.Clear();
                    EditorManager.instance.curSelected.Add(selectable);

                    selectable.Selected = true;
                });
            }

            DisableInitialObjects();
        }

        //objects must be enabled to be found by GameObject.Find(), objects that are not meant to initially be active are disabled
        void DisableInitialObjects()
        {
            foreach (GameObject gameObject in worldUIs)
            {
                gameObject.SetActive(false);
            }

            uiBarBottom.SetActive(false);
        }

        /// When given button pressed, open respective blocks panel and disable other block panels that might be open. Disables/enables bottom UI bar.
        void AddWorldUIEvent(int buttonId)
        {
            worldButtons[buttonId].onClick.AddListener(delegate
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
            });
        }

        // When InputField text is submitted, apply position transform on currently selected objects based on text.
        void AddPositionInputEvent(InputField inputField, int coordinate)
        {
            inputField.onSubmit.AddListener(delegate (string value)
            {
                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    Vector3 pos = selectable.transform.position;
                    pos[coordinate] = float.Parse(value);

                    selectable.transform.position = pos;
                }
            });
        }

        // When InputField text is submitted, apply rotation transform on currently selected objects based on text.
        void AddRotationInputEvent(InputField inputField, int coordinate)
        {
            inputField.onSubmit.AddListener(delegate (string value)
            {
                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    Vector3 rot = selectable.transform.rotation.eulerAngles;
                    rot[coordinate] = float.Parse(value);

                    selectable.transform.rotation = Quaternion.Euler(rot);
                }
            });
        }

        // When InputField text is submitted, apply scale transform on currently selected objects based on text.
        void AddScaleInputEvent(InputField inputField, int coordinate)
        {
            inputField.onSubmit.AddListener(delegate (string value)
            {
                foreach (EditorSelectable selectable in EditorManager.instance.curSelected)
                {
                    Vector3 scale = selectable.transform.localScale;
                    scale[coordinate] = float.Parse(value);

                    selectable.transform.localScale = scale;
                }
            });
        }

        //reset editor when UI is closed (when scene is left)
        void OnDestroy()
        {
            EditorManager.instance.ResetEditor();
        }

        /// Adds a RawImageUVScroll component to the object.
        private void AddUVScroll(GameObject gameObject, float scrollSpeed)
        {
            RawImageUVScroll uvScroll = gameObject.AddComponent<RawImageUVScroll>();
            uvScroll.targetScrollSpeed.x = scrollSpeed;
            uvScroll.scrollEnabled = true;
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
