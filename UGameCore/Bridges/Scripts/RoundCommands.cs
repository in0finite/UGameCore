using UnityEngine;
using UGameCore.RoundManagement;
using static UGameCore.CommandManager;
using UGameCore.Utilities;

namespace UGameCore.Commands
{

    public class RoundCommands : MonoBehaviour {

		public CommandManager commandManager;


        void Start () {

			this.EnsureSerializableReferencesAssigned();

			string[] commands = new string[] { "endround" };

			foreach (var cmd in commands) {
				this.commandManager.RegisterCommand( cmd, ProcessCommand );
			}

		}

        ProcessCommandResult ProcessCommand( ProcessCommandContext context ) {

			string command = context.command;

            string[] words = command.Split( " ".ToCharArray() );
			
			string response = "";

			if (words [0] == "endround") {

				if (NetworkStatus.IsServerStarted) {

					RoundSystem.singleton.EndRound ("");

				}

			}

			return ProcessCommandResult.SuccessResponse(response);
		}
	
	}

}
