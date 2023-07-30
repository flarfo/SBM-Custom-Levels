using System;
using System.Collections;
using UnityEngine;

namespace SBM_CustomLevels
{
    public class EditorSelectable : MonoBehaviour
    {
        //when selected, add enable outline and set inspector to display object transform values
        public bool Selected
        {
            get { return selected; }

            set
            {
                selected = value;

                if (outline)
                {
                    outline.enabled = value;
                }
                
                if (value)
                {
                    SetInspectorInfo();
                }

                // carrots are weird, outlines dont reset when deselected. fixes outline staying baked permanently
                if (!value && (gameObject.name.Contains("Carrot") || gameObject.name.Contains("HotdogKart")))
                {
                    outline.FixInstantiated();
                }
            }
        }

        public Outline outline;

        Vector3 mouseOffset;
        float zCoord;
        private bool selected = false;

        //apply outline
        void Awake()
        {
            if (!outline)
            {
                outline = GetComponent<Outline>();
            }

            outline.OutlineColor = new Color32(255, 0, 203, 255);
            outline.OutlineWidth = 2f;

            outline.enabled = false;
        }

        void OnEnable()
        {
            EditorManager.instance.selectableObjects.Add(this);
        }

        void OnDestroy()
        {
            EditorManager.instance.selectableObjects.Remove(this);
        }

        private void OnMouseDown()
        {
            SetMouseOffset();
        }

        private void OnDisable()
        {
            Selected = false;
            EditorManager.instance.selectableObjects.Remove(this);
        }

        public void MoveObject(Vector2 snapVector)
        {
            Vector3 pos = GetMouseWorldPos() + mouseOffset;

            if (EditorManager.instance.snapEnabled)
            {
                Vector2 snappedPos = new Vector3();

                if (snapVector.x == 0)
                {
                    snappedPos.x = transform.position.x;
                }
                else
                {
                    snappedPos.x = Mathf.Round(pos.x / snapVector.x) * snapVector.x;
                }

                if (snapVector.y == 0)
                {
                    snappedPos.y = transform.position.y;
                }
                else
                {
                    snappedPos.y = Mathf.Round(pos.y / snapVector.y) * snapVector.y;
                }

                transform.position = new Vector3(snappedPos.x, snappedPos.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(pos.x, pos.y, transform.position.z);
            }
        }

        public void MoveObjectToMouse(Vector2 snapVector)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(EditorManager.instance.editorCamera.transform.position.z - transform.position.z);
            Vector3 pos = EditorManager.instance.editorCamera.ScreenToWorldPoint(mousePos);

            Vector3 finalPosition;

            if (EditorManager.instance.snapEnabled)
            {
                Vector2 snappedPos = new Vector3();
                
                if (snapVector.x == 0)
                {
                    snappedPos.x = pos.x;
                }
                else
                {
                    snappedPos.x = Mathf.Round(pos.x / snapVector.x) * snapVector.x;
                }

                if (snapVector.y == 0)
                {
                    snappedPos.y = pos.y;
                }
                else
                {
                    snappedPos.y = Mathf.Round(pos.y / snapVector.y) * snapVector.y;
                }

                finalPosition = new Vector3(snappedPos.x, snappedPos.y, transform.position.z);
            }
            else
            {
                finalPosition = new Vector3(pos.x, pos.y, transform.position.z);
            }

            transform.position = finalPosition;
        }

        public void SetMouseOffset()
        {
            zCoord = EditorManager.instance.editorCamera.WorldToScreenPoint(transform.position).z;

            mouseOffset = gameObject.transform.position - GetMouseWorldPos();
        }

        private Vector3 GetMouseWorldPos()
        {
            Vector3 mousePoint = Input.mousePosition;

            mousePoint.z = zCoord;

            return EditorManager.instance.editorCamera.ScreenToWorldPoint(mousePoint);
        }

        /// <summary>
        /// Sets inspector UI information based on this objects transform.
        /// </summary>
        public void SetInspectorInfo(bool updatePosition = true, bool updateRotation = true, bool updateScale = true)
        {
            bool multiple = false;

            if (EditorManager.instance.curSelected.Count > 1)
            {
                multiple = true;
            }

            EditorUI.instance.SetInspectorName(gameObject.name, multiple);

            if (updatePosition)
            {
                EditorUI.instance.SetInspectorPosition(transform.position, multiple);
            }

            if (updateRotation)
            {
                EditorUI.instance.SetInspectorRotation(transform.rotation.eulerAngles, multiple);
            }
            
            if (updateScale)
            {
                EditorUI.instance.SetInspectorScale(transform.localScale, multiple);
            }
        }
    }
}
