using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore.Console
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

            this.commandManager.RegisterCommandsFromTypeMethods(this);

        }

#if UNITY_EDITOR
		[CommandMethod("log100", description = "Log 100 messages")]
        ProcessCommandResult Log100Cmd(ProcessCommandContext context)
        {
            for (int i = 0; i < 100; i++)
				Debug.Log(i + "\n" + i + "\n" + i + "\n" + i, this);
			return ProcessCommandResult.Success;
        }

        [CommandMethod("log1000", description = "Log 1000 messages")]
        ProcessCommandResult Log1000Cmd(ProcessCommandContext context)
        {
            for (int i = 0; i < 1000; i++)
                Debug.Log(i + "\n" + i + "\n" + i + "\n" + i, this);
            return ProcessCommandResult.Success;
        }
#endif

    }

}
