using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore.Chat {


	public	class ChatMessage
	{
		public ChatMessage (string msg, string sender)
		{
			this.msg = msg;
			this.sender = sender;
		}
		
		public	string	msg = "" ;
		public	string	sender = "" ;
	}


	public class ChatManager : MonoBehaviour {
		
		public	static	ChatManager singleton { get ; private set ; }
		public	string	serverChatNick = "<color=green>Server</color>";
		public	static	event System.Action<ChatMessage>	onChatMessage = delegate {};


		void Awake () {
			
			singleton = this;

			onChatMessage += (ChatMessage chatMsg) => Debug.Log ("<color=blue>" + chatMsg.sender + "</color> : " + chatMsg.msg);
			
			ChatSync.onChatMessageReceivedOnServer += (Player p, string msg) => SendChatMessageToAllPlayers( msg, p.playerName ) ;
			ChatSync.onChatMessageReceivedOnLocalPlayer += (ChatMessage chatMsg) => onChatMessage (chatMsg);

		}
		
		// Update is called once per frame
		void Update () {
			
		}

		void	OnSceneChanged( SceneChangedInfo info ) {

			if (UnityEngine.Networking.NetworkServer.active) {
				SendChatMessageToAllPlayersAsServer ("Map changed to " + info.s2.name + ".");
			}

		}


		public	static	void	SendChatMessageToAllPlayersAsServer( string msg ) {

			if (NetworkStatus.IsServerStarted ()) {
				SendChatMessageToAllPlayers (msg, singleton.serverChatNick);
			}

		}

		public	static	void	SendChatMessageToAllPlayersAsLocalPlayer( string msg ) {

			if (null == Player.local) {
				return;
			}

			var chatSync = Player.local.GetComponent<ChatSync> ();
			if (chatSync != null) {
				chatSync.CmdChatMsg (msg);
			}

		}

		/// <summary> Use only on server. </summary>
		public	static	void	SendChatMessageToAllPlayers( string msg, string sender ) {

			if (!NetworkStatus.IsServerStarted ())
				return;

			foreach (var player in PlayerManager.GetLoggedInNonBotPlayers()) {
				SendChatMessageToPlayer ( player, msg, sender );
			}

			if (!NetworkStatus.IsHost ()) {
				// running as dedicated server
				// we should invoke the event here, because there is no local player to receive the chat message
				onChatMessage( new ChatMessage(msg, sender) );
			}

		}

		/// <summary> Use only on server. </summary>
		public	static	void	SendChatMessageToPlayer( Player player, string msg ) {

			if (!NetworkStatus.IsServerStarted ())
				return;

			SendChatMessageToPlayer (player, msg, singleton.serverChatNick);

		}

		private	static	void	SendChatMessageToPlayer( Player player, string msg, string sender ) {

			if (!NetworkStatus.IsServerStarted ())
				return;

			var chatSync = player.GetComponent<ChatSync> ();
			if (chatSync != null) {
				chatSync.TargetChatMsg (player.connectionToClient, msg, sender);
			}

		}

	}

}
