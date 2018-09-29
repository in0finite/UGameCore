using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore {
	
	public class CmdLineArgumentsProcessor : MonoBehaviour {
		

		void Start () {

			ChangeStartupScene.onPreSceneChange += this.Process;

		}

		void Process() {

			// don't do it if in editor
			if (Application.isEditor) {
				return;
			}


			Debug.Log("Processing command line arguments.");

			// process command line arguments
			string commandLineArgumentErrorString = "Command line argument error";
			string[] commandLineArgs = System.Environment.GetCommandLineArgs ();
			int commandLine_Port = 0;
			string	commandLine_ip = "";
			bool	commandLine_useMM = false;
			for (int i=0; commandLineArgs != null && i < commandLineArgs.Length; i++) {

				if (0 == i) {
					// skip first argument, since it is a program path
					continue;
				}

				string arg = commandLineArgs[i] ;

				if (arg.StartsWith ("-port:")) {

					string portNumStr = new string (arg.ToCharArray (6, arg.Length - 6));
					if (!int.TryParse (portNumStr, out commandLine_Port))
						Debug.LogError (commandLineArgumentErrorString + ": invalid port number");

				} else if (arg.StartsWith ("-ip:")) {

					commandLine_ip = new string (arg.ToCharArray (4, arg.Length - 4));

				} else if (arg.StartsWith ("-mm:")) {

					string str = new string (arg.ToCharArray (4, arg.Length - 4));
					int value = 0;
					if (!int.TryParse (str, out value))
						Debug.LogError (commandLineArgumentErrorString + ": invalid mm value");
					commandLine_useMM = 1 == value;

				}

				else if ("-startserver" == arg) {
					/*
					if (i == commandLineArgs.Length - 1) {
						// this is the last argument, so there is no port number specified
						this.LogError (commandLineArgumentErrorString + ":\n" + arg + " : no port number specified.");
					} else {
						// parse port number
						int portNumber = 0;
						if (!int.TryParse (commandLineArgs [i + 1], out portNumber)) {
							this.LogError (commandLineArgumentErrorString + ":\n" + arg + " : invalid port number.");

							break;
						} else {
							// start server
							this.networkManager.StartServer (false, portNumber, this.GetAvailableMaps ());
						}

						// skip the next argument
						i++;
					}
					*/

					if (commandLine_Port > 0) {
						// start server

					//	this.networkManager.StartServer (commandLine_useMM, commandLine_Port, false);

						NetManager.StartServer (commandLine_Port);
					}

				}

				else if ("-connect" == arg) {
					/*
					if (i == commandLineArgs.Length - 1) {
						// this is the last argument, so there is no ip specified
						this.LogError (commandLineArgumentErrorString + ":\n" + arg + " : no ip specified.");
					} else {
						// parse ip and port
						string[] ipAndPort = commandLineArgs [i + 1].Split (":".ToCharArray ());
						if (ipAndPort.Length != 2) {
							this.LogError (commandLineArgumentErrorString + ":\n" + arg + " : invalid ip.");
							break;
						} else {
							string ip = ipAndPort [0];
							int portNumber = 0;
							if (!int.TryParse (ipAndPort [1], out portNumber)) {
								this.LogError (commandLineArgumentErrorString + ":\n" + arg + " : invalid ip.");

								break;
							} else {
								// start connecting to server
								this.networkManager.StartConnecting (false, ip, portNumber);
							}
						}

						// skip the next argument
						i++;
					}
					*/

					if (commandLine_useMM) {
						
					} else {
						if (commandLine_Port > 0 && commandLine_ip.Length > 0) {
							NetManager.StartClient (commandLine_ip, commandLine_Port);
						}
					}

				} else {

					Debug.LogError (commandLineArgumentErrorString + ": unknown argument: " + arg);

				}

			}


		}
		

		public	static	bool	GetArgument( string argName, ref string argValue ) {

			string[] commandLineArgs = System.Environment.GetCommandLineArgs ();
			if (null == commandLineArgs)
				return false;

			if (commandLineArgs.Length < 2)	// first argument is program path
				return false;

			string search = "-" + argName + ":";
			var foundArg = System.Array.Find( commandLineArgs, arg => arg.StartsWith(search) );
			if (null == foundArg)
				return false;

			// found specified argument
			// extract value

			argValue = foundArg.Substring (search.Length);
			return true;
		}

	}

}
