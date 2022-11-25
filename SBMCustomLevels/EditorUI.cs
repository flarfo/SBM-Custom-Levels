using System;
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

        public Text lastSavedText;

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
            Button deleteButton = GameObject.Find("DeleteButton").GetComponent<Button>();

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

            deleteButton.onClick.AddListener(delegate
            {
                foreach (EditorSelectable editorSelectable in EditorManager.instance.curSelected)
                {
                    Destroy(editorSelectable.gameObject);
                }

                EditorManager.instance.curSelected.Clear();
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

                        EditorManager.instance.background = Instantiate(Resources.Load<GameObject>(RecordLevel.NameToPath(button.gameObject.name)));
                        RenderSettings.skybox.shader = Shader.Find("Skybox/Horizon With Sun Skybox");

                        //light
                    });

                    continue;
                }

                button.onClick.AddListener(delegate
                {
                    GameObject spawnedObject;
                    Vector3 centerPos = EditorManager.instance.editorCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));

                    if (button.gameObject.name == "Water")
                    {
                        spawnedObject = Instantiate(LevelLoader_Mod.fakeWater);

                        FakeWater fakeWater = spawnedObject.GetComponent<FakeWater>();
                        fakeWater.width = 3;
                        fakeWater.height = 2;
                    }
                    else if (button.gameObject.name == "Wormhole")
                    {
                        if (EditorManager.instance.wormhole)
                        {
                            EditorManager.instance.wormhole.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default

                            return;
                        }

                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;

                        EditorManager.instance.wormhole = spawnedObject;
                    }
                    else
                    {
                        spawnedObject = Instantiate(Resources.Load(RecordLevel.NameToPath(button.gameObject.name))) as GameObject;
                    }

                    spawnedObject.AddComponent<Outline>();
                    EditorSelectable selectable = spawnedObject.AddComponent<EditorSelectable>();

                    spawnedObject.transform.position = new Vector3(centerPos.x, centerPos.y, 0); //0 = point at which objects exist by default

                    if (spawnedObject.layer == 10)
                    {
                        spawnedObject.layer = 0;
                    }

                    if (button.gameObject.name == "CarrotDestroyer")
                    {
                        spawnedObject.transform.localScale = new Vector3(1, 1, 1);
                    }
                    else if (button.gameObject.name == "IceSledSpikesGuide")
                    {
                        spawnedObject.AddComponent<MeshRenderer>().material = LevelLoader_Mod.skyboxWorld1; //CHANGE
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
            inputField.onValueChanged.AddListener(delegate (string value)
            {
                if (!inputField.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

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
        void AddRotationInputEvent(InputField inputField, int coordinate)
        {
            inputField.onValueChanged.AddListener(delegate (string value)
            {
                if (!inputField.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

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
        void AddScaleInputEvent(InputField inputField, int coordinate)
        {
            inputField.onValueChanged.AddListener(delegate (string value)
            {
                if (!inputField.isFocused) //fix for rapid clicking objects sets their position to inspector values of other object
                {
                    return;
                }

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
