using UnityEngine;

namespace uGameCore.RoundManagement {
	
	public class RoundSystemEventsLogger : MonoBehaviour {


		void OnRoundStarted() {

			Debug.Log ("Round started");

		}

		void OnRoundFinished( string winningTeam ) {
			
			Debug.Log (GetTextForLogWhenRoundEnds (winningTeam));

		}

		public	static	string	GetTextForLogWhenRoundEnds(string winningTeam) {

			string logStr = "Round ended - ";

			if (string.IsNullOrEmpty (winningTeam))
				logStr += "no winner";
			else
				logStr += "winning team: " + winningTeam;

			return logStr;
		}

	}

}
