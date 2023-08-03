using System;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore
{
	public class BatchMode : MonoBehaviour
	{


		void Start() {

			if(CmdLineUtils.HasArgument( "autochooseteam")) {
				// automatically choose team
				PlayerTeamChooser.onReceivedChooseTeamMessage += (string[] teams) => {
					// choose random team
					var team = teams[ UnityEngine.Random.Range(0, teams.Length) ];
					Player.local.GetComponent<PlayerTeamChooser> ().TeamChoosed ( team );
				};
			}

		}

		private	void	OnServerStopped() {

			if (CmdLineUtils.HasArgument("autoexit")) {
				GameManager.singleton.ExitApplication ();
			}

		}

		private	void	OnClientDisconnected() {

			if (CmdLineUtils.HasArgument("autoexit")) {
				GameManager.singleton.ExitApplication ();
			}

		}


	}
}

