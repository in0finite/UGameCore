using UnityEngine;

namespace uGameCore {

	/// <summary>
	/// Detects OnDamaged and OnKilled messages, and informs players about inflicted damage.
	/// </summary>
	public class InflictedDamageReporter : MonoBehaviour {


		void OnDamaged( InflictedDamageInfo info ) {

			Player attacker = info.player as Player;

			if (attacker != null) {
				
				// notify player that he inflicted damage

				//	p.SendMessage ("OnInflictedDamage", info, SendMessageOptions.DontRequireReceiver);
				attacker.OnInflictedDamage (info.damage);

			}

		}

		void OnKilled( InflictedDamageInfo info ) {

			// notify player that his object is dead

			Player attacker = info.player as Player;

			var deadPlayer = PlayerManager.GetPlayerByGameObject (this.gameObject);
			if (deadPlayer != null) {
				deadPlayer.gameObject.BroadcastMessageNoExceptions ("OnDied", attacker);
			} else {
				// manually increase num kills
				if (attacker != null) {
					attacker.GetComponent<Score.Score>().NumKills++;
				}
			}

		}

	}

}