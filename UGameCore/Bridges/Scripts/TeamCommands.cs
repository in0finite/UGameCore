using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uGameCore.Commands {

	public class TeamCommands : MonoBehaviour {


		void Start () {

			string[] commands = new string[] { "team_change" };

			foreach (var cmd in commands) {
				CommandManager.RegisterCommand( cmd, ProcessCommand );
			}

		}

		string ProcessCommand( string command ) {

			string invalidSyntaxString = "Invalid syntax.";

			string[] words = command.Split( " ".ToCharArray() );
			int numWords = words.Length ;
			string restOfTheCommand = command.Substring (command.IndexOf (' ') + 1);

			string response = "";

		//	var networkManager = UnityEngine.Networking.NetworkManager.singleton;

			if (words [0] == "team_change") {

				if (numWords < 2) {
					response += invalidSyntaxString;
				} else {
					Player player = PlayerManager.GetPlayerByName (restOfTheCommand);
					if (player != null) {
						int currentTeam = TeamManager.singleton.teams.IndexOf (player.Team);
						int newTeam = currentTeam + 1;
						if (newTeam < 0)
							newTeam = 0;
						if (newTeam >= TeamManager.singleton.teams.Count)
							newTeam = 0;

						player.GetComponent<PlayerTeamChooser>().ChangeTeam ( TeamManager.singleton.teams [newTeam] );

					} else {
						response += "There is no such player connected.";
					}
				}

			}

			return response;
		}
	

	}

}
