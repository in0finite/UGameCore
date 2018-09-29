using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Reflection;
using System.Linq;


namespace uGameCore {
	

	/*
	public	enum MenuType
	{
		PauseMenu = 1,
		MainMenu,
		OptionsMenu,
		FirstTimeEnterNickMenu,
		InGame,
		Dummy,
		Other

	}
	*/


	public	interface IForbidUserInput {

		bool	CanGameObjectsReadInput ();

	}

	public	interface IForbidGuiDrawing {

		bool	CanGameObjectsDrawGui ();

	}



	public class GameManager : MonoBehaviour {
		
		

		void	Awake() {

			if (singleton != null) {
				// error
				// multiple instances of singleton object

				Debug.LogError (
					"Multiple instances of singleton object detected. The reasons could be:\n" +
					"- startup scene is loaded more than once\n" +
					"- you created singleton object in non-startup scene\n" +
					"- you manually instantiated singleton object from script\n" +
					"Singleton objects should only be created in startup scene (first scene in build settings)."
				);

				Debug.LogError ("Quitting");
				ExitApplication ();

				return;
			}

			var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
			if (scene.buildIndex != 0) {
				// error
				// game manager created in non-startup scene

				Debug.LogError("Game manager created in non-startup scene. It must be created only in startup " +
					"scene (first scene in build settings).");

				Debug.LogError ("Quitting");
				ExitApplication ();

				return;
			}


			singleton = this;

			
		}

		void Start () {
			

			Application.runInBackground = true;


			this.fpsStopwatch.Start ();


			Debug.Log (Utilities.Utilities.GetAssetName() + " started");

		}

		void Update () {
			

			// calculate average fps
			float timeElapsed = this.fpsStopwatch.ElapsedMilliseconds / 1000f ;
			if (0f == timeElapsed)
				timeElapsed = float.PositiveInfinity;
			this.fpsStopwatch.Reset ();
			this.fpsStopwatch.Start ();

			float fpsNow = 1.0f / timeElapsed ;
			fpsSum += fpsNow ;
			fpsSumCount ++ ;

			if( Time.time - lastTimeFpsUpdated > secondsToUpdateFps ) {
				// Update average fps
				if( fpsSumCount > 0 ) {
					averageFps = fpsSum / fpsSumCount ;
				} else {
					averageFps = 0 ;
				}

				fpsSum = 0 ;
				fpsSumCount = 0 ;

				lastTimeFpsUpdated = Time.time ;
			}


			if (NetworkStatus.IsClientConnecting()) {
				this.timePassedSinceStartedConnectingToServer += Time.deltaTime ;
			}


		}
		
		void OnGUI () {


			// Set font and controls size based on screen size, platform, etc. It's done here because screen size can
			// be changed during runtime.
			{
				GUIStyle[] styles = new GUIStyle[] { GUI.skin.label, GUI.skin.button, GUI.skin.box,
					GUI.skin.textArea, GUI.skin.textField };
				float fontSizeModifier = 1 / 50.0f;
				#if UNITY_ANDROID
				fontSizeModifier = 1 / 25.0f ;
				#endif
				foreach (GUIStyle s in styles) {
					s.fontSize = Mathf.RoundToInt (Mathf.Min (Screen.width, Screen.height) * fontSizeModifier);
				}

				#if UNITY_ANDROID
				float scrollbarSize = Mathf.Min (Screen.width, Screen.height) / 600.0f * 20 ;
				float scrollbarThumbSize = Mathf.Min (Screen.width, Screen.height) / 600.0f * 19 ;
				GUI.skin.verticalScrollbar.fixedWidth = scrollbarSize;
				GUI.skin.verticalScrollbarThumb.fixedWidth = scrollbarThumbSize;
				GUI.skin.horizontalScrollbar.fixedHeight = scrollbarSize;
				GUI.skin.horizontalScrollbarThumb.fixedHeight = scrollbarThumbSize;

				GUI.skin.toggle.fixedHeight = Mathf.Min (Screen.width, Screen.height) / 600.0f * 25 ;
				GUI.skin.toggle.fontSize = Mathf.RoundToInt (Mathf.Min (Screen.width, Screen.height) / 37.5f);
				#endif
			}


//			// Draw chat.
//			if( NetworkStatus.IsClientConnected () || NetworkStatus.IsServerStarted () )
//			{
//
//				int chatFontSize = GUI.skin.label.fontSize * 3 / 4 ;
//				int width = Screen.width / 2 ;
//				int height = Screen.height / 3 ;
//				int x = 5 ;
//				int y = Screen.height - height - 5 ;
//				Rect rect = new Rect (x, y, width, height);
//				//	GUI.Box (rect, "");
//				GUILayout.BeginArea (rect);
//				GUILayout.BeginScrollView( new Vector2( 0.0f, Mathf.Infinity ) );
//				
//				foreach( ChatMessageQueueInfo info in this.chatMessagesQueue ) {
//					//	ChatMessageQueueInfo info = (ChatMessageQueueInfo) o ;
//					if( null == info )
//						continue ;
//					
//					GUILayout.Label( "<size=" + chatFontSize + "><color=blue>" + info.sender + "</color> : <color=yellow>" + info.msg + "</color></size>" );
//					
//				}
//				
//				GUILayout.EndScrollView();
//				GUILayout.EndArea ();
//			}


//			// draw kill events
//			if (NetworkStatus.IsClientConnected () || NetworkStatus.IsServerStarted ()) {
//
//				GUILayout.BeginArea (new Rect(0, 0, Screen.width - 3, Screen.height) );
//
//				foreach( KillEventQueueInfo info in this.killEventsQueue ) {
//					GUILayout.BeginHorizontal ();
//					GUILayout.FlexibleSpace ();
//					GUILayout.Label( info.killer + "<color=red> x-> </color>" + info.dier );
//					GUILayout.EndHorizontal ();
//				}
//
//				GUILayout.EndArea ();
//			}


			/*
			// draw menus
			if (this.openedMenuType == MenuType.MainMenu) {
				this.DrawMainMenu ();
			} else if (this.openedMenuType == MenuType.OptionsMenu) {
			//	this.DrawOptionsMenu ();
			} else {
				if (this.openedMenuType == MenuType.Other) {
					// send message to all scripts in this game object
					// or let them detect if their menu is opened

					this.gameObject.BroadcastMessage ("OnDrawMenu", openedMenuStringPath, SendMessageOptions.DontRequireReceiver);

				}
			}
			*/


			/*
			// draw chat input edit box
			if (this.openedMenuType != MenuType.InGame
				&& (NetworkStatus.IsClientConnected () || NetworkStatus.IsServerStarted ())) {
				// draw it in right bottom corner

				int chatInputEditBoxWidth = Screen.width / 5;
			//	int chatInputEditBoxHeight = 30;
			//	int chatSubmitButtonWidth = 50;
			//	int chatSubmitButtonHeight = 27;
			//	int chatLabelHeight = 20;

			//	int chatAreaWidth = chatInputEditBoxWidth + 5 + chatSubmitButtonWidth ;
			//	int chatAreaHeight = chatInputEditBoxHeight + 10 + chatLabelHeight ;


			//	GUILayout.BeginArea (new Rect (0,
			//		Screen.height - chatAreaHeight, Screen.width, chatAreaHeight));
				GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );
				GUILayout.FlexibleSpace ();

				GUILayout.BeginHorizontal ();
			//	GUILayout.FlexibleSpace ();
				GUILayout.Label ("<b><color=black> CHAT</color></b>" );
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
			//	GUILayout.FlexibleSpace();

				GUILayout.Space (2);
				bool bSend = GUILayout.Button ("Send");
				GUI.SetNextControlName ("chat_input");
				this.chatInputEditBoxText = GUILayout.TextField (this.chatInputEditBoxText, 1000, GUILayout.Width (chatInputEditBoxWidth) );
				if (!bSend) {
					if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl () == "chat_input") {
						bSend = true;
					}
				}

				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal();

				GUILayout.Space (2);

				GUILayout.EndArea ();


				if (bSend) {
					if (this.networkManager.IsClient ()) {
						if (this.mainNetworkScript != null && ClientScene.ready) {
							this.mainNetworkScript.CmdChatMsg (this.chatInputEditBoxText);
							this.chatInputEditBoxText = "";
						}
					} else if (this.networkManager.IsServer () && ! SceneChanger.isLoadingScene ) {
						ChatManager.SendChatMessageToAllPlayersAsServer (this.chatInputEditBoxText);
						this.chatInputEditBoxText = "";
					}
				}

			}
			*/


		}

		public	static	bool	DrawButtonWithCalculatedSize( string text ) {

			Vector2 size = GUI.skin.button.CalcScreenSize (GUI.skin.button.CalcSize (new GUIContent (text)));

			return GUILayout.Button (text, GUILayout.Width (size.x), GUILayout.Height (size.y));
		}


//		public	void	DrawMainMenu() {
//
//			int buttonWidth = (int) GUI.skin.button.CalcSize (new GUIContent ("aaaaaaaaaaaaa")).x; //Mathf.Min( Screen.width / 7, 200 );	// 90 (640), 200 (1366), 270 (1920)
//			int mainButtonsAreaHeight = Screen.height / 2;	// 240 (480), 380 (768), 540 (1080)
//			int buttonHeight = Mathf.Min( (int) (35.0f * Screen.height / 650.0f), 40 );	// 24 (480), 38 (768), 54 (1080)
//
//			int x = Screen.width / 2 - buttonWidth / 2;
//			int y = Screen.height / 2 - mainButtonsAreaHeight / 2;
//			int areaWidth = buttonWidth;	//Mathf.RoundToInt (buttonWidth * 1.1f);
//
//			GUIStyle buttonStyle = new GUIStyle (GUI.skin.button);
//		//	buttonStyle.fixedHeight = buttonHeight;
//
//
//			// Draw box as background.
//			string boxString = "<b>MAIN MENU</b>" ;
//			int box_offset_x = (int)(Screen.height / 600.0f * 30);
//			int box_offset_y = (int) GUI.skin.box.CalcScreenSize (GUI.skin.box.CalcSize (new GUIContent (boxString))).y + 3;	//(int)(Screen.height / 600.0f * 20);
//			GUI.Box( new Rect( x - box_offset_x, y - box_offset_y, buttonWidth + 2 * box_offset_x, mainButtonsAreaHeight + 2 * box_offset_y ), boxString );
//
//
//			GUILayout.BeginArea (new Rect ( x, y, buttonWidth, mainButtonsAreaHeight));
//
//			GUILayout.FlexibleSpace ();
//
//			this.mainMenuScrollBarPosition = GUILayout.BeginScrollView (this.mainMenuScrollBarPosition);
//
//
//			#if SERVER
//
//			// Display gui controls to start server.
////			if (!networkManager.IsClient() && !networkManager.IsServer()) {
////
////				GUILayout.Label( "Listen port: " );
////				IntInput.Display( ref this.listenPortInputInfo );
////
////				this.useMMForServer = GUILayout.Toggle( this.useMMForServer, "use mm" );
////
////				bool startServer = GUILayout.Button ("Start server", buttonStyle);
////			//	bool startHost = GUILayout.Button ("Start host", buttonStyle);
////				if (startServer) {
////
////					if( this.listenPortInputInfo.outValue > 0 ) {
////						
////					//	this.networkManager.StartServer (this.useMMForServer, this.listenPortInputInfo.outValue, true );
////
////						NetworkManager.singleton.networkPort = this.listenPortInputInfo.outValue ;
////						NetworkManager.singleton.StartHost();
////					}
////
////				}
////
////			}
//
//			// Display controls to stop the server.
////			if (networkManager.IsServer()) {
////
////				if (GUILayout.Button ("Stop server", buttonStyle)) {
////					NetworkManager.singleton.StopServerAndHost();
////				}
////
////			}
//
//			// display buttons for controlling server
//			
////			if( NetworkStatus.IsServerStarted () ) {
////			
////				GUILayout.Space(Screen.height / 25.0f);
////
////				/*
////				if( GUILayout.Button("Add 10 bots", GUILayout.Height(buttonHeight)) ) {
////					for( int i=0; i < 10; i++ )
////						this.networkManager.AddBot();
////				}
////
////				if( GUILayout.Button("Remove 5 bots", GUILayout.Height(buttonHeight)) ) {
////					int numRemoved = 0 ;
////					for( int i=0; i < this.networkManager.players.Count; i++ ) {
////						if( this.networkManager.IsPlayerBot( this.networkManager.players[i] ) ) {
////							this.networkManager.DisconnectPlayer( this.networkManager.players[i], 0, "" );
////							i-- ;
////							numRemoved++ ;
////							if( numRemoved >= 5 )
////								break ;
////						}
////					}
////				}
////				*/
////
////				if( GUILayout.Button("End round", buttonStyle) ) {
////					RoundSystem.singleton.EndRound("");
////				}
////
////				if( GUILayout.Button("Change map", buttonStyle) ) {
////					MapCycle.singleton.ChangeMapToNextMap();
////				}
////
////				GUILayout.Space(Screen.height / 25.0f);
////
////			}
//
//			#endif
//
//			// Display controls to connect to server.
////			if ( (!networkManager.IsClient() && !networkManager.IsServer()) || (networkManager.IsClient() && NetworkStatus.IsClientDisconnected ()) ) {
////
////				#if ALLOW_CLIENT_TO_CHOOSE_CONNECTION_METHOD
////
////				GUILayout.Label( "Ip: " );
////				this.serverIpString = GUILayout.TextArea( this.serverIpString );
////
////				GUILayout.Label( "Port: " );
////				IntInput.Display( ref this.serverPortInfo );
////
////				this.useMMForClient = GUILayout.Toggle (this.useMMForClient, "use mm");
////
////				GUILayout.Space (4);
////
////				#endif
////
////				if (GUILayout.Button ("Connect", buttonStyle)) {
////
////					bool bConnect = false;
////					if (this.useMMForClient) {
////						bConnect = true;
////					} else {
////						#if ALLOW_CLIENT_TO_CHOOSE_CONNECTION_METHOD
////						if (this.serverPortInfo.outValue > 0 && this.serverIpString.Length > 0) {
////							bConnect = true;
////						}
////						#endif
////					}
////
////					if (bConnect) {
////						
////					//	this.networkManager.StartConnecting (this.useMMForClient, this.serverIpString, this.serverPortInfo.outValue);
////
////						NetworkManager.singleton.StartClient ( this.serverIpString, this.serverPortInfo.outValue );
////
////						this.timePassedSinceStartedConnectingToServer = 0;
////					}
////
////				}
////
////			}
//
//			// Display connection progress.
////			if (NetworkStatus.IsClientConnecting()) {
////
////				float percentage = this.timePassedSinceStartedConnectingToServer - Mathf.Floor( this.timePassedSinceStartedConnectingToServer );
////
////				string s = "Connecting." ;
////				for( int i=0 ; i < (int)(3 * percentage) ; i++ ) {
////					s += "." ;
////				}
////				GUILayout.Label( s );
////
////				// display moving box - to act as a progress bar
////
////				GUILayout.BeginHorizontal();
////
////				int offset = (int) ( percentage * areaWidth ) ;
////				GUILayout.Space( offset );
////
////				int boxWidth = 40;
////				if (offset + boxWidth + 5 > areaWidth)
////					boxWidth = areaWidth - offset - 5;
////				
////				if (boxWidth > 7)
////					GUILayout.Box ("", GUILayout.Width (boxWidth), GUILayout.Height (buttonHeight));
////				else
////					GUILayout.Label ("", GUILayout.Width (boxWidth), GUILayout.Height (buttonHeight));
////
////				GUILayout.EndHorizontal();
////
////			}
//
//			// Display controls to disconnect from server.
////			if (!networkManager.IsServer() && networkManager.IsClient() && (NetworkStatus.IsClientConnected () || NetworkStatus.IsClientConnecting())) {
////
////				string buttonName = "Disconnect" ;
////				if( NetworkStatus.IsClientConnecting() )
////					buttonName = "Cancel" ;
////
////				if (GUILayout.Button ( buttonName, buttonStyle)) {
////					
////				//	this.networkManager.StartDisconnecting();
////
////					NetworkManager.singleton.StopClient ();
////				}
////
////			}
//
//			// display buttons for controlling server from client
//			#if UNITY_EDITOR
////			if (NetworkStatus.IsClientConnected () && !this.networkManager.IsHost()) {
////
////				GUILayout.Space (10);
////
////				if( GUILayout.Button("End round", buttonStyle) ) {
////					this.mainNetworkScript.ExecuteCommandOnServer ("endround");
////				}
////				if( GUILayout.Button("Shutdown server", buttonStyle) ) {
////					this.mainNetworkScript.ExecuteCommandOnServer ("exit");
////				}
////
////				GUILayout.Space (10);
////
////			}
//			#endif
//
//			// Display button for openning options menu.
////			if (GUILayout.Button ("Options", buttonStyle)) {
////				
////				this.OpenMenu (MenuType.OptionsMenu);
////
////				CVarManager.ReadCVarsFromPlayerPrefs ();
////			}
//
//
//			// Display exit button.
////			if (GUILayout.Button ("Exit", buttonStyle)) {
////				this.ExitApplication();
////			}
//
//
//			GUILayout.EndScrollView ();
//
//			GUILayout.FlexibleSpace ();
//
//			GUILayout.EndArea ();
//
//
//			#if UNITY_ANDROID
//
//			{
//				// draw button for opening/closing console on android
//				Vector2 size = GUI.skin.button.CalcScreenSize( GUI.skin.button.CalcSize( new GUIContent("Console") ) );
//				Vector2 pos = new Vector2( Screen.width - size.x - 3, 3 );
//				if (GUI.Button (new Rect (pos, size), "Console")) {
//					this.isConsoleOpened = !this.isConsoleOpened;
//				}
//			}
//
//			#endif
//
//
//		}


		public	static	bool	CanGameObjectsReadUserInput() {

//			var components = singleton.GetComponentsInChildren<IForbidUserInput> ();
//			foreach (var c in components) {
//				if (!c.CanGameObjectsReadInput ()) {
//					return false;
//				}
//			}
//
//			return true;

			return m_forbidInputHandlers.TrueForAll( f => f.CanGameObjectsReadInput() );
		}

		public	static	bool	CanGameObjectsDrawGui() {

//			var components = singleton.GetComponentsInChildren<IForbidGuiDrawing> ();
//			foreach (var c in components) {
//				if (!c.CanGameObjectsDrawGui ()) {
//					return false;
//				}
//			}
//
//			return true;

			return m_forbidGuiDrawingHandlers.TrueForAll (f => f.CanGameObjectsDrawGui());

		}

		public	static	void	RegisterInputForbidHandler( IForbidUserInput forbidder ) {

			m_forbidInputHandlers.Add (forbidder);

		}

		public	static	void	RegisterGuiDrawingForbidHandler( IForbidGuiDrawing forbidder ) {

			m_forbidGuiDrawingHandlers.Add (forbidder);

		}

		
		public	void	Log( string s ) {
			
			Debug.Log (s);

		}

		public	void	LogWarning( string s ) {
			
			Debug.LogWarning (s);

		}
		
		public	void	LogError( string s ) {
			
			Debug.LogError (s);

		}


		public	void	HideAndLockMouse() {

			Cursor.visible = false;
		//	Cursor.lockState = CursorLockMode.Locked;

		}

		public	void	ShowAndUnlockMouse() {

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

		}

//		// Hides/shows and locks/unlocks mouse based on opened menus, windows, etc.
//		public	void	UpdateMouseLockAndVisibilityState() {
//
//			if (this.openedMenuType == MenuType.InGame) {
//				
//				bool shouldHideMouse = true;
//
//				foreach (WindowInfo wi in this.openedWindows) {
//					if (wi.isClosed)
//						continue;
//					
//					if (wi.type == WindowType.ChooseTeamWindow) {
//						shouldHideMouse = false;
//						break;
//					}
//				}
//
//				if (shouldHideMouse) {
//					// check if we are spectating
//				//	if (NetworkStatus.IsClientConnected ()
//				//		&& this.mainNetworkScript != null && this.mainNetworkScript.IsSpectating() ) {
//				//		shouldHideMouse = false;
//				//	}
//				}
//
//				if (shouldHideMouse) {
//					this.HideAndLockMouse ();
//				} else {
//					this.ShowAndUnlockMouse ();
//				}
//
//			} else {
//
//				this.ShowAndUnlockMouse ();
//
//			}
//
//		}

		public	void	ExitApplication() {

			#if UNITY_EDITOR

			UnityEditor.EditorApplication.isPlaying = false ;

			#else

			if(Application.isEditor) {
				// exit play mode using reflection

				var asm = Utilities.Utilities.GetEditorAssembly();
				var type = asm.GetType( "UnityEditor.EditorApplication" );
				var prop = type.GetProperty("isPlaying");
				prop.SetValue( null, (object) false, null );

			} else {

				Application.Quit();
			}

			#endif

		}


		private	void	OnServerStarted() {

			Debug.Log ("Server started.");

		}

		private	void	OnServerStopped() {

			Debug.Log ("Server stopped.");

		}

		private	void	OnClientConnected() {

			Debug.Log ("Client connected.");

		}

		/*
		// Functions is called from MyNetworkManager.
		public	void	OnClientDisconnect( int reason ) {

			if (1 == reason) {
				// Failed to connect to server.

				// Display and log message.
				string s = "Failed to connect to server." ;
				this.Log ( s );
				this.AddMessageBox( s, false );

			} else if (2 == reason) {
				// User disconnnected manually, or client disconnected manually after a command from server.

				// Don't display anything, just log.

				this.Log ("Disconnected from server.");

			} else if( 3 == reason ) {
				// Connection lost, or server disconnected us, etc.

				// Don't display any message, because we don't know if the server have already sent
				// us disconnection message, or is the connection lost.
				// Just log some info (in case connection was lost).

				this.Log ("Disconnected from server.");
			}


			if (this.openedMenuType == MenuType.InGame) {
				this.OpenMenu (MenuType.MainMenu);
			}

			this.chatMessagesQueue.Clear ();


		}
		*/

		private	void	OnClientDisconnected() {

			Debug.Log ("Client disconnected.");

		}


		public	void	SetMaximumFps( int maxFps, bool changeFixedDeltaTime ) {

			Application.targetFrameRate = maxFps;

			if (changeFixedDeltaTime) {
				Time.fixedDeltaTime = 1.0f / maxFps;
			}

		}

		public	static	float	GetAverageFps() {
			return singleton.averageFps;
		}

		

		
//		[System.NonSerialized]	public	int	lastSpawnPositionIndex = 0 ;

	//	private	MenuType	OpenedMenuType = MenuType.MainMenu ;
	//	public	MenuType	openedMenuType { get { return this.OpenedMenuType; } private set { this.OpenedMenuType = value ; } }
	//	private	static	string	openedMenuStringPath = MenuType.MainMenu.ToString ();

		private float averageFps = 0f ;
		private int secondsToUpdateFps = 1 ;
		private float lastTimeFpsUpdated = 0 ;
		private float fpsSum = 0f ;
		private int fpsSumCount = 0 ;
		private	System.Diagnostics.Stopwatch	fpsStopwatch = new System.Diagnostics.Stopwatch();

//		[System.NonSerialized]
//		public	NetworkManager	networkManager = null ;
//		// This should be set from main network script once it is created.
//		[System.NonSerialized]
//		public	Player	mainNetworkScript = null ;

//		[HideInInspector]
//		public	float	mouseSensitivityX = 500 ;
//		[HideInInspector]
//		public	float	mouseSensitivityY = 500 ;

		[HideInInspector]	public	float	minAccelerometerVerticalValue = 0.3f ;
		[HideInInspector]	public	float	minAccelerometerHorizontalValue = 0.3f ;
		[HideInInspector]	public	float	accelerometerVerticalOffset = 0.3f ;


		private	float	timePassedSinceStartedConnectingToServer = 0 ;	// used for displaying progress

		private	static	List<IForbidUserInput>	m_forbidInputHandlers = new List<IForbidUserInput> ();
		private	static	List<IForbidGuiDrawing>	m_forbidGuiDrawingHandlers = new List<IForbidGuiDrawing> ();


		public	static	GameManager	singleton { get ; private set ; }


	}



}


