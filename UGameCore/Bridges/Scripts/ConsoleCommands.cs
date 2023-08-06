using UGameCore.Commands;
using UGameCore.Menu;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore {
	
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
				return "";
			});

            // display all entered commands (history)
            commandManager.RegisterCommand ("history", (cmd) => {
				string output = "";
				foreach(var historyCommand in this.console.History) {
					output += historyCommand + "\n" ;
				}
				return output;
			});

		}
		

	}

}
