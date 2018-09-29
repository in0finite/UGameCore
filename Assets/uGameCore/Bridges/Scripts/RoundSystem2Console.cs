using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uGameCore.RoundManagement;

namespace uGameCore {
	
	public class RoundSystem2Console : MonoBehaviour {


		void Start () {
			Menu.Console.onDrawStats += () => {
				if (NetworkStatus.IsServerStarted ()) {
					GUILayout.Label (GetTextForConsole());
				}
			};

			Menu.Console.RegisterStats( () => {
				if (NetworkStatus.IsServerStarted ()) {
					return GetTextForConsole();
				}
				return "" ;
			});
		}

		static string GetTextForConsole() {
			if (RoundSystem.EnableRoundSystem)
				return " round time: " + Utilities.Utilities.FormatElapsedTime (Time.time - RoundSystem.singleton.TimeWhenRoundStarted);
			return "";
		}

	}

}
