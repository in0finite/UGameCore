using UnityEngine;

namespace uGameCore {
	
	public class SpawnPlayerWhenLoggedIn : MonoBehaviour
	{

		void OnLoggedIn() {

			PlayingObjectSpawner.MarkPlayerForSpawning (this.GetComponent<Player> ());

		}

	}

}
