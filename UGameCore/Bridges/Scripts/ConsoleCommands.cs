using UnityEngine;

namespace uGameCore {
	
	public class ConsoleCommands : MonoBehaviour
	{
		
		void Start ()
		{

			// clear the console
			Commands.CommandManager.RegisterCommand ("clear", (cmd) => {
				Menu.Console.ClearLog();
				return "";
			});

			// display all entered commands (history)
			Commands.CommandManager.RegisterCommand ("history", (cmd) => {
				string output = "";
				foreach(var historyCommand in Menu.Console.History) {
					output += historyCommand + "\n" ;
				}
				return output;
			});

		}
		

	}

}
