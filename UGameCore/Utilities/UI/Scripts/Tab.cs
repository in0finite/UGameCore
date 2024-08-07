using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UGameCore.Utilities.UI {

	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class Tab : MonoBehaviour {

		public	TabView	tabView;
		public	RectTransform	button ;
		public	RectTransform	panel ;
		public RectTransform RootObject => this.panel;

        [Tooltip("Optional text component placed before content")]
        public TextMeshProUGUI TextBeforeContent;
        [Tooltip("Place where all content is inserted")]
        public RectTransform ContentParent;
        public UnityEvent OnTabActivated;

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
		public Button TabButton => this.buttonComponent;

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
			if (null == this.tabView)
				this.tabView = this.transform.parent != null ? this.transform.parent.GetComponent<TabView>() : null;

			if (null == this.button)
				this.button = this.TryGetComponent(out Button b) ? b.GetRectTransform() : null;

			if (this.button != null)
				this.SaveOriginalButtonColorIfNeeded ();
		}

		void Start()
		{
			// add button click listener which will change active tab
			if (this.buttonComponent != null)
				this.buttonComponent.onClick.AddListener(() => this.tabView.SwitchTab(this));

            if (this.button != null)
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
