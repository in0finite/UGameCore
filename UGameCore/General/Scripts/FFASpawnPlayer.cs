using UnityEngine;

namespace UGameCore {

	/// <summary>
	/// Can be used to handle player spawning when FFA is turned on and round system is turned off.
	/// </summary>
	public class FFASpawnPlayer : MonoBehaviour {

		public	bool	spawnPlayerWhenHeLogsIn = true;
		public	bool	spawnPlayerWhenSceneChanges = true;



		void Start () {
			
		}

		private void OnLoggedIn ()
		{
			if (TeamManager.FFA) {
				if (this.spawnPlayerWhenHeLogsIn) {
					PlayingObjectSpawner.MarkPlayerForSpawning (this.GetComponent<Player> ());
				}
			}
		}

		private void OnSceneChanged (SceneChangedInfo info)
		{
			if (TeamManager.FFA) {
				if (this.spawnPlayerWhenSceneChanges) {
					PlayingObjectSpawner.MarkPlayerForSpawning (this.GetComponent<Player> ());
				}
			}
		}


	}

}
