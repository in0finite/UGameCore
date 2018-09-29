using UnityEngine;

namespace uGameCore {

	/// <summary>
	/// Detects OnDamaged and OnKilled messages, and informs players about inflicted damage, kill and death.
	/// </summary>
	public class InflictedDamageReporter : MonoBehaviour {


		void OnDamaged( InflictedDamageInfo info ) {

			Player attacker = info.player as Player;

			if (attacker != null) {
				
				// notify player that he inflicted damage

				var info2 = new InflictedDamageInfo ();
				info2.damage = info.damage;
				info2.player = PlayerManager.GetPlayerByGameObject (this.gameObject);

				attacker.gameObject.BroadcastMessageNoExceptions ("OnInflictedDamage", info2);
			}

		}

		void OnKilled( InflictedDamageInfo info ) {
			
			Player attacker = info.player as Player;

			var deadPlayer = PlayerManager.GetPlayerByGameObject (this.gameObject);

			// notify dead player
			if (deadPlayer != null) {
				deadPlayer.gameObject.BroadcastMessageNoExceptions ("OnDied", attacker);
			}

			// notify attacker that he made a kill
			if (attacker != null) {
				var info2 = new InflictedDamageInfo (info.damage, deadPlayer);
				attacker.gameObject.BroadcastMessageNoExceptions("OnEarnedKill", info2);
			}

		}

	}

}