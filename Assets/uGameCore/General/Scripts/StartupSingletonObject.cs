using UnityEngine;

namespace uGameCore {
	
	public class StartupSingletonObject : MonoBehaviour {

		protected	static	StartupSingletonObject	singleton = null;

		void Awake () {

			if (null == singleton) {
				
				singleton = this;

				DontDestroyOnLoad (this.gameObject);

				this.gameObject.BroadcastMessageNoExceptions ("OnSingletonAwake");

			} else {
				Destroy (this.gameObject);
			}

		}

		void Start() {

			if (this == singleton) {
				this.gameObject.BroadcastMessageNoExceptions ("OnSingletonStart");
			}

		}

	}

}
