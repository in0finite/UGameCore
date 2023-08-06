using UGameCore.Menu;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore {
	
	public class ConsoleCommands : MonoBehaviour
	{
		public Console console;

		
		void Start ()
		{

			this.EnsureSerializableReferencesAssigned();

			// clear the console
			Commands.CommandManager.RegisterCommand ("clear", (cmd) => {
                this.console.ClearLog();
				return "";
			});

			// display all entered commands (history)
			Commands.CommandManager.RegisterCommand ("history", (cmd) => {
				string output = "";
				foreach(var historyCommand in this.console.History) {
					output += historyCommand + "\n" ;
				}
				return output;
			});

		}
		

	}

}
