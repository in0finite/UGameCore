﻿using System.Collections.Generic;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Menu {

	public class ButtonControllerSettingsMenu : MonoBehaviour {

		public	Transform	settingsMenuContent = null ;


		public void SaveSettings() {

			if (null == this.settingsMenuContent)
				return;

			// Save settings.


			// reset valid states
			SettingsMenu.ResetValidStateForAllEntries();


			// Obtain all values from menu and see which ones changed.

			var cvarsToChange = new List<ConfigVar> ();
			var changedValues = new List<ConfigVarValue> ();
			var changedValuesIndexes = new List<int> ();

			int i = 0;
			foreach (var entry in SettingsMenu.GetEntries ()) {

                ConfigVar cvar = entry.cvar;
                ConfigVarValue currentCvarValue = cvar.GetValue();
				ConfigVarValue editedValue = entry.editedValue;

				// compare with current value
				if (!currentCvarValue.Equals (editedValue)) {
					// value is changed
					cvarsToChange.Add(cvar);
					changedValues.Add (editedValue);
					changedValuesIndexes.Add (i);
				}

				i++;
			}


			// Check if all settings are correct, and if they are, save them, otherwise show user which settings are not valid.

			var invalidValuesIndexes = SettingsMenu.AreSettingsValid (cvarsToChange, changedValues);

			if (invalidValuesIndexes.Count > 0) {
				// notify user which settings are invalid

				var entries = new List<SettingsMenu.Entry> (SettingsMenu.GetEntries ());

				foreach (var index in invalidValuesIndexes) {
					int cvarIndex = changedValuesIndexes [index];

					//entries [cvarIndex].control.transform.GetComponent<Image> ().color = Color.red;
					SettingsMenu.SetEntryValidState( entries [cvarIndex], false );
				}

			} else {

                // settings are correct

                // apply new values

                //					foreach (CVar cvar in CVarManager.CVars) {
                //
                //						if( cvar.isInsideCfg ) {
                //							bool isChanged = false ;
                //
                //							if( cvar.displayType == CVarDisplayType.String ) {
                //								if( PlayerPrefs.GetString( cvar.cfgName ) != cvar.currentString ) {
                //									isChanged = true ;
                //
                //									cvarsToChange.Add (cvar);
                //									changedValues.Add (cvar.currentString);
                //								//	CVarManager.ChangeCVarValue( cvar, var.currentString);
                //								}
                //
                //							//	PlayerPrefs.SetString( var.name, var.currentString );
                //
                //							} else if( cvar.displayType == CVarDisplayType.FloatSlider ) {
                //								if( PlayerPrefs.GetFloat( cvar.cfgName ) != cvar.currentFloat ) {
                //									isChanged = true ;
                //
                //									cvarsToChange.Add (cvar);
                //									changedValues.Add (cvar.currentFloat);
                //								//	CVarManager.ChangeCVarValue( cvar, var.currentFloat);
                //								}
                //
                //							//	PlayerPrefs.SetFloat( var.name, var.currentFloat );
                //							}
                //
                //						}
                //
                //					}

                CVarManager cVarManager = SettingsMenu.singleton.configVarManager;

                cVarManager.ChangeCVars (cvarsToChange.ToArray (), changedValues.ToArray ());

                cVarManager.SaveConfigVars();

				string str = "Successfully saved " + cvarsToChange.Count + " cvars: ";
				foreach (var cvar in cvarsToChange) {
					str += cvar.FinalSerializationName + " ";
				}
				Debug.Log (str);

				MenuManager.singleton.OpenParentMenu ();

			}


		}


	}

}
