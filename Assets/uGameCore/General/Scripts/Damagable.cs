using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore {

	public class InflictedDamageInfo {
		public float damage = 0;
		public object player = null ;
		public InflictedDamageInfo ()
		{
			
		}
		public InflictedDamageInfo (float damage, object player)
		{
			this.damage = damage;
			this.player = player;
		}
	}

	public class Damagable : NetworkBehaviour {

		[SyncVar]	[SerializeField]	protected	float	health = 100 ;
		public	float	Health { get { return this.health; } }


		/// <summary>
		/// Inflicts damage to this object, broadcasts message, and destroys game object if health is <= 0.
		/// </summary>
		public	virtual	void	Damage( float amount, object player ) {

			if (!this.isServer)
				return;

			if (this.health <= 0)	// already destroyed ?
				return;
			
			// check if attacker can damage this object
			Player myPlayer = PlayerManager.GetPlayerByGameObject( this.gameObject );
			if (!TeamManager.CanPlayerDamagePlayer (player as Player, myPlayer))
				return;

			this.health -= amount;

			var info = new InflictedDamageInfo ( amount, player );
			this.gameObject.BroadcastMessageNoExceptions ("OnDamaged", info);

			if (this.health <= 0) {
				this.gameObject.BroadcastMessageNoExceptions ("OnKilled", info);

				Destroy (this.gameObject);
			}

		}

	}

}
