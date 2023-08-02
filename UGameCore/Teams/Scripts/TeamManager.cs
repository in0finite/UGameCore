using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore {
	
	public class TeamManager : MonoBehaviour {

		public	List<string>	teams = new List<string> ();
		public	static	string[]	teamNames { get { return singleton.teams.ToArray (); } }
		public	static	int	TeamCount { get { return singleton.teams.Count; } }

		public	bool	isFriendlyFireOn = false ;
		/// <summary>
		/// In free for all mode, every player plays for himself.
		/// </summary>
		[SerializeField]	private	bool	isFreeForAllModeOn = false ;

		public	static	bool	FFA { get { return TeamManager.IsFreeForAllModeOn (); } }

		public	static	TeamManager	singleton { get ; private set ; }



		void Awake () {

			if (singleton != null)
				return;
			
			singleton = this;


			if (1 == TeamCount) {
				Debug.LogError ("There can not be only 1 team");
			}

		}

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {

			if (!NetworkServer.active)
				return;





		}

		public	static	bool	HasAnyTeamToChooseFrom() {
			return TeamCount > 1;
		}

		public	static	bool	IsTeamSpectatingTeam( string team ) {
			if (IsFreeForAllModeOn ()) {
				return null == team;
			} else {
				return "" == team;
			}
		}

		public	static	bool	IsFreeForAllModeOn() {
			return TeamCount < 2 || singleton.isFreeForAllModeOn;
		}

		public	static	bool	SetFreeForAllMode( bool enabled ) {
			if (enabled) {
				singleton.isFreeForAllModeOn = true;
				return true;
			} else {
				// check if teams are valid
				if (TeamCount < 2)
					return false;
				singleton.isFreeForAllModeOn = false;
				return true;
			}
		}

		public	static	bool	IsFriendlyFireOn() {
			return singleton.isFriendlyFireOn;
		}

		public	static	bool	ArePlayersFriendly( Player p1, Player p2 ) {

			if (null == p1 || null == p2)
				return false;

			if (p1 == p2)
				return true;

			if (IsFreeForAllModeOn ())
				return false;

			return p1.Team == p2.Team;
		}

		/// <summary>
		/// Determines if one player can damage another, which is affected by FFA, teams they belong to, friendly fire.
		/// </summary>
		public	static	bool	CanPlayerDamagePlayer( Player attacker, Player target ) {

			if (target != null) {
				if (TeamManager.ArePlayersFriendly (target, attacker)) {
					// check if friendly fire is allowed
					if (!TeamManager.IsFriendlyFireOn ()) {
						// players are in the same team, and friendly fire is not allowed
						return false;
					}
				}
			}

			return true;
		}


		public	int[]	GetNumPlayersInEachTeam() {

			int[] numPlayers = new int[this.teams.Count];
			for (int i = 0; i < numPlayers.Length; i++)
				numPlayers [i] = 0;

			foreach (var player in PlayerManager.players) {
				
				int teamIndex = this.teams.IndexOf (player.Team);
				if (teamIndex < 0)
					continue;

				numPlayers [teamIndex]++;
			}

			return numPlayers;
		}

	}

}
