﻿using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.MapManagement {
	
	public class PopulateDropdownWithMapCycle : MonoBehaviour {

	//	public	Dropdown	sceneSelectDropdown = null ;

		void Start () {

			var dropdown = Utilities.Utilities.FindObjectOfTypeOrLogError<SceneSelectDropdown> ().GetComponent<Dropdown> ();

			dropdown.AddOptions (MapCycle.singleton.mapCycleList);

		}

	}

}
