using UnityEngine;

namespace UGameCore.RoundManagement {

	public class ButtonControllerRoundSystem : MonoBehaviour {


		public	void	EndRound() {

			RoundSystem.singleton.EndRound ("");

		}

	}

}
