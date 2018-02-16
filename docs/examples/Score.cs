using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore.Score {
	
	public class Score : NetworkBehaviour {
		
		public	bool	resetOnSceneChange = true;

		[SyncVar(hook="KillScoreChanged")]	private	int numKills = 0;
		[SyncVar(hook="DeathScoreChanged")]	private	int	numDeaths = 0 ;

		public int NumKills { get { return this.numKills; } set { numKills = value; } }
		public int NumDeaths { get { return this.numDeaths; } set { numDeaths = value; } }

		private	Player	m_player = null;


		// Use this for initialization
		void Start () {

			m_player = GetComponent<Player> ();

		}

		void OnSceneChanged( SceneChangedInfo info ) {

			if (!this.isServer)
				return;

			if (this.resetOnSceneChange) {
				// reset player score
				this.numKills = 0;
				this.numDeaths = 0;
			}


		}

		private	void	OnDied( Player playerWhoKilledYou ) {

			if (!this.isServer)
				return;
			
			this.numDeaths++;

			if (playerWhoKilledYou != null) {
				if (m_player != playerWhoKilledYou) {
					playerWhoKilledYou.GetComponent<Score>().numKills++;
				}
			}

		}

		private	void	KillScoreChanged( int newKills ) {

			if (this.isServer)
				return;
			
			this.numKills = newKills;

		}

		private	void	DeathScoreChanged( int newDeaths ) {

			if (this.isServer)
				return;
			
			this.numDeaths = newDeaths;

		}


	}

}
