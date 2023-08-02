using UnityEngine;

namespace UGameCore.Menu {
	
	public class UpdateSettingsMenuWhenItIsOpened : MonoBehaviour {


		void OnMenuOpened() {

			SettingsMenu.UpdateMenuBasedOnCVars ();
			SettingsMenu.ResetValidStateForAllEntries ();

		}

	}

}
