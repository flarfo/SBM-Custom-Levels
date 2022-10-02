using UnityEngine;
using HarmonyLib;

namespace SBM_CustomLevels
{
    [HarmonyPatch]
    internal class CameraController : MonoBehaviour
    {
        private Camera camera;

        private Vector3 panOrigin;
        private Vector3 lastPos;

        //position at which camera exists
        private float zCoord = -5.8f;

        public bool freeCam = false;

        private void Awake()
        {
            camera = gameObject.GetComponent<Camera>();

            transform.position = new Vector3(0, 0, zCoord);
        }

        //drag to move when middle click
        private void LateUpdate()
        {   
            if (freeCam)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                lastPos = transform.position/8;

                Vector3 mousePos = Input.mousePosition;

                panOrigin = camera.ScreenToViewportPoint(new Vector3(mousePos.x, mousePos.y, zCoord));
            }

            if (Input.GetKey(KeyCode.Mouse2))
            {
                Vector3 mousePos = Input.mousePosition;

                Vector3 pos = camera.ScreenToViewportPoint(new Vector3(mousePos.x, mousePos.y, zCoord)) - panOrigin;

                Vector3 finalPos = (lastPos - pos) * 8 ;

                transform.position = new Vector3(finalPos.x, finalPos.y, zCoord);                                         
            }

            if (Input.GetKeyDown(KeyCode.Pause))
            {
                freeCam = !freeCam;
            }
        }

        //freecam
        private void Update()
        {

        }

        //prevent camera from automatically following player, disrupting drag movement
        [HarmonyPatch(typeof(SBM.Shared.Cameras.TrackingCamera), "FixedUpdate")]
        [HarmonyPrefix]
        static bool StopCameraMovement()
        {
            return !EditorManager.instance.InEditor;
        }
    }
}
