using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.MapManagement {
	
	public class DisplayMapImageWhenMapIsSelected : MonoBehaviour {

		public	Dropdown	mapSelectDropdown = null;
		public	RawImage	targetImage = null;
		public	float	alphaWhenEmpty = 0.3f ;


		void OnEnable() {

			if (null == this.mapSelectDropdown)
				return;
			
			this.mapSelectDropdown.onValueChanged.AddListener( this.MapSelectionChanged ) ;

		}

		void OnDisable() {

			if (null == this.mapSelectDropdown)
				return;

			this.mapSelectDropdown.onValueChanged.RemoveListener( this.MapSelectionChanged ) ;

		}

		void Start() {

			if (null == this.mapSelectDropdown)
				return;

			// update image when script is started
			// this needs to be done because the dropdown may have already changed value before
			// we subscribed to it, or haven't changed it at all
			// it has to be done in Start(), to let other scripts initialize (i.e MapCycle)
			this.MapSelectionChanged( this.mapSelectDropdown.value );

		}

		void MapSelectionChanged( int newIndex ) {
			
			if (null == this.targetImage)
				return;
			
			Texture tex = null;

			if (newIndex >= 0 && newIndex < MapCycle.singleton.mapTextures.Count) {
				tex = MapCycle.singleton.mapTextures [newIndex];
			}

			this.targetImage.texture = tex;

			var color = this.targetImage.color;
			if (null == tex) {
				color.a = this.alphaWhenEmpty;
			} else {
				color.a = 1f;	// make it visible
			}

			this.targetImage.color = color;

		}

	}

}
