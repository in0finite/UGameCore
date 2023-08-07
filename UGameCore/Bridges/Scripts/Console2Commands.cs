using UGameCore.Menu;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore
{
    public class Console2Commands : MonoBehaviour
	{
		public Console console;
		public CommandManager commandManager;


        private void OnEnable()
        {
            this.console.onTextSubmitted += TextSubmitted;
        }

        private void OnDisable()
        {
            this.console.onTextSubmitted -= TextSubmitted;
        }

        void Start()
		{
			this.EnsureSerializableReferencesAssigned();
		}

		void TextSubmitted( string text ) {

            // process it as a command

            // Commands are always executed locally (ie. not sent to server).
            // The actual command callback can decide what to do based on network state, and potentially
            // send the command to server.

            var player = Player.local;

            var context = new CommandManager.ProcessCommandContext
			{
				command = text,
				hasServerPermissions = player != null ? player.IsServerAdmin : true, // only give perms if offline or on dedicated server
				executor = player,
				lastTimeExecutedCommand = player != null ? player.LastTimeExecutedCommand : null,
			};

            var result = this.commandManager.ProcessCommand(context);

			Debug.Log(result.response);
		}
	}
}
