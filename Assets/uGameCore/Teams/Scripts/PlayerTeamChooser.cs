using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace uGameCore {
	
	public class PlayerTeamChooser : NetworkBehaviour {

		private	Player	m_player = null ;

		private	bool	m_shouldChooseTeam = false;
		private	bool	m_shouldSendChooseTeamMessage = false;
		private	List<string>	m_teamsToChooseFrom = new List<string>();
		private	string	m_titleForChoosingTeams = "";

		[SyncVar]	private	string	m_team = "" ;
		public	string Team { get { return m_team; } protected set { m_team = value; } }

		[SyncVar]	private	bool	m_isSpectator = true ;
		public	bool	IsSpectator { get { return this.m_isSpectator; } }

		public	bool	resetTeamOnSceneChange = true ;
		public	bool	sendChooseTeamMessageIfFFAIsOn = true ;

		public	static	event System.Action<string[]>	onReceivedChooseTeamMessage = delegate {};

		/// <summary>
		/// Info about received choose-team message.
		/// </summary>
		public class ReceivedChoosedTeamMessageInfo {
			private string[] teams = null;
			private string title = "";
			public string[] Teams { get { return this.teams; } }
			public string Title { get { return this.title; } }
			public ReceivedChoosedTeamMessageInfo (string[] teams, string title)
			{
				this.teams = teams;
				this.title = title;
			}
		}



		void Awake () {
			m_player = GetComponent<Player> ();
		}

		void Update () {

			if (!this.isServer)
				return;
			

			if (m_shouldSendChooseTeamMessage) {
				// we should send him message saying that he can choose team

				if (!SceneChanger.isLoadingScene && m_player.conn.isReady) {

					m_shouldSendChooseTeamMessage = false;
					m_shouldChooseTeam = true;

					Debug.Log ("Sending choose-team message to " + m_player.playerName);

					this.RpcChooseTeam (m_teamsToChooseFrom.ToArray (), m_titleForChoosingTeams);

				}

			}

		}


		/// <summary>
		/// If the current team is different, destroys playing object and changes the player team. If FFA is on,
		/// player will not be spectator.
		/// </summary>
		public	void	ChangeTeam( string newTeam ) {

			if (!this.isServer)
				return;

			if (!m_player.IsLoggedIn ())
				return;

			if (m_team != newTeam) {
				
				m_player.DestroyPlayingObject ();

				m_team = newTeam;

				// update spectator status
				if (!TeamManager.FFA) {
					m_isSpectator = ! TeamManager.teamNames.Contains (newTeam);
				}

			}

			// update spectator status
			if (TeamManager.FFA) {
				m_isSpectator = false;
			}

		}


		void OnLoggedIn() {
			
			this.OfferPlayerToChooseTeam ();

		}

		void OnSceneChanged( SceneChangedInfo info ) {

			if (!NetworkStatus.IsServer ())
				return;

			if (this.resetTeamOnSceneChange) {
				m_team = "";
				m_isSpectator = true;
				m_shouldChooseTeam = false;
				m_shouldSendChooseTeamMessage = false;

				this.OfferPlayerToChooseTeam ();
			}

		}

		public	bool	OfferPlayerToChooseTeam() {

			if (!NetworkStatus.IsServer ())
				return false;

			if (TeamManager.FFA && !this.sendChooseTeamMessageIfFFAIsOn)
				return false;

			m_teamsToChooseFrom.Clear ();

			if (TeamManager.FFA) {
				m_teamsToChooseFrom.AddRange (new string[]{ "Play" });
				m_titleForChoosingTeams = "";
			} else {
				m_teamsToChooseFrom.AddRange (TeamManager.teamNames);
				m_titleForChoosingTeams = "Choose team";
			}

			m_shouldChooseTeam = false;
			m_shouldSendChooseTeamMessage = true;

			return true;
		}

		[ClientRpc]
		private	void	RpcChooseTeam( string[] teams, string title ) {
			// Server tells us that we can choose team.

			if (!isLocalPlayer) {
				return;
			}

			Debug.Log ("Received choose team message.");

			this.gameObject.BroadcastMessageNoExceptions ("OnReceivedChooseTeamMessage", new ReceivedChoosedTeamMessageInfo(teams, title) );

			onReceivedChooseTeamMessage (teams);

		}

		public	void	TeamChoosed( string teamName ) {

			this.CmdTeamChoosed (teamName);

		}

		[Command]
		private	void	CmdTeamChoosed( string teamName ) {


			if(!m_shouldChooseTeam) {
				return ;
			}

			m_shouldChooseTeam = false ;


			if (TeamManager.FFA) {
				// FFA is on - we have only 1 predefined option

				if ("Play" == teamName) {
					this.Team = "";
					m_isSpectator = false;
				} else {
					// invalid team
					return;
				}

			} else {
				// FFA is off

				// player must select one of the existing teams
				if (!TeamManager.teamNames.Contains (teamName)) {
					// invalid team
					return;
				}

				m_team = teamName;
				m_isSpectator = false;
			}


			Debug.Log (m_player.playerName + " choosed team: " + teamName);

			this.BroadcastChoosedTeamMessage( teamName );

		}

		private	void	BroadcastChoosedTeamMessage( string teamName ) {

			this.gameObject.BroadcastMessageNoExceptions("OnPlayerChoosedTeam", teamName);

		}


	}

}
