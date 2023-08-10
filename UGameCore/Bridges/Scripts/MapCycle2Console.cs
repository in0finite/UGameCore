using UnityEngine;
using UGameCore.MapManagement;
using UGameCore.Utilities;

namespace UGameCore
{

    public class MapCycle2Console : MonoBehaviour {

        public Console.Console console;


        void Start () {
			
			this.EnsureSerializableReferencesAssigned();

			this.console.onDrawStats += () => {
				if (NetworkStatus.IsServerStarted) {
					GUILayout.Label (GetTextForConsole1());
					GUILayout.Label (GetTextForConsole2());
				}
			};

            this.console.RegisterStats( () => {
				if (NetworkStatus.IsServerStarted) {
					return GetTextForConsole1();
				}
				return "" ;
			});

            this.console.RegisterStats( () => {
				if (NetworkStatus.IsServerStarted) {
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
