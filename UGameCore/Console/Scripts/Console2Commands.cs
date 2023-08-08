using System.Collections.Generic;
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
            this.console.onAutoComplete.AddListener(this.OnAutoComplete);
        }

        private void OnDisable()
        {
            this.console.onTextSubmitted -= TextSubmitted;
            this.console.onAutoComplete.RemoveListener(this.OnAutoComplete);
        }

        void Start()
		{
			this.EnsureSerializableReferencesAssigned();
		}

        CommandManager.ProcessCommandContext CreateCommandContext(string text)
        {
            // Commands are always executed locally (ie. not sent to server).
            // The actual command callback can decide what to do based on network state, and potentially
            // send the command to server.

            if (text.IsNullOrWhiteSpace())
                return null;

            var player = Player.local;

            var context = new CommandManager.ProcessCommandContext
            {
                command = text,
                hasServerPermissions = player != null ? player.IsServerAdmin : true, // only give perms if offline or on dedicated server
                executor = player,
                lastTimeExecutedCommand = player != null ? player.LastTimeExecutedCommand : null,
            };

            return context;
        }

        void TextSubmitted( string text ) {

            // process it as a command

            var context = CreateCommandContext(text);
            if (null == context)
                return;

            var result = this.commandManager.ProcessCommand(context);

            if (result.response != null)
            {
                if (result.IsSuccess)
                    Debug.Log(result.response, this);
                else
                    Debug.LogError(result.response, this);
            }
        }

        void OnAutoComplete(string text, int caretPosition)
        {
            if (caretPosition <= 0)
                return;

            string textBeforeCaret = text[..caretPosition];

            var context = CreateCommandContext(textBeforeCaret);
            if (null == context)
                return;

            var possibleCompletions = new List<string>();
            this.commandManager.AutoCompleteCommand(context, out string exactCompletion, possibleCompletions);

            // log all possible completions
            if (possibleCompletions.Count > 0)
                Debug.Log(string.Join("\t\t", possibleCompletions), this);
            
            // assign the exact completion into the InputField, respecting the caret position

            if (exactCompletion == null)
                return;

            string textAfterCaret = text[caretPosition..];

            this.console.consoleSubmitInputField.text = exactCompletion + textAfterCaret;
            this.console.consoleSubmitInputField.caretPosition = exactCompletion.Length;
        }
	}
}
