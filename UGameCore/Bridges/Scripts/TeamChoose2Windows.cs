using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

namespace uGameCore.Menu.Windows {
	
	public class TeamChoose2Windows : MonoBehaviour {
		
		public	Rect	cancelButtonNormalizedRect = new Rect (0.01f, 0.01f, 0.15f, 0.075f);


		void OnReceivedChooseTeamMessage( PlayerTeamChooser.ReceivedChoosedTeamMessageInfo messageInfo ) {

			CloseAllChooseTeamWindows ();

			// open window for choosing team

			Rect rect = WindowManager.GetCenteredRect( 0.5f, 0.5f );

			var array = new Window[1];
			array[0] = WindowManager.OpenWindow (rect, messageInfo.Title, messageInfo.Teams, false, (s, go) => {
					go.GetComponent<Button>().onClick.AddListener( () => {
					// team choosed
					Player.local.GetComponent<PlayerTeamChooser>().TeamChoosed (s);

					WindowManager.CloseWindow (array[0]);
				} );
			}, WindowProcedure);

			Window window = array [0];
			window.windowTag = window.gameObject.name = "ChooseTeamWindow";

			// create cancel button
			var cancelButtonGo = window.AddButtonBelowContent( 96, 27, "Cancel");
		//	StartCoroutine (this.SetCancelButtonSize (window, cancelButtonGo));
			cancelButtonGo.GetComponent<Button> ().onClick.AddListener (() => { WindowManager.CloseWindow( window ); });

		}

		IEnumerator	SetCancelButtonSize( Window window, GameObject cancelButton ) {

			yield return null;
			yield return null;
			yield return null;
			yield return null;

			var text = cancelButton.GetComponentInChildren<Text> ();

//			TextGenerationSettings generationSettings = layoutElement.GetGenerationSettings (new Vector2 (layoutElement.GetPixelAdjustedRect ().size.x, 0));
//			float width = layoutElement.cachedTextGenerator.GetPreferredWidth (layoutElement.text, generationSettings);
//			float height = layoutElement.cachedTextGenerator.GetPreferredHeight (layoutElement.text, generationSettings);
//
//			Debug.Log ("pref width: " + width + ", height: " + height);
//			
//			this.m_cancelButton.GetComponent<RectTransform> ().SetRectAndAdjustAnchors (new Rect (5, 5, width,
//				height));

			var rt = cancelButton.GetComponent<RectTransform> ();
			var prt = rt.parent as RectTransform;
			rt.SetNormalizedRectAndAdjustAnchors (this.cancelButtonNormalizedRect);

			text.resizeTextForBestFit = true;

			WindowManager.ReduceScrollViewHeightNormalized ( window, this.cancelButtonNormalizedRect.height + this.cancelButtonNormalizedRect.y );
			
		}

		void Update() {

//			if (this.m_setCancelButtonSize) {
//				this.m_setCancelButtonSize = false;
//
//				var layoutElement = this.m_cancelButton.GetComponentInChildren<Text> ();
//				this.m_cancelButton.GetComponent<RectTransform> ().SetRectAndAdjustAnchors (new Rect (5, 5, layoutElement.preferredWidth,
//					layoutElement.preferredHeight));
//			}

		}

		void WindowProcedure( Window wi ) {

			if (NetworkStatus.IsClientDisconnected ()) {
				WindowManager.CloseWindow (wi);
				return;
			}

			GUILayout.Space (Screen.height / 20);

			foreach (string s in wi.displayStrings) {

				if (GameManager.DrawButtonWithCalculatedSize (s)) {
					// team choosed
					Player.local.GetComponent<PlayerTeamChooser>().TeamChoosed (s);

					WindowManager.CloseWindow (wi);
				}

				GUILayout.Space (Screen.height / 60);
			}

			GUILayout.FlexibleSpace ();

			if (GameManager.DrawButtonWithCalculatedSize("Cancel")) {
				WindowManager.CloseWindow (wi);
			}

		}

		void CloseAllChooseTeamWindows() {

			foreach (var w in WindowManager.OpenedWindows.ToList()) {
				if (w.procedure == WindowProcedure) {
					WindowManager.CloseWindow (w);
				}
			}

		}

		void OnSceneChanged( SceneChangedInfo info ) {

			CloseAllChooseTeamWindows();

		}

		void OnDestroy() {

			if (!this.IsLocalPlayer ())
				return;

			// local player has disconnected
			// close window
			// it has to be done here, because OnSceneChanged() will not be called (this game object
			// will be destroyed)

			CloseAllChooseTeamWindows ();

		}


	}

}
