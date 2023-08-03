using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UGameCore.Net;

namespace UGameCore {
	
	public class SceneChanger : MonoBehaviour {

		public	static	bool	isLoadingScene { get ; private set ; }


		private	void	OnSceneChanged( SceneChangedInfo info ) {

			isLoadingScene = false;

		}

		/// <summary>
		/// Initiates the process of scene changing using NetworkManager.singleton.ServerChangeScene().
		/// The function will not succeed if a scene is already being loaded, if the specified scene doesn't
		/// exist, or if ServerChangeScene() throws exception.
		/// </summary>
		public	static	bool	ChangeScene( string newScene ) {

			if (isLoadingScene)	// already loading scene
				return false;

		//	if (SceneManager.GetSceneByName (newScene).buildIndex < 0)	// the scene doesn't exist
		//		return false;

			isLoadingScene = true;

			try {
				NetManager.ChangeScene(newScene);
			} catch( System.Exception e ) {

				isLoadingScene = false;
				Debug.LogException (e);

				return false;
			}


			return true;
		}

	}

}
