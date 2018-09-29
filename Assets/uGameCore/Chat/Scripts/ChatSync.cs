using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore.Chat {
	
	public class ChatSync : NetworkBehaviour {

		public	static	event System.Action<Player, string>	onChatMessageReceivedOnServer = delegate {};
		public	static	event System.Action<ChatMessage>	onChatMessageReceivedOnLocalPlayer = delegate {};

		
		void Start () {
			
		}


		[Command]
		public	void	CmdChatMsg( string msg ) {
			
			Player p = GetComponent<Player>() ;


			// Remove tags.
			msg = msg.Replace ("<", "");	// the only easy way :D
			msg = msg.Replace (">", "");
			//	msg = msg.Replace ("<color", "color");
			//	msg = msg.Replace ("<size", "size");
			//	msg = msg.Replace ("<b>", "");
			//	msg = msg.Replace ("<i>", "");
			//	msg = msg.Replace (">", "\\>");

			// Forward this message to all clients including the sender.
		//	ChatManager.SendChatMessageToAllPlayers( msg, p.playerName );
			onChatMessageReceivedOnServer( p, msg );


		}

		[TargetRpc]
		public	void	TargetChatMsg( NetworkConnection conn, string msg, string sender ) {

			if (!this.isLocalPlayer) {
				return;
			}

			onChatMessageReceivedOnLocalPlayer (new ChatMessage (msg, sender));

		}

	}

}
