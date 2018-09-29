using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore.RoundManagement {
	
	public class RoundSystemMarkPlayerForSpawning : NetworkBehaviour {

	//	private	bool	m_shouldBeSpawnedInNextRound = false ;
		public	int		maxRoundTimeToSpawnImmediately = 10 ;

		private	Player	m_player = null;


		void Awake() {
			
			m_player = GetComponent<Player> ();

		}

		void OnPlayerChoosedTeam( string team ) {
			
			// check if player can be spawned immediately

			if (m_player.IsSpectator())
				return;

		//	m_shouldBeSpawnedInNextRound = true;

			if (RoundSystem.singleton.IsRoundStartedSinceMapChange) {
				if (Time.time - RoundSystem.singleton.TimeWhenRoundStarted < this.maxRoundTimeToSpawnImmediately) {
					// player can be spawned immediately
				//	m_shouldBeSpawnedInNextRound = false ;
					PlayingObjectSpawner.MarkPlayerForSpawning (m_player);
				}
			}

		}

		void OnRoundStarted() {

//			if (m_shouldBeSpawnedInNextRound) {
//				
//				PlayingObjectSpawner.MarkPlayerForSpawning (GetComponent<Player> ());
//
//				m_shouldBeSpawnedInNextRound = false;
//			}


			if (!m_player.IsLoggedIn ())
				return;

			if (m_player.IsSpectator ())
				return;

		//	if (!m_player.ChoosedTeam)
		//		return;

			PlayingObjectSpawner.MarkPlayerForSpawning (m_player);

		}

		void OnRoundFinished( string winner ) {

			// what if the things change before round start ? - this should all be done in OnRoundStarted()

		}

	}

}
