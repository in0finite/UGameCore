using UnityEngine;
using UnityEngine.Networking;

namespace UGameCore {
	
	public class ClientDisconnectCleanup : MonoBehaviour {


		void	OnClientDisconnected() {

			// Maybe we don't have to do this, because ClientScene.DestroyAllClientObjects() already done it ?
			if (Player.local != null) {
				// Sometimes (when simulating network latency) player object will not be destroyed, when we
				// disconnect due to packet drop or high latency.
				// That's because we call DontDestroyOnLoad() on player object.
				// So we destroy it here.

				Destroy( Player.local.gameObject );

			}

		}

	}

}
