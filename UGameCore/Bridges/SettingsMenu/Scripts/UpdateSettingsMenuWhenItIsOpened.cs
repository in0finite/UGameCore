using UnityEngine;

namespace uGameCore.Menu {
	
	public class UpdateSettingsMenuWhenItIsOpened : MonoBehaviour {


		void OnMenuOpened() {

			SettingsMenu.UpdateMenuBasedOnCVars ();
			SettingsMenu.ResetValidStateForAllEntries ();

		}

	}

}
