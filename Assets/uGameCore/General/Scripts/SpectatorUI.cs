using System;
using UnityEngine;
using UnityEngine.UI;

namespace uGameCore
{
	public class SpectatorUI : MonoBehaviour
	{

		public	Canvas	spectatorCanvas = null;
		public	Text	spectatingObjectText = null;
		public	Button	goPreviousButton = null;
		public	Button	goNextButton = null;


		void Start() {

			// add button handlers

			if (goPreviousButton != null) {
				goPreviousButton.onClick.AddListener (() => { Spectator.FindObjectForSpectating( -1 ); });
			}

			if (goNextButton != null) {
				goNextButton.onClick.AddListener (() => { Spectator.FindObjectForSpectating( 1 ); });
			}

		}

		void Update() {

			this.spectatorCanvas.enabled = Spectator.IsSpectating ();

			if (this.spectatorCanvas.enabled) {
				// update text based on spectated object

				string text = "";

				var co = Spectator.GetSpectatingGameObject ().GetComponent<ControllableObject> ();
				if (co != null) {
					var player = co.playerOwner;
					if (player != null) {
						text = player.playerName + " ";
						if (player.Team != "")
							text += "(" + player.Team + ") ";
						text += "<color=orange>[" + player.health + "]</color>";
					} else {
					//	Debug.LogWarning ("Spectated controllable object has no player owner");
					}
				}

				this.spectatingObjectText.text = text;
			}

		}

	}
}

