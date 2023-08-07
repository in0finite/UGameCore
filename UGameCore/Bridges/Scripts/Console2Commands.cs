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

			// Process command

			// do it locally (don't send to server), for now

			var player = Player.local;

			var context = new CommandManager.ProcessCommandContext
			{
				command = text,
				hasServerPermissions = true,
				executor = player,
				lastTimeExecutedCommand = player != null ? player.LastTimeExecutedCommand : null,
			};

            var result = this.commandManager.ProcessCommand(context);

			Debug.Log(result.response);
		}
	}
}
