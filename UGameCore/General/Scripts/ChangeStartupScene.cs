using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace UGameCore {
	
	public class ChangeStartupScene : MonoBehaviour {

		/// <summary>
		/// How many frames to wait before changing scene, to let other components initialize.
		/// </summary>
		public	int	numFramesToWait = 3 ;

		public	bool	useSceneFromNetworkManager = true ;
		public	string	sceneToChangeTo = "" ;

		public	static	event	System.Action	onPreSceneChange = delegate {} ;


		void Start () {
			
			StartCoroutine (this.ChangeScene ());

		}

		IEnumerator	ChangeScene() {
			
			// wait specified number of frames
			for (int i = 0; i < this.numFramesToWait; i++) {
				yield return new WaitForEndOfFrame ();
			}

		//	try {
				onPreSceneChange ();
		//	} catch (System.Exception ex) {
		//		Debug.LogException (ex);
		//	}

			// change scene

			string sceneName = "";
			if (this.useSceneFromNetworkManager) {
				throw new System.NotSupportedException("Can't use scene from network manager");
			} else {
				sceneName = this.sceneToChangeTo;
			}

			if (string.IsNullOrEmpty (sceneName)) {
				Debug.LogError ("Invalid offline scene");
			} else {
				SceneChanger.ChangeScene (sceneName);
			}

		}


	}

}
