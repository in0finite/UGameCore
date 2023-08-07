using UGameCore.Menu;
using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore
{

    public class ConsoleCommands : MonoBehaviour
	{
		public Console console;
		public CommandManager commandManager;


        void Start ()
		{

			this.EnsureSerializableReferencesAssigned();

            // clear the console
            commandManager.RegisterCommand ("clear", (cmd) => {
                this.console.ClearLog();
				return ProcessCommandResult.Success;
			});

            // display all entered commands (history)
            commandManager.RegisterCommand ("history", (cmd) => {
				string output = "";
				foreach(var historyCommand in this.console.History) {
					output += historyCommand + "\n" ;
				}
				return ProcessCommandResult.SuccessResponse(output);
			});

		}
		

	}

}
