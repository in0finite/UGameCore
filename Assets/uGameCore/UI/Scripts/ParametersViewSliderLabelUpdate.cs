using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.UI {
	
	public class ParametersViewSliderLabelUpdate : MonoBehaviour {

		public Text label;

		private Slider m_slider;

		public string entryName {
			get;
			set;
		}



		private void Awake ()
		{
			this.m_slider = this.GetComponent<Slider> ();
		}

		private void Start ()
		{
			if (null == this.label) {
				return;
			}

			if (null == this.m_slider) {
				return;
			}

			this.m_slider.onValueChanged.AddListener ( (float value) => {
				this.label.text = this.entryName + " : " + value;
			});

		}

	}

}
