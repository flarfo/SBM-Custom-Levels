using UnityEngine;
using HarmonyLib;

namespace SBM_CustomLevels
{
    [HarmonyPatch]
    internal class CameraController : MonoBehaviour
    {
        public static Camera camera;

        private Vector3 panOrigin;
        private Vector3 lastPos;

        //position at which camera exists
        private float zCoord = -5.8f;

        private float dragZoomScale = 1f;

        private void Awake()
        {
            camera = gameObject.GetComponent<Camera>();

            transform.position = new Vector3(0, 0, zCoord);
        }

        private void LateUpdate()
        {
            //drag to move when middle click
            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                lastPos = transform.position/8/dragZoomScale; //adjust lastpos based on scaled drag speed

                Vector3 mousePos = Input.mousePosition;

                panOrigin = camera.ScreenToViewportPoint(new Vector3(mousePos.x, mousePos.y, transform.position.z));
            }

            dragZoomScale = Mathf.Clamp(transform.position.z / -5.8f, 0.25f, 12); //change drag speed based on zoom (further = faster)

            if (Input.GetKey(KeyCode.Mouse2))
            {
                Vector3 mousePos = Input.mousePosition;

                Vector3 pos = camera.ScreenToViewportPoint(new Vector3(mousePos.x, mousePos.y, transform.position.z)) - panOrigin;

                Vector3 finalPos = (lastPos - pos) * 8 * dragZoomScale; //scale drag speed

                transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);                                         
            }

            //scroll zoom
            transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp(transform.position.z + Input.mouseScrollDelta.y, -100f, -0.8f));
        }

        //prevent camera from automatically following player, disrupting drag movement
        [HarmonyPatch(typeof(SBM.Shared.Cameras.TrackingCamera), "FixedUpdate")]
        [HarmonyPrefix]
        static bool StopCameraMovement()
        {
            return !EditorManager.InEditor || (EditorManager.instance.Testing && !EditorManager.instance.testingPaused);
        }
    }
}
