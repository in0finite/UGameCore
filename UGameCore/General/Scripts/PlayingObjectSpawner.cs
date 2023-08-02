using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore {
	
	public class PlayingObjectSpawner : MonoBehaviour {

		public	GameObject	playingObjectPrefab = null ;

		public static GameObject PlayingObjectPrefab {
			get {
				return singleton.playingObjectPrefab;
			}
			set {
				singleton.playingObjectPrefab = value;
			}
		}

		private	static	List<Player>	m_playersMarkedForSpawning = new List<Player> ();

		public	static	PlayingObjectSpawner	singleton { get ; private set ; }


		void Awake() {

			if (null == singleton) {
				singleton = this;
			}

		}

		// Use this for initialization
		void Start () {
			
		}

		void OnSceneChanged( SceneChangedInfo info ) {

			m_playersMarkedForSpawning.Clear ();

		}
		
		// Update is called once per frame
		void Update () {

			if (!NetworkStatus.IsServerStarted ()) {
				return;
			}

			// spawn all players which are marked for spawning
			m_playersMarkedForSpawning.RemoveAll (delegate(Player p) {
			
				if(null == p)
					return true ;
				if(!p.IsLoggedIn())
					return true ;

				if(null == p.GetControllingGameObject()) {

					Vector3 pos = new Vector3();
					Quaternion rot = Quaternion.identity ;
					if( CanPlayerBeSpawnedAtAnySpawnPosition(p, ref pos, ref rot) ) {

						if( p.CreateGameObjectForPlayer( pos, rot ) != null ) {
							Debug.Log ("Spawned game object for " + p.playerName);
						} else {
							Debug.LogError ("Failed to create game object for " + p.playerName);
						}

						return true ;

					} else {
						// failed to find spawn location for player's game object
						// try next time
					}

				}

				return false ;
			});

		}

		public	static	void	MarkPlayerForSpawning( Player p ) {

			if (!NetworkStatus.IsServerStarted ()) {
				return;
			}

			if (!m_playersMarkedForSpawning.Contains (p)) {
				m_playersMarkedForSpawning.Add (p);
			}

		}

		public	static	GameObject	CreateGameObjectForPlayer( Player player, Vector3 pos, Quaternion q ) {

			if (null == PlayingObjectPrefab)
				return null;

			return Instantiate (PlayingObjectPrefab, pos, q );
		}

		/// <summary>
		/// Does a capsule collision check around spawn position.
		/// </summary>
		public	static	bool	CanPlayerBeSpawnedAt( Player player, Vector3 spawnPos, Quaternion q ) {

			if (Physics.CheckCapsule (spawnPos + Vector3.up * 0.4f, spawnPos - Vector3.up * 0.4f, 0.5f))
				return false;

			return true;
		}

		/// <summary>
		/// Randomly selects spawn places around map and tests collision for each of them, until it finds the one
		/// that suits.
		/// </summary>
		public	static	bool	CanPlayerBeSpawnedAtAnySpawnPosition( Player player, ref Vector3 pos, ref Quaternion q ) {
			
			int	playerTeamIndex = TeamManager.singleton.teams.IndexOf (player.Team);

			var spawnPositions = NetworkManager.singleton.startPositions;

			for (int count = 0, i = Random.Range (0, spawnPositions.Count);
				count < spawnPositions.Count; count++, i = (i + 1) % spawnPositions.Count) {

				SpawnPoint spawnPoint = spawnPositions [i].GetComponent<SpawnPoint> ();
				if (null == spawnPoint)
					continue;
				
				if (spawnPoint.teamIndex != -1) {
					if (spawnPoint.teamIndex != playerTeamIndex) {
						continue;
					}
				}

				Vector3 spawnPos = spawnPositions [i].position;
				Quaternion spawnRotation = spawnPositions [i].rotation;

				if (!CanPlayerBeSpawnedAt (player, spawnPos, spawnRotation))
					continue;

				pos = spawnPos;
				q = spawnRotation ;
				return true;
			}


			return false;
		}

	}

}
