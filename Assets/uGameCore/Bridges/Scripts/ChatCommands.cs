using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uGameCore.Commands {

	public class ChatCommands : MonoBehaviour {


		void Start () {

			string[] commands = new string[] { "say" };

			foreach (var cmd in commands) {
				CommandManager.RegisterCommand( cmd, ProcessCommand );
			}

		}

		string ProcessCommand( string command ) {

		//	string invalidSyntaxString = "Invalid syntax.";

			string[] words = command.Split( " ".ToCharArray() );
			int numWords = words.Length ;
			string restOfTheCommand = command.Substring (command.IndexOf (' ') + 1);

			string response = "";

			if (numWords > 1 && words [0] == "say") {

				if (NetworkStatus.IsClientConnected ()) {

					var chatSync = Player.local.GetComponent<Chat.ChatSync> ();
					if (chatSync != null) {
						chatSync.CmdChatMsg (restOfTheCommand);
					}

				} else {
					response += "This command is only available when you are connected to server.";
				}

			}

			return response;
		}
	
	
	}

}
