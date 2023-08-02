using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Utilities.UI {
	
	public class Tab : MonoBehaviour {

		public	TabView	tabView;
		public	RectTransform	button ;
		public	RectTransform	panel ;

		private	bool	m_savedOriginalButtonColor = false;
		private	Color	m_originalButtonColor = Color.white;
		public Color originalButtonColor {
			get {
				this.SaveOriginalButtonColorIfNeeded ();
				return m_originalButtonColor;
			}
		}

		/// <summary> Image component attached to button. </summary>
		public Image buttonImageComponent { get { if(this.button) return this.button.GetComponent<Image>(); else return null; } }

		public Text buttonTextComponent { get { if(this.button) return this.button.GetComponentInChildren<Text>(); else return null; } }

		public Button buttonComponent { get { if(this.button) return this.button.GetComponent<Button>(); else return null; } }

		public	string	tabButtonText {
			get {
				if (this.buttonTextComponent)
					return this.buttonTextComponent.text;
				else
					return null;
			}
		}



		void Awake()
		{
			
			if (this.button)
				this.SaveOriginalButtonColorIfNeeded ();
			
		}

		void Start()
		{
			// add button click listener which will change active tab
			this.buttonComponent.onClick.AddListener( () => { this.tabView.SwitchTab(this); } );

			// remember original color of button - maybe it wasn't done in Awake() because button was null
			this.SaveOriginalButtonColorIfNeeded ();

		}

		void SaveOriginalButtonColorIfNeeded() {

			if (!m_savedOriginalButtonColor) {
				m_originalButtonColor = this.buttonImageComponent.color;
				m_savedOriginalButtonColor = true;
			}

		}

	}

}
