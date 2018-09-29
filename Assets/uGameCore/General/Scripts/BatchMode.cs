using System;
using UnityEngine;

namespace uGameCore
{
	public class BatchMode : MonoBehaviour
	{


		void Start() {

			string s = "";
			if(CmdLineArgumentsProcessor.GetArgument( "autochooseteam", ref s )) {
				// automatically choose team
				PlayerTeamChooser.onReceivedChooseTeamMessage += (string[] teams) => {
					// choose random team
					var team = teams[ UnityEngine.Random.Range(0, teams.Length) ];
					Player.local.GetComponent<PlayerTeamChooser> ().TeamChoosed ( team );
				};
			}

		}

		private	void	OnServerStopped() {

			string s = "";
			if (CmdLineArgumentsProcessor.GetArgument ("autoexit", ref s)) {
				GameManager.singleton.ExitApplication ();
			}

		}

		private	void	OnClientDisconnected() {

			string s = "";
			if (CmdLineArgumentsProcessor.GetArgument ("autoexit", ref s)) {
				GameManager.singleton.ExitApplication ();
			}

		}


	}
}

