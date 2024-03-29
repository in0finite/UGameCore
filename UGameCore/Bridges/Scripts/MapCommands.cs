﻿using UnityEngine;
using UGameCore.MapManagement;
using UGameCore.Utilities;
using static UGameCore.CommandManager;

namespace UGameCore.Commands
{

    public class MapCommands : MonoBehaviour {

		public CommandManager commandManager;


        void Start () {

			this.EnsureSerializableReferencesAssigned();

			string[] commands = new string[] { "change_scene", "list_maps", "timeleft",
				"nextmap" };

			foreach (var cmd in commands) {
				this.commandManager.RegisterCommand( cmd, ProcessCommand );
			}

		}

        ProcessCommandResult ProcessCommand(ProcessCommandContext context) {

			string command = context.command;

            string[] words = command.Split( " ".ToCharArray() );
			int numWords = words.Length ;
		//	string restOfTheCommand = command.Substring (command.IndexOf (' ') + 1);

			string response = "";

		//	var networkManager = UnityEngine.Networking.NetworkManager.singleton;


			if (2 == numWords && words [0] == "change_scene") {

				string newSceneName = words [1];
				if (NetworkStatus.IsServerStarted) {
					if (newSceneName.Length < 1) {
						response += "Invalid scene name.";
					} else {
						bool mapExists = MapCycle.singleton.mapCycleList.Contains (newSceneName);

						if (mapExists) {
							response += "Changing scene to " + newSceneName + ".";
							SceneChanger.ChangeScene (newSceneName);
						} else {
							response += "This scene does not exist.";
						}
					}
				}

			} else if (words [0] == "list_maps") {

				if (NetworkStatus.IsServerStarted) {
					var maps = MapCycle.singleton.mapCycleList;
					foreach (string mapName in maps) {
						response += mapName + "\n";
					}
				} else {
					if (NetworkStatus.IsClientConnected ()) {
						// Ask server to display all available maps.
						Player.local.CmdListMaps ();
					}
				}

			} else if (words [0] == "timeleft") {

				if (NetworkStatus.IsServerStarted) {
					response += MapCycle.singleton.GetTimeLeftAsString ();
				}

			} else if (words [0] == "nextmap") {

				if (NetworkStatus.IsServerStarted) {
					response += MapCycle.singleton.GetNextMap ();	
				}

			}

			return ProcessCommandResult.SuccessResponse(response);
		}


	}

}
