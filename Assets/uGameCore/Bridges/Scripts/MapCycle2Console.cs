using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uGameCore.MapManagement;

namespace uGameCore {
	
	public class MapCycle2Console : MonoBehaviour {


		void Start () {
			
			Menu.Console.onDrawStats += () => {
				if (NetworkStatus.IsServerStarted ()) {
					GUILayout.Label (GetTextForConsole1());
					GUILayout.Label (GetTextForConsole2());
				}
			};


			Menu.Console.RegisterStats( () => {
				if (NetworkStatus.IsServerStarted ()) {
					return GetTextForConsole1();
				}
				return "" ;
			});
			Menu.Console.RegisterStats( () => {
				if (NetworkStatus.IsServerStarted ()) {
					return GetTextForConsole2();
				}
				return "" ;
			});

		}

		static string GetTextForConsole1() {
			return " map time: " + Utilities.Utilities.FormatElapsedTime (MapCycle.singleton.TimePassedSinceStartedMap);
		}

		static string GetTextForConsole2() {
			if (MapCycle.singleton.AutomaticMapChanging)
				return " time left: " + MapCycle.singleton.GetTimeLeftAsString ();
			return "";
		}
		

	}

}
