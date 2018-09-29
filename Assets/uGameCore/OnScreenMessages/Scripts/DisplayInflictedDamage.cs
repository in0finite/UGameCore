using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore.OnScreenMessages {
	
	public class DisplayInflictedDamage : NetworkBehaviour
	{
		public	float	timeToDisplay = 2 ;
		public	float	velocity = 0.15f ;
		public	Color	color = Color.green ;
		public	Color	backgroundColor = new Color( 0f, 0f, 0f, 0f );



		void OnInflictedDamage (InflictedDamageInfo info) {

			if (!this.isServer)
				return;

			this.TargetInflictedDamage ( this.connectionToClient, info.damage, this.timeToDisplay, this.velocity);

		}

		[TargetRpc]
		void	TargetInflictedDamage( NetworkConnection conn, float damage, float timeToDisplay, float velocity ) {

			if (!this.isLocalPlayer)
				return;

			var msg = new OnScreenMessage ();

			msg.text = damage.ToString ();

			Vector2 dir = Random.insideUnitCircle;

			msg.screenPos = new Vector2 (0.5f, 0.5f) + dir * Random.Range (0f, 0.2f);

			msg.velocity = dir * velocity;

			msg.timeLeft = timeToDisplay;

			msg.color = this.color;
			msg.backgroundColor = this.backgroundColor;

			OnScreenMessageManager.AddMessage (msg);
		}

	}

}
