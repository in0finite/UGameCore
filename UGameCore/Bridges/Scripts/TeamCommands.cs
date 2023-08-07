using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore.Commands
{

    public class TeamCommands : MonoBehaviour {

		public CommandManager commandManager;


        void Start () {

			this.EnsureSerializableReferencesAssigned();

			string[] commands = new string[] { "team_change" };

			foreach (var cmd in commands) {
				this.commandManager.RegisterCommand( cmd, ProcessCommand );
			}

		}

        ProcessCommandResult ProcessCommand(ProcessCommandContext context) {

			string command = context.command;
            string invalidSyntaxString = CommandManager.invalidSyntaxText;

			string[] words = command.Split( " ".ToCharArray() );
			int numWords = words.Length ;
			string restOfTheCommand = command.Substring (command.IndexOf (' ') + 1);

			string response = "";

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

			return ProcessCommandResult.SuccessResponse(response);
		}
	

	}

}
