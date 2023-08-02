using UnityEngine;
using UnityEngine.EventSystems;

namespace uGameCore.Utilities.UI {
	
	public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{

		public	string	tooltipText = "";
		public	Vector2	offset = new Vector2 (70f, 40f);
		public	Vector2	offsetInScreenPercentage = new Vector2( 70f / 1280f, 40f / 720f ) ;
		public	Vector2	size = new Vector2 (100f, 50f);
		public	Vector2	sizeInScreenPercentage = new Vector2( 100f / 1280f, 50f / 720f ) ;
		public	int 	maxFontSize = 14;
		public	TextAnchor	textAnchor = TextAnchor.UpperLeft ;
		public	Color	backgroundColor = new Color( 0.3f, 0.3f, 0.3f, 0.75f ) ;
		public	Color	textColor = Color.white ;
		public	bool	moveTooltipWithMouse = true;

		private	int		m_tooltipId = 0;
		private	bool	m_isPointerInside = false;

		public	event System.Action	onTooltipSet = delegate {};



		public void OnPointerEnter (PointerEventData eventData)
		{
			m_isPointerInside = true;
			this.SetTooltip (eventData.position);
		}

		public void OnPointerExit (PointerEventData eventData)
		{
			m_isPointerInside = false;
			TooltipManager.RemoveTooltip (m_tooltipId);
		}

//		public void OnMove (AxisEventData eventData)
//		{
//			// update tooltip position
//			this.SetTooltip ( new Vector2( Input.mousePosition.x, Input.mousePosition.y ) );
//		}


		void OnDisable() {

			m_isPointerInside = false;
			TooltipManager.RemoveTooltip (m_tooltipId);

		}

		void Update() {

			if (m_isPointerInside) {
				// update tooltip position
				this.SetTooltip( new Vector2( Input.mousePosition.x, Input.mousePosition.y ) );
			}

		}

		public	void	SetTooltip( Vector2 basePos ) {

		//	Vector2 screenSize = new Vector2 (Screen.width, Screen.height);

			Vector2 size = this.size; // Vector2.Scale( this.sizeInScreenPercentage, screenSize );

			m_tooltipId = TooltipManager.SetTooltip( this.tooltipText, this.offset, size, this.maxFontSize, this.textAnchor, 
				this.backgroundColor, this.textColor, this.GetComponentInParent<Canvas>() );

			this.onTooltipSet ();

		}

	}

}
