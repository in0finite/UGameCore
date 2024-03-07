using UnityEngine;


namespace UGameCore
{

    public class CameraController : NetworkBehaviour {

		public Spectator spectator;


		void Update () {
		
			if (null == Camera.main)
				return;
			
			if (null == Player.local)
				return;

			if (Player.local.GetControllingGameObject () == this.gameObject || spectator.CurrentlySpectatedObject == this.gameObject) {
				this.UpdateCamera ();
			}


		}

		protected	virtual	void	UpdateCamera() {


		}

	}


}

