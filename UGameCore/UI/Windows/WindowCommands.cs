using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;
using Random = UnityEngine.Random;

namespace UGameCore.UI.Windows
{

    public class WindowCommands : MonoBehaviour {

		public CommandManager commandManager;
		public WindowManager windowManager;


        void Start () {

			this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
		}

        [CommandMethod("msgbox", "Shows message box", syntax = "([string title], [string message])")]
        ProcessCommandResult MsgBoxCmd(ProcessCommandContext context)
        {
			string title = context.ReadStringOrDefault(null);
            string message = context.ReadStringOrDefault(null);

			WindowManager.OpenMessageBox(title, message);

            return ProcessCommandResult.Success;
        }

        [CommandMethod("msgboxtest", "Runs test on message boxes")]
        ProcessCommandResult MsgBoxTestCmd(ProcessCommandContext context)
        {
			// test message box
			// create multiple message boxes with different sizes, text, title

			Vector2[] sizes = new Vector2[]
			{
				new Vector2 (300, 100), new Vector2 (320, 200), new Vector2 (500, 300),
				new Vector2 (200, 300), new Vector2 (800, 500),
			};

            int[] textLengths = new int[] { 20, 300 };

            foreach (Vector2 size in sizes)
            {
                foreach (int textLength in textLengths)
                {
                    string text = GenerateRandomString(textLength);
                    var msgBox = WindowManager.OpenMessageBox(text, false);
                    // use random title length
                    msgBox.Title = GenerateRandomString(Random.Range(0, 30));
                    // set random position on screen
                    msgBox.SetRectangle(new Rect(new Vector2(Random.value * Screen.width, Random.value * Screen.height), size));
                }
            }

            return ProcessCommandResult.Success;
        }

        [CommandMethod("msgboxallclients", "Sends message box to all clients", syntax = "([string title], [string message])")]
        ProcessCommandResult MsgBoxAllClientsCmd(ProcessCommandContext context)
        {
            NetworkStatus.ThrowIfNotOnServer();

            string title = context.ReadStringOrDefault(null);
            string message = context.ReadStringOrDefault(null);

            foreach (var script in Player.GetComponentOnAllPlayers<Player2Windows>())
				script.DisplayMsgBoxOnClient(title, message);
            
            return ProcessCommandResult.Success;
        }

		private static string GenerateRandomString( int length ) {

			char[] charArray = new char[length];
			for (int i = 0; i < charArray.Length; i++) {
				charArray [i] = (char) Random.Range ('a', 'z');
			}

			// insert additional characters, like space and new line
			char[] charsToInsert = new char[]{ ' ', '\n' };

			int numOfEachCharToInsert = charArray.Length / 15 ;
			for (int i = 0; i < charsToInsert.Length ; i++) {
				for (int j = 0; j < numOfEachCharToInsert; j++) {
					// insert it at random position
					int index = Random.Range( 0, charArray.Length - 1 );
					charArray [index] = charsToInsert[i];
				}
			}

			return new string (charArray);
		}
	}
}
