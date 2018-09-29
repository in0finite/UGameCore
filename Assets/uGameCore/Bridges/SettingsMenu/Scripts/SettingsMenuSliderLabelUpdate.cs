using UnityEngine;

namespace uGameCore.Menu {
	
	public class SettingsMenuSliderLabelUpdate : MonoBehaviour {

		public	UnityEngine.UI.Text	label = null;
		public	string	cvarName { get ; set ; }
		private	UnityEngine.UI.Slider	m_slider = null;


		void Awake() {

			m_slider = GetComponent<UnityEngine.UI.Slider> ();
		}

		void Start() {

			if (null == label)
				return;
			if (null == m_slider)
				return;

			m_slider.onValueChanged.AddListener( value => label.text = cvarName + " : " + value );
		}

	}

}
