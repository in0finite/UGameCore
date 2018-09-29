using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uGameCore.RoundManagement;

namespace uGameCore.Commands {

	public class RoundCommands : MonoBehaviour {


		void Start () {

			string[] commands = new string[] { "endround" };

			foreach (var cmd in commands) {
				CommandManager.RegisterCommand( cmd, ProcessCommand );
			}

		}

		string ProcessCommand( string command ) {

		//	string invalidSyntaxString = "Invalid syntax.";

			string[] words = command.Split( " ".ToCharArray() );
			int numWords = words.Length ;
		//	string restOfTheCommand = command.Substring (command.IndexOf (' ') + 1);

			string response = "";

		//	var networkManager = UnityEngine.Networking.NetworkManager.singleton;

			if (words [0] == "endround") {

				if (NetworkStatus.IsServerStarted ()) {

					RoundSystem.singleton.EndRound ("");

				}

			}

			return response;
		}
	
	}

}
