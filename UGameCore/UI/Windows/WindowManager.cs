using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UGameCore.Utilities;

namespace UGameCore.Menu.Windows {
	
	public class WindowManager : MonoBehaviour {
		
		public	static	WindowManager	singleton { get ; private set ; }

		static	int	lastId = 0 ;
		static	List<Window>	m_openedWindows = new List<Window> ();
		public	static	IEnumerable<Window>	OpenedWindows { get { return m_openedWindows.WhereAlive (); } }

		[Tooltip("Canvas where all windows will be placed")]
		public Canvas windowsCanvas;

		public	GameObject	windowPrefab = null;
        public GameObject messageBoxPrefab = null;
        public GameObject messageBoxConfirmationPrefab = null;
        public	GameObject	displayStringPrefab = null;
		public	GameObject	textPrefab = null;
		public	GameObject	buttonPrefab = null;

		public	Vector2	msgBoxSize = new Vector2( 400, 300 );



		void Awake () {
			
			if (null == singleton)
				singleton = this;

		}

        private void Start()
        {
			this.EnsureSerializableReferencesAssigned();
        }

        void Update () {

			// remove closed windows from list
			m_openedWindows.RemoveDeadObjects();

		}

		// could be used in edit-mode
		private	static	void	MessageBoxProcedure( Window wi ) {

			// message box with close button

			if (wi.displayStrings.Count > 0) {
				GUILayout.Label (wi.displayStrings [0]);
			}

			GUILayout.FlexibleSpace ();

			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (GUIUtils.ButtonWithCalculatedSize("Close")) {
				CloseWindow (wi);
			}
			GUILayout.EndHorizontal ();

		}

		public	static	void	CloseWindow( Window wi ) {

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

			int width = (int) singleton.msgBoxSize.x;
			int height = (int) singleton.msgBoxSize.y;
			return OpenMessageBox (singleton.messageBoxPrefab, width, height, text, isModal);

		}

		public	static	Window	OpenMessageBox( string title, string text ) {

			var window = WindowManager.OpenMessageBox (text, false);
			window.Title = title;
			return window;
		}

		public	static	Window	OpenMessageBox( string title, string text, int width, int height ) {

			var window = WindowManager.OpenMessageBox (singleton.messageBoxPrefab, width, height, text, false);
			window.Title = title;
			return window;
		}

        public static MessageBoxConfirmation OpenMessageBoxConfirm(string title, string text)
        {
            var window = WindowManager.OpenMessageBox(singleton.messageBoxConfirmationPrefab, (int)singleton.msgBoxSize.x, (int)singleton.msgBoxSize.y, text, false);
            window.Title = title;
            return window.GetComponentOrThrow<MessageBoxConfirmation>();
        }

        public	static	Window	OpenMessageBox(GameObject prefab, int width, int height, string text, bool isModal)
		{
			Rect rect = singleton.GetRectForWindow(width, height);

            Action<string,GameObject> processButton = (s, button) => {
				// stretch button to parent size
				var el = button.AddComponent<Utilities.StretchToParentLayoutElement>();
				el.width = 1.0f;	// same width as parent
				el.height = 0.8f;
				el.stretchElement = button.GetComponentInParent<ScrollRect>().GetRectTransform () ;
			};

			var window = OpenWindow(prefab, rect, "", new string[] {}, isModal, processButton, MessageBoxProcedure);

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

			return window;
		}

		public Rect GetRectForWindow(int width, int height)
		{
            Rect rect = GetCenteredRect(width / (float)Screen.width, height / (float)Screen.height);
            rect.position += UnityEngine.Random.insideUnitCircle * 100;
			return rect;
        }

		void AddCloseButton(Window window)
		{
            float closeButtonWidth = 0.3f * 320;
            float closeButtonHeight = 0.15f * 144;
            var closeButton = window.AddButtonBelowContent(closeButtonWidth, closeButtonHeight, "Close");
            closeButton.name = "CloseButton";
            closeButton.GetComponentInChildren<Button>().onClick.AddListener(() => CloseWindow(window));
        }

        public	static	Window	OpenWindow( GameObject windowPrefab, Rect rect, string title, IEnumerable<string> displayStrings, bool isModal,
			Action<string, GameObject> onDisplayStringCreated, Action<Window> windowProcedure ) {


//			if (wi.isModal) {
//				GUI.FocusControl( "" );
//				GUI.UnfocusWindow ();
//			}

			// create window game object
			var go = windowPrefab.InstantiateAsUIElement( singleton.windowsCanvas.transform );
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

			return OpenWindow (singleton.windowPrefab, rect, title, displayStrings, isModal, null, null);

		}
	}
}
