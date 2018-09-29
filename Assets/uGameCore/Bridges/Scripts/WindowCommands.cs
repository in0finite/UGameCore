using System.Collections.Generic;
using UnityEngine;

namespace uGameCore.Commands {

	public class WindowCommands : MonoBehaviour {


		void Start () {

			string[] commands = new string[] { "msgbox", "modal_msgbox", "msgboxtest", "msgboxallclients" };

			foreach (var cmd in commands) {
				CommandManager.RegisterCommand( cmd, ProcessCommand );
			}

		}

		string ProcessCommand( string command ) {
			
			string[] words = CommandManager.SplitCommandIntoArguments (command);
			int numWords = words.Length ;

			string response = "";

			if (words [0] == "msgbox") {

				var msgBox = Menu.Windows.WindowManager.OpenMessageBox ("some example text", false);
				msgBox.Title = "Example message box";

			} else if (words [0] == "modal_msgbox") {

				var msgBox = Menu.Windows.WindowManager.OpenMessageBox ("this is modal message box", true);
				msgBox.Title = "Modal msg box";

			} else if (words [0] == "msgboxtest") {

				// test message box
				// create multiple message boxes with different sizes, text, title

				Vector2[] sizes = new Vector2[] { new Vector2 (300, 100), new Vector2 (320, 200), new Vector2 (500, 300), 
					new Vector2 (200, 300), new Vector2 (800, 500)
				};

				int[] textLengths = new int[]{ 20, 300 };

				foreach (Vector2 size in sizes) {
					foreach (int textLength in textLengths) {
						string text = GenerateRandomString (textLength);
						var msgBox = Menu.Windows.WindowManager.OpenMessageBox ((int)size.x, (int)size.y, text, false);
						// use random title length
						msgBox.Title = GenerateRandomString (Random.Range (0, 30));
						// set random position on screen
						msgBox.SetRectangle (new Rect (new Vector2 (Random.value * Screen.width, Random.value * Screen.height), size));
					}
				}

			} else if (words [0] == "msgboxallclients") {

				CommandManager.EnsureServerIsStarted();

				if (numWords < 3) {
					response += CommandManager.invalidSyntaxText ;
				} else {

					string title = words [1];
					string text = CommandManager.GetRestOfTheCommand (command, 1);

					foreach (var script in Player.GetComponentOnAllPlayers<Menu.Windows.Player2Windows> ()) {
						script.DisplayMsgBoxOnClient( title, text );
					}

				}

			}


			return response;
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
