using UnityEngine;
using UnityEngine.UI;

namespace UGameCore
{
    public class SpectatorUI : MonoBehaviour
	{
		public Spectator spectator;
		public	Canvas	spectatorCanvas = null;
		public	Text	spectatingObjectText = null;
		public	Button	goPreviousButton = null;
		public	Button	goNextButton = null;


		void Start() {

			// add button handlers

			if (goPreviousButton != null) {
				goPreviousButton.onClick.AddListener (() => { spectator.FindObjectForSpectating( Spectator.DirectionChange.Previous ); });
			}

			if (goNextButton != null) {
				goNextButton.onClick.AddListener (() => { spectator.FindObjectForSpectating( Spectator.DirectionChange.Next ); });
			}

		}

		void Update() {

			this.spectatorCanvas.enabled = spectator.IsSpectating;

			if (this.spectatorCanvas.enabled) {
				// update text based on spectated object

				string text = "";

				var co = spectator.CurrentlySpectatedObject.GetComponent<ControllableObject> ();
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

