using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace uGameCore.Utilities
{
	public class Draggable : MonoBehaviour, IDragHandler
	{
		public	bool	updateAnchors = true ;
		public	bool	updateOffset = false ;


		public void OnDrag (PointerEventData eventData)
		{
			var rt = this.transform as RectTransform;
			var parent = this.transform.parent as RectTransform;

			if (this.updateOffset) {
				rt.offsetMin += eventData.delta;
				rt.offsetMax += eventData.delta;
			}

			if (this.updateAnchors) {
				Vector2 scaledDelta = eventData.delta;
				scaledDelta.x /= parent.rect.width;
				scaledDelta.y /= parent.rect.height;
				rt.anchorMin += scaledDelta;
				rt.anchorMax += scaledDelta;
			}
		}

	}
}

