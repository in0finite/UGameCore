using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace uGameCore.RoundManagement {

	public class RoundSystem : MonoBehaviour {

		[SerializeField]	private	bool	m_enableRoundSystem = true;
		public	static	bool	EnableRoundSystem { get { return singleton.m_enableRoundSystem; } set { singleton.m_enableRoundSystem = value; } }

		private	bool	m_isRoundStartedSinceMapChange = false;
		public	bool	IsRoundStartedSinceMapChange { get { return m_isRoundStartedSinceMapChange; } }
		private	float	m_timePassedSinceStartedMap = 0;
		private	bool	m_isRoundFinished = false ;
		private	float	m_timeWhenRoundFinished = 0;
		private	float	m_timeSinceCheckedForRoundEnd = 0;
		private	float	m_timeWhenRoundStarted = 0;
		public	float	TimeWhenRoundStarted { get { return m_timeWhenRoundStarted; } }

	//	public	static	event System.Action	onRoundStarted = delegate {};
	//	public	static	event System.Action<string>	onRoundEnded = delegate {};

		public	static	RoundSystem	singleton { get ; private set ; }


		void Awake () {

			if (null == singleton) {
				singleton = this;
			}

		}

		// Use this for initialization
		void Start () {

		}

		void OnSceneChanged(SceneChangedInfo info) {

			m_isRoundStartedSinceMapChange = false;
			m_isRoundFinished = false;
			m_timePassedSinceStartedMap = 0;

		}

		// Update is called once per frame
		void Update () {

			if (!NetworkStatus.IsServerStarted ()) {
				return;
			}


			if (!SceneChanger.isLoadingScene) {

				m_timePassedSinceStartedMap += Time.deltaTime ;
				m_timeSinceCheckedForRoundEnd += Time.deltaTime;

				if (m_enableRoundSystem) {

					// check if round should be started
					if (!m_isRoundStartedSinceMapChange) {
						// round is not started since the map was changed
						if (m_timePassedSinceStartedMap > 2) {
							this.StartRound ();
							return;
						}
					} else {
						if (m_isRoundFinished && Time.time - m_timeWhenRoundFinished > 3.5f) {
							// start new round
							this.StartRound ();
							return;
						}
					}

					// check if round should be ended
					if (m_isRoundStartedSinceMapChange && !m_isRoundFinished && m_timeSinceCheckedForRoundEnd > 1
					   && Time.time - m_timeWhenRoundStarted > 3) {

						try {
							CheckForRoundEnd ();
						} finally {
							m_timeSinceCheckedForRoundEnd = 0;
						}

					}

				}

			}


			this.RespawnPlayers ();


		}

		void	CheckForRoundEnd() {

			if (TeamManager.IsFreeForAllModeOn ()) {
				int numAlivePlayers = PlayerManager.players.Count(p => p.IsAlive());
				int numNonSpectatingPlayers = PlayerManager.players.Count(p => ! p.IsSpectator());
				if (1 >= numAlivePlayers) {
					// either 1 player is alive, or none
					if (numNonSpectatingPlayers > numAlivePlayers) {
						// there is someone waiting to play
						// round should end
						EndRound ("");
					} else {
						// there is noone waiting to play
						// no need to end round
					}
				}

				return;
			}


			bool[]	teamHasPlayer = new bool[TeamManager.TeamCount];
			for (int i = 0; i < teamHasPlayer.Length; i++)
				teamHasPlayer [i] = false;
			bool[]	teamHasAlivePlayer = new bool[TeamManager.TeamCount];
			for (int i = 0; i < teamHasAlivePlayer.Length; i++)
				teamHasAlivePlayer [i] = false;

			foreach (var player in PlayerManager.players) {
				
				int teamIndex = TeamManager.singleton.teams.IndexOf (player.Team);
				if (teamIndex < 0)
					continue;

				teamHasPlayer [teamIndex] = true;

				if (player.IsAlive ()) {
					teamHasAlivePlayer [teamIndex] = true;
				}

			}

			int numTeamsWithAlivePlayers = 0;
			int numTeamsWhichHavePlayers = 0;
			string winningTeam = "";
			for (int i = 0; i < teamHasPlayer.Length; i++) {
				if (teamHasAlivePlayer [i]) {
					numTeamsWithAlivePlayers++;
					winningTeam = TeamManager.singleton.teams [i];
				}

				if (teamHasPlayer [i])
					numTeamsWhichHavePlayers++;
			}


			bool shouldEnd = false;

			if (numTeamsWhichHavePlayers >= 2 && numTeamsWithAlivePlayers <= 1) {
				// there are 2 or more teams, and only 1 is left standing
				shouldEnd = true ;
			}

			if(1 == numTeamsWhichHavePlayers && 0 == numTeamsWithAlivePlayers) {
				// all players are members of 1 team, and none of the players is spawned
				winningTeam = "" ;
				shouldEnd = true ;
			}

			if(shouldEnd) {
				this.EndRound (winningTeam);
			}


		}

		private	void	RespawnPlayers() {


//			if (!SceneChanger.isLoadingScene && m_isRoundStartedSinceMapChange && !m_isRoundFinished) {
//
//				foreach (var p in PlayerManager.players) {
//
//					if (p.status != PlayerStatus.ShouldBeSpawnedInThisRound)
//						continue;
//
//					// spawn object for this player
//
//					Vector3 pos = Vector3.zero;
//					Quaternion q = Quaternion.identity;
//					bool canBeSpawned = PlayingObjectSpawner.CanPlayerBeSpawnedAtAnySpawnPosition (p, ref pos, ref q);
//					if (canBeSpawned) {
//						
//						if (null == p.controllingObject) {
//
//							p.CreateGameObjectForPlayer (pos, q);
//
//							if (p.controllingObject != null) {
//								Debug.Log ("Spawned game object for " + p.playerName);
//							} else {
//								Debug.LogError ("Failed to create game object for " + p.playerName);
//							}
//
//							p.status = PlayerStatus.Playing;
//
//						} else {
//
//							Debug.LogError ("Trying to spawn player " + p.playerName + ", but his object is not destroyed.");
//						}
//
//					}
//
//				}
//
//			}


		}
	
		private	void	StartRound() {

			if (m_isRoundStartedSinceMapChange && ! m_isRoundFinished) {
				// round is not finished, so new one can't be started
				return ;
			}

			m_isRoundStartedSinceMapChange = true;
			m_isRoundFinished = false;
			m_timeWhenRoundStarted = Time.time;

			foreach (var player in PlayerManager.players) {

				player.DestroyPlayingObject ();

//				if (player.status == PlayerStatus.ShouldBeSpawnedInNextRound)
//					player.status = PlayerStatus.ShouldBeSpawnedInThisRound;
//				else if (player.status == PlayerStatus.Playing)
//					player.status = PlayerStatus.ShouldBeSpawnedInThisRound;

			}

			this.RoundStarted ();

		}

		public	void	EndRound( string winningTeam ) {

			if (!m_isRoundStartedSinceMapChange)
				return;
			if (m_isRoundFinished) {
				// round is already finished
				return ;
			}

			m_isRoundFinished = true ;
			m_timeWhenRoundFinished = Time.time;

			this.RoundFinished (winningTeam);

		}

		private	void	RoundStarted() {

//			this.gameObject.BroadcastMessageNoExceptions ("OnRoundStarted");
//
//			onRoundStarted ();

			Utilities.Utilities.SendMessageToAllMonoBehaviours ("OnRoundStarted");
		}

		private	void	RoundFinished( string winningTeam ) {

//			this.gameObject.BroadcastMessageNoExceptions ("OnRoundFinished", winningTeam);
//
//			onRoundEnded (winningTeam);

			Utilities.Utilities.SendMessageToAllMonoBehaviours ("OnRoundFinished", winningTeam);
		}

	}

}
