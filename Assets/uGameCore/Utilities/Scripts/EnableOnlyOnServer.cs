using UnityEngine;

namespace uGameCore.Utilities {
	
	public class EnableOnlyOnServer : MonoBehaviour {


		void Start() {

			InvokeRepeating ("EnableDisable", 0f, 0.1f);

		}

		void EnableDisable () {
			
			this.gameObject.SetActive (NetworkStatus.IsServerStarted ());

		}

	}

}
