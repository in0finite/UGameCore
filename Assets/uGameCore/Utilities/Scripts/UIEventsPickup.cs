using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace uGameCore.Utilities {

	public class UIEventsPickup : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
	IPointerUpHandler
	{

		public	event	Action<PointerEventData>	onPointerClick = delegate {};
		public	event	Action<PointerEventData>	onPointerEnter = delegate {};
		public	event	Action<PointerEventData>	onPointerExit = delegate {};
		public	event	Action<PointerEventData>	onPointerDown = delegate {};
		public	event	Action<PointerEventData>	onPointerUp = delegate {};


		public void OnPointerClick (PointerEventData eventData)
		{
			onPointerClick (eventData);
		}

		public void OnPointerEnter (PointerEventData eventData)
		{
			onPointerEnter (eventData);
		}

		public void OnPointerExit (PointerEventData eventData)
		{
			onPointerExit (eventData);
		}

		public void OnPointerDown (PointerEventData eventData)
		{
			onPointerDown (eventData);
		}

		public void OnPointerUp (PointerEventData eventData)
		{
			onPointerUp (eventData);
		}


	}

}
