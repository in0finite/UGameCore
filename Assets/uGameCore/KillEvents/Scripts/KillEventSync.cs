using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore.KillEvents {


	public class KillEvent {
		public KillEvent (string killer, string dier)
		{
			this.killer = killer;
			this.dier = dier;
		}
		
		public	string killer = "";
		public	string dier = "";
	}


	public class KillEventSync : NetworkBehaviour {

		public	static	event System.Action<KillEvent> onKillEvent = delegate {};


		void Start () {
			
		}


		void OnDied( Player playerWhoKilledYou ) {

			if (!this.isServer)
				return;

			Player playerWhoDied = GetComponent<Player> ();

			string killer = "";
			if (playerWhoKilledYou != null)
				killer = playerWhoKilledYou.playerName;
			string dier = playerWhoDied.playerName;

			// send rpc to all players
			foreach (var p in PlayerManager.GetLoggedInNonBotPlayers()) {
				var sync = p.GetComponent<KillEventSync> ();
				if (sync != null) {
					sync.TargetKillEvent (p.connectionToClient, killer, dier);
				}
			}

			if (!NetworkStatus.IsHost ()) {
				// running as dedicated server
				// invoke event here, because there is no local player to invoke it
				onKillEvent( new KillEvent(killer, dier) );
			}

		}

		[TargetRpc]
		void	TargetKillEvent( NetworkConnection conn, string killer, string dier) {

			if (!this.isLocalPlayer) {
				return;
			}

			onKillEvent (new KillEvent (killer, dier));

		}

	}

}
