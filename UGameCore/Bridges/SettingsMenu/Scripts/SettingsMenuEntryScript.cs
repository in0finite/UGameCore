using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Menu {
	
	public class SettingsMenuEntryScript : MonoBehaviour
	{

		private	Color	m_originalImageColor = Color.white;
		public Color originalImageColor { get { return this.m_originalImageColor; } }


		void Awake ()
		{
			// remember original image color
			var image = this.GetComponent<Image> ();
			if (image)
				m_originalImageColor = image.color;
		}

	}

}
