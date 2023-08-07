using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore.Commands
{

    public class ChatCommands : MonoBehaviour {

		public CommandManager commandManager;


        void Start () {

			this.EnsureSerializableReferencesAssigned();

			string[] commands = new string[] { "say" };

			foreach (var cmd in commands) {
				this.commandManager.RegisterCommand( cmd, ProcessCommand );
			}

		}

		ProcessCommandResult ProcessCommand(ProcessCommandContext context) {

			string command = context.command;

			string[] words = command.Split( " ".ToCharArray() );
			int numWords = words.Length ;
			string restOfTheCommand = command.Substring (command.IndexOf (' ') + 1);

			if (numWords > 1 && words [0] == "say") {

				if (NetworkStatus.IsClientConnected ()) {

					var chatSync = Player.local.GetComponent<Chat.ChatSync> ();
					if (chatSync != null) {
						chatSync.CmdChatMsg (restOfTheCommand);
					}

					return ProcessCommandResult.Success;
                } else {
					return ProcessCommandResult.Error("This command is only available when you are connected to server.");
				}

			}

			return ProcessCommandResult.InvalidCommand;
		}
	
	
	}

}
