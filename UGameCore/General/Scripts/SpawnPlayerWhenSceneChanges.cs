using UnityEngine;

namespace UGameCore {

	public class SpawnPlayerWhenSceneChanges : MonoBehaviour
	{

		void OnSceneChanged (SceneChangedInfo info) {

			PlayingObjectSpawner.MarkPlayerForSpawning (this.GetComponent<Player> ());

		}

	}

}
