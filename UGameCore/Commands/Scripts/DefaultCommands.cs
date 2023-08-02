using System.Collections.Generic;
using UnityEngine;

namespace uGameCore.Commands {
	
	public class DefaultCommands : MonoBehaviour {


		void Start () {
			
			string[] commands = new string[] { "camera_disable", "uptime", "client_cmd", "players", "kick", "kick_instantly",
				"startserver", "starthost", "connect", "stopnet", "exit"};

			foreach (var cmd in commands) {
				CommandManager.RegisterCommand( cmd, ProcessCommand );
			}

		}
		
		string ProcessCommand( string command ) {

			string[] words = CommandManager.SplitCommandIntoArguments (command);
			int numWords = words.Length ;
			string restOfTheCommand = CommandManager.GetRestOfTheCommand (command, 0);

			string response = "";


			if (2 == numWords && words [0] == "camera_disable") {

				int cameraDisable = int.Parse (words [1]);

				if (Camera.main != null) {

					if (0 == cameraDisable) {
						Camera.main.enabled = true;
					} else if (1 == cameraDisable) {
						Camera.main.enabled = false;
					} else {
						response += "Invalid value.";
					}

				}

			} else if (words [0] == "uptime") {

				response += Utilities.Utilities.FormatElapsedTime (Time.realtimeSinceStartup);

			}
//			else if (words [0] == "server_cmd") {
//
//				if (NetworkStatus.IsClientConnected ()) {
//					if (numWords < 2)
//						response += invalidSyntaxString;
//					else
//						Player.local.ExecuteCommandOnServer (restOfTheCommand);
//				}
//
//			}
			else if (words [0] == "client_cmd") {

				if (NetworkStatus.IsServerStarted ()) {
					if (numWords < 2)
						response += CommandManager.invalidSyntaxText;
					else
						CommandManager.SendCommandToAllPlayers (restOfTheCommand, true);
				}

			} else if (words [0] == "players") {

				// list all players

				response += "name | net id";
				if (NetworkStatus.IsServerStarted ())
					response += " | ip";
				response += "\n";

				foreach (var player in PlayerManager.players) {
					response += player.playerName + " | " + player.netId.Value;
					if (NetworkStatus.IsServerStarted ())
						response += " | " + player.connectionToClient.address;
					response += "\n";
				}

			} else if (words [0] == "kick") {

				if (NetworkStatus.IsServerStarted ()) {
					var p = PlayerManager.GetPlayerByName (restOfTheCommand);
					if (null == p) {
						response += "There is no such player connected.";
					} else {
						p.DisconnectPlayer (3, "You are kicked from server.");
					}

				} else {
					response += "Only server can use this command.";
				}

			} else if (words [0] == "kick_instantly") {

				if (NetworkStatus.IsServerStarted ()) {
					var p = PlayerManager.GetPlayerByName (restOfTheCommand);
					if (null == p) {
						response += "There is no such player connected.";
					} else {
						p.DisconnectPlayer (0, "");
					}

				} else {
					response += "Only server can use this command.";
				}

			} else if (words [0] == "bot_add") {

				if (NetworkStatus.IsServerStarted ()) {

					//					Player player = this.networkManager.AddBot ();
					//					if (player != null)
					//						response += "Added bot: " + player.playerName;
					//					else
					//						response += "Failed to add bot.";
					//

					/*	GameObject go = GameObject.Instantiate( this.playerObjectPrefab );
					if( go != null ) {
						go.GetComponent<NavMeshAgent>().enabled = true ;

						FPS_Character script = go.GetComponent<FPS_Character>();
						script.isBot = true ;
							//	script.playerName = this.networkManager.CheckPlayerNameAndChangeItIfItExists( "bot" );
						// find random waypoints
						GameObject[] waypoints = GameObject.FindGameObjectsWithTag( "Waypoint" );
						if( waypoints.Length > 0 ) {
							int index1 = Random.Range( 0, waypoints.Length );
							int index2 = Random.Range( 0, waypoints.Length );
							if( index1 == index2 ) {
								index2 ++ ;
								if( index2 >= waypoints.Length )
									index2 = 0 ;
							}

							script.botWaypoints.Add( waypoints[index1].transform );
							script.botWaypoints.Add( waypoints[index2].transform );

							script.botCurrentWaypointIndex = 0;
						}

					//	Player player = this.networkManager.AddLocalPlayer( go );

						// the above function assigns name
					//	script.playerName = player.playerName ;

						NetworkServer.Spawn( go );

						script.respawnOnStart = true;
					//	script.Respawn();

						response += "Added bot." ;

					} else {
						response += "Can't create object for bot." ;
					}
				*/

				} else {
					response += "Only server can use this command.";
				}

			} else if (words [0] == "bot_add_multiple") {

				//				if (this.networkManager.IsServer () && NetworkStatus.IsServerStarted ()) {
				//
				//					int numBotsToAdd = 0;
				//					if (2 == numWords && int.TryParse (words [1], out numBotsToAdd)) {
				//
				//						int numBotsAdded = 0;
				//						for (int i = 0; i < numBotsToAdd; i++) {
				//							Player player = this.networkManager.AddBot ();
				//							if (player != null)
				//								numBotsAdded++;
				//						}
				//
				//						response += "Added " + numBotsAdded + " bots.";
				//
				//					} else {
				//						response += invalidSyntaxString;
				//					}
				//				}

			} else if (words [0] == "remove_all_bots") {

				if (NetworkStatus.IsServerStarted ()) {
					int count = 0;
					foreach (var p in PlayerManager.players) {
						if (p.IsBot ()) {
							p.DisconnectPlayer (0, "");
							count++;
						}
					}
					response += "Removed " + count + " bots.";
				}

			} else if (words [0] == "startserver" || words[0] == "starthost") {

				int portNumber = NetManager.defaultListenPortNumber;

				if (numWords > 1)
					portNumber = int.Parse (words [1]);

				if (words [0] == "startserver")
					NetManager.StartServer (portNumber);
				else
					NetManager.StartHost (portNumber);

			} else if (words [0] == "connect") {

				if (numWords != 3) {
					response += CommandManager.invalidSyntaxText;
				} else {
					string ip = words [1];
					int port = int.Parse (words [2]);

					NetManager.StartClient (ip, port);
				}

			} else if (words [0] == "stopnet") {

				NetManager.StopNetwork ();

			} else if (words [0] == "exit") {

				GameManager.singleton.ExitApplication();

			}

			return response ;

		}

	}

}
