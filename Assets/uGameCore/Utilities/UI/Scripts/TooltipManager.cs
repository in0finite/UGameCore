using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Utilities.UI {
	
	public class TooltipManager : MonoBehaviour, IModuleScript
	{

		public	static	TooltipManager	Instance	{ get ; private set ; }

		public	GameObject	tooltipTextPrefab = null;

		private	static	GameObject	m_currentTooltip = null;
		public	static	GameObject	TooltipObject	{ get { return m_currentTooltip; } }

		private	static	int		m_currentTooltipId = 0;



		void Awake ()
		{
			
			if (null == Instance) {
				Instance = this;
			}

		}

		void Start ()
		{
			
			CreateTooltipObject ();

		}

		private	static	void	CreateTooltipObject() {

			if (m_currentTooltip != null)
				return;

			// create tooltip object and disable it

			m_currentTooltip = Instantiate (Instance.tooltipTextPrefab);

			m_currentTooltip.name = "Tooltip";

			// make sure it's not destroyed when scene changes - but it can still be destroyed if it's
			// parent gets destroyed
			DontDestroyOnLoad (m_currentTooltip);

			m_currentTooltip.SetActive (false);

		}


		/// <summary>
		/// Sets the tooltip text. Specified canvas is the one where the UI element is located.
		/// Returns id of tooltip.
		/// </summary>
		public	static	int	SetTooltip( string text, Vector2 offset, Vector2 dimensions, int maxFontSize, TextAnchor textAnchor, 
			Color backgroundColor, Color textColor, Canvas canvas ) {

			// ensure tooltip object is created => it can be destroyed if it's parent gets destroyed
			CreateTooltipObject ();

			m_currentTooltip.SetActive (true);

			// set background color
			Image image = m_currentTooltip.GetComponent<Image>();
			if (image != null) {
				image.color = backgroundColor;
			}

			// set text properties
			Text textComponent = m_currentTooltip.GetComponentInChildren<Text> ();
			if (textComponent != null) {
				textComponent.text = text;
				textComponent.resizeTextMaxSize = maxFontSize;
				textComponent.alignment = textAnchor;
				textComponent.color = textColor;
			}

			// set it's parent to specified canvas
			if (canvas != null) {
				m_currentTooltip.transform.SetParent (canvas.transform, false);
			}

			// position and dimensions
			Vector2 mousePosition = Input.mousePosition ;
			var rectTransform = m_currentTooltip.transform as RectTransform ;
			if(rectTransform != null) {
				
				// position element relative to left bottom corner
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.zero;

				Vector2 finalPos = mousePosition + offset ;
				if (canvas != null) {
					// adjust position based on scaling

					// use scale factor from Canvas, not from CanvasScaler
					finalPos /= canvas.scaleFactor ;
				}
				// add half of dimensions, but non-scaled, because we are not scaling dimensions
				finalPos += dimensions * 0.5f;

				rectTransform.anchoredPosition = finalPos;

				rectTransform.sizeDelta = dimensions;

			}


			m_currentTooltipId++;

			return m_currentTooltipId;
		}

		public	static	void	RemoveTooltip( int tooltipId )
		{
			if (null == m_currentTooltip)
				return;

			if (m_currentTooltipId != tooltipId)
				return;

			m_currentTooltip.SetActive (false);

		}

	}

}
