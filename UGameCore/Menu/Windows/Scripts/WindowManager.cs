using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace uGameCore.Menu.Windows {
	
	public class WindowManager : MonoBehaviour {
		
		public	static	WindowManager	singleton { get ; private set ; }

		static	int	lastId = 0 ;
		static	List<Window>	m_openedWindows = new List<Window> ();
		public	static	IEnumerable<Window>	OpenedWindows { get { return m_openedWindows.WhereAlive (); } }

		private	static	Canvas	m_windowsCanvas = null;
		public	static	Canvas	WindowsCanvas { get { return m_windowsCanvas; } }

		public	GameObject	windowPrefab = null;
		public	GameObject	displayStringPrefab = null;
		public	GameObject	textPrefab = null;
		public	GameObject	buttonPrefab = null;

		// TODO: msgbox size should be in absolute coordinates ?
		public	Vector2	msgBoxSize = new Vector2( 0.25f, 0.2f );



		void Awake () {
			
			singleton = this;

			// find canvas
			m_windowsCanvas = Utilities.Utilities.FindObjectOfTypeOrLogError<WindowsCanvas>().GetComponent<Canvas>();

		}

		void Update () {

			// remove closed windows from list
			m_openedWindows.RemoveAll( wi => null == wi || wi.isClosed );


		}

		void OnGUI() {


//			// display modal windows
//			bool alreadyShownModalWindow = false;
//			foreach (WindowInfo wi in openedWindows) {
//
//				if (wi.isClosed)
//					continue;
//
//				if( wi.isModal ) {
//					if( ! alreadyShownModalWindow ) {
//						GUI.ModalWindow( wi.id, wi.rect, WindowFunction, wi.title );
//						GUI.FocusWindow( wi.id );
//						alreadyShownModalWindow = true ;
//					} else {
//						// display it as regular window
//						GUI.Window( wi.id, wi.rect, WindowFunction, wi.title );
//					}
//				}
//
//			}
//
//			// display non modal windows
//			foreach (WindowInfo wi in openedWindows) {
//
//				if (wi.isClosed)
//					continue;
//
//				if( ! wi.isModal ) {
//					GUI.Window( wi.id, wi.rect, WindowFunction, wi.title );
//				}
//
//			}


		}


		static	void	WindowFunction (int windowID) {

			// find window info by it's id
			Window wi = null ;
			foreach( Window windowInfo in m_openedWindows ) {
				if (windowInfo.id == windowID) {
					wi = windowInfo;
					break;
				}
			}

			if( null == wi )
				return ;

			//	GUILayout.BeginArea (wi.rect);

			// call window procedure
			if (wi.procedure != null) {
				wi.procedure.Invoke (wi);
			}

			//	GUILayout.EndArea ();

			GUI.DragWindow();

		}

		private	static	void	MessageBoxProcedure( Window wi ) {

			// message box with close button

			if (wi.displayStrings.Count > 0) {
				GUILayout.Label (wi.displayStrings [0]);
			}

			GUILayout.FlexibleSpace ();

			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (GameManager.DrawButtonWithCalculatedSize("Close")) {
				CloseWindow (wi);
			}
			GUILayout.EndHorizontal ();

		}

		public	static	void	CloseWindow( Window wi ) {

			wi.isClosed = true;

			if (wi.gameObject != null) {
				Destroy (wi.gameObject);
			}

		//	this.UpdateMouseLockAndVisibilityState ();

		}

		private	static	int		GetNewWindowId() {

			int id = ++lastId;
			return id;

		}

		/// <summary>
		/// Returns a rectangle which is centered on the sceen, and has specified width and height percentages.
		/// </summary>
		public	static	Rect	GetCenteredRect( float screenWidthPercentage, float screenHeightPercentage ) {

			float width = Screen.width * screenWidthPercentage;
			float height = Screen.height * screenHeightPercentage;
			float x = Screen.width / 2 - width / 2;
			float y = Screen.height / 2 - height / 2;
		//	Debug.LogFormat ("x {0} y {1} width {2} height {3} widthPerc {4} heightPerc {5} Screen.width {6} Screen.height {7}", 
		//		x, y, width, height, screenWidthPercentage, screenHeightPercentage, Screen.width, Screen.height);
			return new Rect (x, y, width, height);

		}

		/// <summary>
		/// Reduces height of scroll view from bottom.
		/// </summary>
		/// <param name="amountToReduce">Amount to reduce in normalized coordinates (0 to 1).</param>
		public	static	void	ReduceScrollViewHeightNormalized( Window window, float amountToReduce ) {

			var scrollView = window.scrollView;
			if (null == scrollView)
				return;

			var rt = scrollView.GetRectTransform ();

		//	Vector2 min = rt.anchorMin;
		//	min.y += amountToReduce;
		//	rt.anchorMin = min;

			// don't move corners to anchors, because we have fixed offset from top, which is reserved for title
		//	rt.CornersToAnchors ();

			// don't change anchors, only change offset from bottom (offsetMin)
			Vector2 newOffsetMin = rt.offsetMin;
			newOffsetMin.y += amountToReduce * window.rect.height;
			rt.offsetMin = newOffsetMin;

		}

		public	static	Window	OpenMessageBox( string text, bool isModal ) {

			int width = (int) (singleton.msgBoxSize.x * Screen.width);
			int height = (int) (singleton.msgBoxSize.y * Screen.height);
			return OpenMessageBox ( width, height, text, isModal);

		}

		public	static	Window	OpenMessageBox( string title, string text ) {

			var window = WindowManager.OpenMessageBox (text, false);
			window.Title = title;
			return window;
		}

		public	static	Window	OpenMessageBox( string title, string text, int width, int height ) {

			var window = WindowManager.OpenMessageBox (width, height, text, false);
			window.Title = title;
			return window;
		}

		public	static	Window	OpenMessageBox( int width, int height, string text, bool isModal ) {

			// compute position for window
			Rect rect = GetCenteredRect (width / (float) Screen.width, height / (float) Screen.height);
			rect.position += UnityEngine.Random.insideUnitCircle * 100;


			Action<string,GameObject> processButton = (s, button) => {
				// stretch button to parent size
				var el = button.AddComponent<Utilities.StretchToParentLayoutElement>();
				el.width = 1.0f;	// same width as parent
				el.height = 0.8f;
				el.stretchElement = button.GetComponentInParent<ScrollRect>().GetRectTransform () ;
			};

			var window = OpenWindow( rect, "", new string[] {}, isModal, processButton, MessageBoxProcedure );

			window.gameObject.name = "MessageBox";

			// disable layout group - because we don't need it, and it will screw up position of text component
//			window.contentLayoutGroupEnabled = false;
//			if (window.contentLayoutGroup)
//				Destroy (window.contentLayoutGroup);

			// set alignment of scroll view to middle center - but it doesn't work
//			if (window.contentLayoutGroup) {
//				window.contentLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
//				// rebuild layout
//				LayoutRebuilder.MarkLayoutForRebuild (window.contentLayoutGroup.GetRectTransform ());
//			}

			// let's try to destroy ContentSizeFitter
			if(window.content) {
				var contentSizeFitter = window.content.GetComponent<ContentSizeFitter> ();
				if (contentSizeFitter) {
				//	contentSizeFitter.enabled = false;
				//	Destroy (contentSizeFitter);
				}
			}

			// create text
			if (window.content) {
				var textGameObject = singleton.textPrefab.InstantiateAsUIElement (window.content);

			//	var rt = textGameObject.GetRectTransform ();
			//	rt.SetNormalizedRectAndAdjustAnchors (new Rect(0.05f, 0.05f, 0.9f, 0.9f));

				var textComponent = textGameObject.GetComponentInChildren<Text> ();
				if (textComponent) {
					textComponent.text = text;
					// because vertical layout can only align elements to upper left, we will set text alignment to the same
					textComponent.alignment = TextAnchor.UpperLeft ;
				}
			}

			// add close button
			float closeButtonWidth = 0.3f * 320;
			float closeButtonHeight = 0.15f * 144;
			var closeButton = window.AddButtonBelowContent( closeButtonWidth, closeButtonHeight, "Close");
			closeButton.name = "CloseButton";
			closeButton.GetComponentInChildren<Button> ().onClick.AddListener (() => CloseWindow (window));


//			Debug.LogFormat ("button width {0} button height {1} h_offset {2} v_offset {3} amount reduced {4} window width {5} " +
//				"window height {6} window rect {7}", 
//				closeButtonWidth, closeButtonHeight, closeButtonHorizontalOffset, closeButtonVerticalOffset, amountToReduce, width, height,
//				rect );

			return window;
		}

		public	static	Window	OpenWindow( Rect rect, string title, IEnumerable<string> displayStrings, bool isModal,
			Action<string, GameObject> onDisplayStringCreated, Action<Window> windowProcedure ) {


//			if (wi.isModal) {
//				GUI.FocusControl( "" );
//				GUI.UnfocusWindow ();
//			}

			// create window game object
			var go = singleton.windowPrefab.InstantiateAsUIElement( m_windowsCanvas.transform );
			Window window = go.AddComponentIfDoesntExist<Window>();

			// assign parameters for old GUI system
		//	window.gameObject = go;
			window.title = title;
			window.displayStrings = new List<string> (displayStrings);
			window.isModal = isModal;
			window.rect = new Rect (rect);
			window.procedure = windowProcedure;

			// add click handler which will bring windows canvas to top
		//	go.AddComponent<Utilities.UIEventsPickup>().onPointerClick += (arg) => { WindowManager.singleton.windowsCanvas.sortingOrder = int.MaxValue; };

			// position and size
			window.SetRectangle( rect );

			// title
			window.Title = title ;

			// populate content with display strings
			if (window.content) {
				foreach (var s in displayStrings) {
				
					var displayStringObject = singleton.displayStringPrefab.InstantiateAsUIElement (window.content.transform);

					var textComponent = displayStringObject.GetComponentInChildren<Text> ();
					if (textComponent)
						textComponent.text = s;

					if (onDisplayStringCreated != null)
						onDisplayStringCreated (s, displayStringObject);
				}
			}


			m_openedWindows.Add (window);

			window.id = GetNewWindowId();

			return window;
		}

		public	static	Window	OpenWindow( Rect rect, string title, IEnumerable<string> displayStrings, bool isModal ) {

			return OpenWindow (rect, title, displayStrings, isModal, null, null);

		}


	}

}
