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
                outline.enabled = value;

                SetInspectorInfo();

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
            outline = GetComponent<Outline>();

            outline.OutlineColor = new Color32(255, 0, 203, 255);
            outline.OutlineWidth = 2f;

            outline.enabled = false;
        }

        private void OnMouseDown()
        {
            SetMouseOffset();
        }

        private void OnDisable()
        {
            Selected = false;
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

        public void SetMouseOffset()
        {
            zCoord = EditorManager.instance.editorCamera.WorldToScreenPoint(transform.position).z;

            mouseOffset = gameObject.transform.position - GetMouseWorldPos();
        }

        Vector3 GetMouseWorldPos()
        {
            Vector3 mousePoint = Input.mousePosition;

            mousePoint.z = zCoord;

            return EditorManager.instance.editorCamera.ScreenToWorldPoint(mousePoint);
        }

        /// <summary>
        /// Sets inspector UI information based on this objects transform.
        /// </summary>
        public void SetInspectorInfo()
        {
            bool multiple = false;

            if (EditorManager.instance.curSelected.Count > 1)
            {
                multiple = true;
            }

            EditorUI.instance.SetInspectorName(gameObject.name, multiple);
            EditorUI.instance.SetInspectorPosition(transform.position, multiple);
            EditorUI.instance.SetInspectorRotation(transform.rotation.eulerAngles, multiple);
            EditorUI.instance.SetInspectorScale(transform.localScale, multiple);
        }
    }
}
