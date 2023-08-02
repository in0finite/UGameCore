using UnityEngine;

namespace uGameCore.RoundManagement {

	public class ButtonControllerRoundSystem : MonoBehaviour {


		public	void	EndRound() {

			RoundSystem.singleton.EndRound ("");

		}

	}

}
