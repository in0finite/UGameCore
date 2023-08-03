using System.Collections;
using UnityEngine;

namespace UGameCore
{

    public class ChangeStartupScene : MonoBehaviour {

		/// <summary>
		/// How many frames to wait before changing scene, to let other components initialize.
		/// </summary>
		public	int	numFramesToWait = 3 ;

		public	string	sceneToChangeTo = "" ;


		void Start () {
			
			StartCoroutine (this.ChangeScene ());

		}

		IEnumerator	ChangeScene() {
			
			// wait specified number of frames
			for (int i = 0; i < this.numFramesToWait; i++) {
				yield return new WaitForEndOfFrame ();
			}

			// change scene

			string sceneName = this.sceneToChangeTo;
			
			if (string.IsNullOrWhiteSpace (sceneName)) {
				Debug.LogError ("Invalid offline scene");
			} else {
				SceneChanger.ChangeScene (sceneName);
			}

		}


	}

}
