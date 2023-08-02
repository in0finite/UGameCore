using UnityEngine;
using UnityEngine.SceneManagement;


namespace uGameCore {

	public class SceneChangedInfo {
		public SceneChangedInfo(Scene scene1, Scene scene2) { s1 = scene1; s2 = scene2; }
		public Scene s1;
		public Scene s2;
	}

	public class SceneManagerEventsForwarder : MonoBehaviour {


		void OnEnable() {

			SceneManager.activeSceneChanged += this.OnSceneManagerEvent_SceneChanged;
			
		}

		void OnDisable() {

			SceneManager.activeSceneChanged -= this.OnSceneManagerEvent_SceneChanged;

		}

		void OnSceneManagerEvent_SceneChanged(Scene s1, Scene s2) {

			string msg = "OnSceneChanged";
			var arg = new SceneChangedInfo (s1, s2);

		//	this.gameObject.BroadcastMessageNoExceptions (msg, arg);

			Utilities.Utilities.SendMessageToAllMonoBehaviours( msg, arg );

		}

	}

}
