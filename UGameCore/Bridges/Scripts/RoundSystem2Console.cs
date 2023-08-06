using UnityEngine;
using UGameCore.RoundManagement;
using UGameCore.Menu;
using UGameCore.Utilities;

namespace UGameCore
{

    public class RoundSystem2Console : MonoBehaviour {

        public Console console;


        void Start () {

			this.EnsureSerializableReferencesAssigned();

			this.console.onDrawStats += () => {
				if (NetworkStatus.IsServerStarted) {
					GUILayout.Label (GetTextForConsole());
				}
			};

            this.console.RegisterStats( () => {
				if (NetworkStatus.IsServerStarted) {
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
