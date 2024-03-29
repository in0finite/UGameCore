﻿using UnityEngine;
using System.Collections.Generic;
using UGameCore.Net;

namespace UGameCore
{

    public class Player : NetworkBehaviour {


		static	Player() {

			ControllableObject.onStartedOnLocalPlayer += (GameObject go) => Player.local.PlayerGameObject = go ;

		}

		void Awake() {

			m_teamChooser = GetComponent<PlayerTeamChooser> ();

		}

		// Use this for initialization
		void Start () {
		
			// This object needs to be preserved across scenes - so when the server changes scene,
			// this object doesn't get destroyed.
			// But it should be destroyed manually when client disconnects from server.
			DontDestroyOnLoad(this.gameObject);

			
			if (this.isLocalPlayer) {
				local = this;
			}
	

			Player p = this;


			PlayerManager.AddNewPlayer (p);


			if(this.isServer) {
				
			//	p.status = PlayerStatus.ShouldLogin;
				p.m_shouldLogin = true ;

				Debug.Log ("New connection from " + p.clientAddress);
			}


			if (this.isLocalPlayer) {
				Debug.Log ("Logging in.");
				this.CmdLoggingIn (this.clientVersion, Settings.GeneralSettings.Nick);
			}



		}

		void OnSceneChanged( SceneChangedInfo info ) {

			if (!this.isServer)
				return;
			
			//	this.health = 100 ;

//			if ( this.IsLoggedIn() ) {
//
//				if (this.IsBot ()) {
//					this.status = PlayerStatus.ShouldBeSpawnedInNextRound;
//				} else {
//					this.status = PlayerStatus.WaitingToChooseTeam;
//				}
//			}

		}

		void Update () {
			

			Player p = this;

			if (this.isServer) {

				// check if player should be disconnected
			//	if (p.status == PlayerStatus.ShouldDisconnect) {
				if(p.timeUntilDisconnect > 0) {

					p.timeUntilDisconnect -= Time.deltaTime;
					if (p.timeUntilDisconnect <= 0) {
						this.DisconnectPlayer (0, "");
					}

				}

			}

		}


		public	static	IEnumerable<T>	GetComponentOnAllPlayers<T>() where T : Component {

			foreach (var player in PlayerManager.players) {
				var component = player.GetComponent<T> ();
				if (component)
					yield return component;
			}

		}


		public	GameObject	GetControllingGameObject() {

			if (null == this.PlayerGameObject) {
				return this.m_authorityObject;
			}

			return this.PlayerGameObject;
		}


		public	override	void	OnStartLocalPlayer() {
			
			base.OnStartLocalPlayer ();

		//	GameManager.singleton.mainNetworkScript = this;

			
		}


		///<summary>
		/// 
		/// To disconnect the player immediately, set timeout to 0.
		/// 
		/// If you want to send some info to player, for example to explain why is he disconnected,
		/// set timeout to some value higher than 0, for example 3 (to allow message to arrive to player), and set 'reason' accordingly.
		/// In this case, server will wait for 'timeout' seconds, and after that close the connection.
		/// 
		/// </summary>
		public	void	DisconnectPlayer( float timeout, string reason ) {

			if (!this.isServer)
				return;

			if (0.0f >= timeout) {
                // disconnect him immediately

                this.DestroyPlayingObject();

                // Close the connection
                this.conn.Disconnect ();

			} else {
				// Send disconnection message to client, and disconnect him after 'timeout' seconds.
			//	this.status = PlayerStatus.ShouldDisconnect;
				this.timeUntilDisconnect = timeout;
				this.RpcDisconnect (reason);
			}

		}


		public	void	DestroyPlayingObject() {

			if (this.controllingObject != null) {
				Object.Destroy (this.controllingObject);
			}

			this.RemoveAuthorityObjectForPlayer ();

			/*	// remove authority
			if (player.conn != null) {
				foreach (NetworkInstanceId id in player.conn.clientOwnedObjects) {
					if (null == id)
						continue;
					GameObject go = NetworkServer.FindLocalObject (id);
					if (null == go)
						continue;
					NetworkIdentity networkIdentity = go.GetComponent<NetworkIdentity> ();
					if (null == networkIdentity)
						continue;

					if (networkIdentity.clientAuthorityOwner == player.conn)
						networkIdentity.RemoveClientAuthority (player.conn);
				}
			}
			*/


		}

		/// This removes player control over object, but doesn't destroy it.
		public	void	RemoveAuthorityObjectForPlayer() {

			if (this.m_authorityObject != null) {
				ControllableObject ncp = this.m_authorityObject.GetComponent<ControllableObject> ();
				if (ncp != null) {
					ncp.playerOwnerGameObject = null;
					ncp.playerOwner = null;
				}
				this.m_authorityObject = null;
			}

		}


		public	bool	IsLoggedIn() {

			if (this.IsBot ())
				return true;

//			if (this.status != PlayerStatus.ShouldLogin && this.status != PlayerStatus.ShouldDisconnect)
//				return true;

			return m_isLoggedIn;
		}

		public	bool	IsAlive() {

			return this.controllingObject != null || this.m_authorityObject != null;

		}

		public	bool	IsSpectator() {
			
			return m_teamChooser.IsSpectator ;

		}


		public	GameObject	CreateGameObjectForPlayer( Vector3 pos, Quaternion q ) {

			this.DestroyPlayingObject();

			var go = PlayingObjectSpawner.CreateGameObjectForPlayer (this, pos, q);
			if (null == go)
				return null;

            NetManager.Spawn( go );

            // this must be set AFTER the object is spawned, because 'playerGameObject' is a syncvar
            this.PlayerGameObject = go;

			this.PreparePlayerObject ();

			return this.controllingObject;
		}

		private	void	PreparePlayerObject() {

			Player player = this;

			ControllableObject script = player.controllingObject.GetComponent<ControllableObject> ();
			if (script != null) {
				script.playerOwnerGameObject = player.gameObject;
				script.playerOwner = player;
			}

		}

		private	void	OnDied( Player playerWhoKilledYou ) {

			if (!this.isServer)
				return;
			
			this.DestroyPlayingObject ();

		}

		/// <summary>
		/// Message should be sent when a player inflicts damage to an object.
		/// </summary>
		private	void	OnInflictedDamage( InflictedDamageInfo info ) {


		}


		public	bool	IsBot() {

			//	return player.isLocalPlayer;

			return false;
		}


		[ServerRpc]
		private	void	CmdLoggingIn( int[] clientVersion, string name ) {
			
			Player p = this ;


			if(!m_shouldLogin) {
				return ;
			}

			m_shouldLogin = false ;


				if(!this.IsValidClientVersion(clientVersion)) {
					/*
					string logString = "Invalid client version: " ;
					if( clientVersion.Length == 5 ) {
						for( int i=0; i < clientVersion.Length - 1 ; i++ )
							logString += clientVersion[i] + "." ;
						logString += clientVersion[clientVersion.Length - 1] ;
					}
					logString += " from " + base.connectionToClient.address ;

					Debug.Log ( logString );
					RpcDisconnect ("Invalid client version.");
					p.status = PlayerStatus.ShouldDisconnect;
					p.timeUntilDisconnect = 3;

					return;
					*/
				}
			
			clientVersion.CopyTo (p.clientVersion, 0);

		//	NetworkReader reader = new NetworkReader ();
		//	msg.Deserialize (reader);

		//	string name = reader.ReadString ();
			if (!PlayerManager.IsValidPlayerName (name)) {
				Debug.Log ("Invalid name '" + name + "' from " + this.clientAddress);
				RpcDisconnect ("Invalid nick.");
			//	p.status = PlayerStatus.ShouldDisconnect;
				p.timeUntilDisconnect = 3;
				return;
			}
			name = PlayerManager.CheckPlayerNameAndChangeItIfItExists (name);
			p.playerName = name;

		//	p.status = PlayerStatus.WaitingToChooseTeam;

			// we will send him message saying to choose team, but only after we finish loading scene
			// (if we are loading scene at all)

			m_isLoggedIn = true ;

			Debug.Log (p.playerName + " logged in");

			this.gameObject.BroadcastMessageNoExceptions( "OnLoggedIn" );

		}

		private bool IsValidClientVersion( int[] clientVersion ) {

			bool isValidClientVersion = clientVersion.Length == 5 ;
			if (isValidClientVersion) {
				int[] requiredVersion = new int[]{ 5, 0, 0, 0, 0};
				for( int i=0; i < clientVersion.Length ; i++ ) {
					if( clientVersion[i] != requiredVersion[i] ) {
						isValidClientVersion = false ;
						break ;
					}
				}
			}

			return isValidClientVersion;
		}

		[ClientRpc]
		private	void	RpcDisconnect( string infoString ) {
			// Sent from server, when he is about to disconnect this player.
			// Reason could be: error in connecting, player is kicked, player is banned, etc.
			// When client gets this message, he should disconnect, and display infoString.

			if (!isLocalPlayer) {
				return;
			}

			Debug.Log (infoString);

			// broadcast message before stopping client -> it may destroy our game object
			this.gameObject.BroadcastMessageNoExceptions ("OnDisconnectedByServer", infoString);

			NetManager.StopClient ();

		}


		[ClientRpc]
		private	void	RpcAllowedToRequestPlayerSpawning() {

			if (!isLocalPlayer) {
				return;
			}

#if MIRROR
			ClientScene.AddPlayer (5);
#endif

		}


		// Used on clients. Sends message to server to request nick change.
		public	void	ChangeNickOnServer( string newNick ) {

			CmdChangeNick (newNick);

		}

		[ServerRpc]
		private	void	CmdChangeNick( string newNick ) {


			if (!PlayerManager.IsValidPlayerName (newNick)) {
				return;
			}

			if (PlayerManager.GetPlayerByName (newNick) != null) {
				// player with this name already exists
				return;
			}


			this.playerName = newNick;

		}

		public	void	ExecuteCommandOnServer( string cmd ) {

		//	CmdExecuteCommandOnServer (cmd);

		}

		[ServerRpc]
		private	void	CmdExecuteCommandOnServer( string cmd ) {

			// this feature should be disabled, since it is a potential security issue


//			Player p = this ;
//
//
//			Debug.Log ("Player " + p.playerName + " executing command:");
//			Debug.Log (cmd);
//
//			string response = "";
//			Commands.CommandManager.ProcessCommand (cmd, ref response);
//
//			if (response != "") {
//				Debug.Log (response);
//
//				// send response back to client
//				this.RpcLog (response);
//			}

		}

		public CommandManager.ProcessCommandResult ExecuteCommand(CommandManager commandManager, string command)
		{
            bool hasServerPermissions = this.IsServerAdmin;
			double t = this.LastTimeExecutedCommand;
			this.LastTimeExecutedCommand = Time.timeAsDouble;
            return commandManager.ProcessCommand(new CommandManager.ProcessCommandContext
            {
                command = command,
                hasServerPermissions = hasServerPermissions,
                executor = this,
				lastTimeExecutedCommand = t,
            });
        }

		[ClientRpc]
		public	void	RpcExecuteCommandOnClient( string command, bool sendResponse ) {

			if (! isLocalPlayer) {
				return ;
			}

			var context = new CommandManager.ProcessCommandContext
			{
				command = command,
				executor = null,
				hasServerPermissions = true,
				lastTimeExecutedCommand = null,
			};

			var result = CommandManager.Singleton.ProcessCommand (context);

			if (sendResponse) {
				// send response back to server
				CmdSendingCommandResponse (result.response);			
			}
			
		}

		[ServerRpc]
		private	void	CmdSendingCommandResponse( string response ) {
			
			Debug.Log (this.playerName + ": " + response);

		}

		[ClientRpc]
		private	void	RpcLog( string text ) {

			if (isLocalPlayer) {
				Debug.Log( text );
			}

		}

		[ServerRpc]
		public	void	CmdListMaps() {
			
			// Client wants to see all available maps.

			string response = "";

			var maps = MapManagement.MapCycle.singleton.mapCycleList ;
			if (0 == maps.Count) {
				response = "There are no available maps." ;
			} else {
				response = "Available maps [" + maps.Count + "]:\n" ;
				foreach (string mapName in maps) {
					response += mapName + "\n" ;
				}
			}

			RpcLog (response);

		}


		private	void	NameChanged( string newName ) {

			this.playerName = newName;

		}



		[SyncVar]	private	bool	m_isLoggedIn = false;
		private	bool	m_shouldLogin = true;

		[SyncVar]	private	GameObject	m_playerGameObject = null ;
		public	GameObject	PlayerGameObject { get { return m_playerGameObject; } private set { m_playerGameObject = value; } }

		// Object over which the player has authority - it is not his main game object, but an object
		// in a scene whose control can be switched between players.
		[SyncVar]	private	GameObject	m_authorityObject = null ;

		[SyncVar(hook="NameChanged")]	private	string	m_playerName = "" ;
		public	string	playerName { get { return m_playerName; } private set { m_playerName = value; } }

		[SyncVar]	private	short	ping = 0 ;
		public	short Ping { get { return this.ping; } }

		private	PlayerTeamChooser	m_teamChooser = null;
		public	string	Team { get { return m_teamChooser.Team ; } }
		/// <summary>
		/// If there are teams to choose from, determines whether the player choosed team, otherwise
		/// (no teams -> free for all mode ?), returns true.
		/// </summary>
		public	bool	ChoosedTeam { get {
				if (TeamManager.HasAnyTeamToChooseFrom ())
					return this.Team != "";
				return true;
			} }
		
		public	float	health { get {
				var go = this.GetControllingGameObject ();
				if (null == go)
					return 0;
				var d = go.GetComponent<Damagable> ();
				if (null == d)
					return 0;

				return d.Health;
			} }
		

		private	int[]		clientVersion = new int[5] { 5, 0, 0, 0, 0 } ;

		public	NetworkConnection	conn {
			get {
				if (this.isServer)
					return this.connectionToClient;
				return this.connectionToServer;
			}
		}

	//	[System.NonSerialized]	public	PlayerStatus status = PlayerStatus.ShouldLogin ;

		private	float	timeUntilDisconnect = 0 ;

	//	[System.NonSerialized]	public	int	timeWhenConnected = 0 ;

	//	public	GameObject	gameObject { get { return this.controllingObject; } }

	//	[System.NonSerialized]	public	Player	mainNetworkScript = null ;

		// Is player playing on a server. This can be the player who is hosting a game,
		// or bot. These players do not have a connection to server, and their
		// scene is shared with the server.
	//	public	bool	isLocalPlayer = false ;

		public	GameObject	controllingObject { 
			get { 
				return this.PlayerGameObject;
			} 
			set { 
				this.PlayerGameObject = value; 
			}
		}

		public	static	Player	local { get ; private set ; }

		public double LastTimeExecutedCommand { get; private set; } = 0;

		public bool IsServerAdmin { get; set; } = false;

    }

}

