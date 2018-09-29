using UnityEngine;

namespace uGameCore.MapManagement {
	
	public class ButtonControllerMapCycle : MonoBehaviour {


		public	void	ChangeMap() {

			MapCycle.singleton.ChangeMapToNextMap ();

		}

		public	void	StartServerWithSpecifiedMap( UnityEngine.UI.Dropdown dropdown ) {

			try {

				if(dropdown.options.Count < 1)
					throw new System.Exception("No maps available");

				if (dropdown.value < 0)
					throw new System.Exception ("Map not selected");

				string sceneName = dropdown.options [dropdown.value].text;

				int index = MapCycle.singleton.mapCycleList.IndexOf (sceneName);
				if (index < 0)
					throw new System.Exception ("Selected map not found in map cycle list");

				UnityEngine.Networking.NetworkManager.singleton.onlineScene = sceneName;

				MapCycle.singleton.SetCurrentMapIndex (index);

				NetManager.StartHost (NetManager.defaultListenPortNumber);

			} catch( System.Exception ex ) {

				Debug.LogException (ex);

				// notify scripts
				Utilities.FailedToStartServerMessage.Broadcast( ex );
			}

		}

	}

}
