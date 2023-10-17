using UnityEngine;
using UnityEngine.EventSystems;

namespace SBM_CustomLevels.Editor
{
    public class DraggableUI: MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
	{
		public Transform target;

		private bool isMouseDown = false;

		private Vector3 startMousePosition;
		private Vector3 startPosition;

		private void Update()
		{
			if (isMouseDown)
			{
				Vector3 mousePosition = Input.mousePosition;
				Vector3 vector = mousePosition - startMousePosition;
				Vector3 vector2 = startPosition + vector;

				target.position = vector2;
			}
		}

		public void OnPointerDown(PointerEventData dt)
		{
			isMouseDown = true;
			startPosition = target.position;
			startMousePosition = Input.mousePosition;
		}

		public void OnPointerUp(PointerEventData dt)
		{
			isMouseDown = false;
		}
	}
}
