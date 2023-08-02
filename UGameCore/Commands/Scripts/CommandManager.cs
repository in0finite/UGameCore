using System.Collections.Generic;
using UnityEngine;

namespace uGameCore.Commands {

	using CommandCallback = System.Func<string, string> ;

	public class CommandManager : MonoBehaviour {

		public	static	CommandManager	singleton { get ; private set ; }

		static	Dictionary<string, CommandCallback>	m_registeredCommands = new Dictionary<string, CommandCallback>();
		public	static	IEnumerable<string>	registeredCommands { get { return m_registeredCommands.Keys; } }

		public	static	string	invalidSyntaxText { get { return "Invalid syntax"; } }

		[SerializeField]	private	List<string>	m_forbiddenCommands = new List<string>();
		/// <summary> Forbidden commands can not be registered. </summary>
		public	static	List<string>	forbiddenCommands { get { return singleton.m_forbiddenCommands; } }



		void Awake() {

			if (null == singleton)
				singleton = this;

			RegisterCommand( "help", ProcessHelpCommand );

		}

		void Start () {
			
		}

		public	static	void	RegisterCommand( string command, CommandCallback callback ) {

			if (CommandManager.forbiddenCommands.Contains (command)) {
				// this command is forbidden
				return ;
			}

			if (m_registeredCommands.ContainsKey (command))
				return;

			m_registeredCommands.Add (command, callback);

		}

		public	static	bool	RemoveCommand( string command ) {

			return m_registeredCommands.Remove (command);

		}

		public	static	string[]	SplitCommandIntoArguments( string command ) {

			// TODO: add support for arguments that have spaces, i.e. those enclosed with quotes

			return command.Split (new string[]{ " ", "\t" }, System.StringSplitOptions.RemoveEmptyEntries);

		}

		public	static	string	GetRestOfTheCommand( string command, int argumentIndex ) {

			if (argumentIndex < 0)
				return "";

			string[] args = SplitCommandIntoArguments (command);

			if (argumentIndex > args.Length - 2)
				return "";

			return string.Join( " ", args, argumentIndex + 1, args.Length - argumentIndex - 1);

		}

		public	static	int		ProcessCommand( string command, ref string response ) {

			if (string.IsNullOrEmpty (command))
				return -1;

			string[] arguments = SplitCommandIntoArguments (command);
			if (0 == arguments.Length)
				return -1;
			
			// find a handler for this command and invoke it

			CommandCallback callback = null;
			if (m_registeredCommands.TryGetValue (arguments [0], out callback)) {

				// we need separate variable, because 'ref' parameters can not be used in lambda
				string responseFromHandler = "";

				// TODO: should this be exception safe ?
				Utilities.Utilities.RunExceptionSafe (() => {
					responseFromHandler = callback (command);
				});

				// assign response
				response = responseFromHandler ;

				return 0;
			} else {
				response = "Unknown command: " + command;
			}

			return -1;
		}

		static	string	ProcessHelpCommand( string command ) {

			string response = "List of available commands:\n";

			foreach (var pair in m_registeredCommands) {
				response += pair.Key + "\n";
			}

			response += "\n";

			return response;
		}
		
		public	static	void	SendCommandToAllPlayers( string command, bool sendResponse ) {

			foreach (var p in PlayerManager.players) {
				p.RpcExecuteCommandOnClient( command, sendResponse );
			}

		}

		/// <summary>
		/// Throws exception if server is not started, with explanation that command can only be used on server.
		/// </summary>
		public	static	void	EnsureServerIsStarted() {

			if (!NetworkStatus.IsServerStarted ()) {
				throw new System.Exception ("Only server can use this command");
			}

		}

	}

}
