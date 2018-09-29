using UnityEngine;
using UnityEngine.Networking;


namespace uGameCore {
	
	public class CameraController : NetworkBehaviour {


		// Use this for initialization
		protected virtual void Start () {
		

		}
		
		// Update is called once per frame
		void Update () {
		
			if (null == Camera.main)
				return;
			
			if (null == Player.local)
				return;

			if (Player.local.GetControllingGameObject () == this.gameObject || Spectator.GetSpectatingGameObject () == this.gameObject) {
				this.UpdateCamera ();
			}


		}

		protected	virtual	void	UpdateCamera() {


		}

	}


}

