using UnityEngine;

namespace UGameCore {
	
	public class SpawnPlayerWhenLoggedIn : MonoBehaviour
	{

		void OnLoggedIn() {

			PlayingObjectSpawner.MarkPlayerForSpawning (this.GetComponent<Player> ());

		}

	}

}
