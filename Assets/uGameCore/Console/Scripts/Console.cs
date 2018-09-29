using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Profiling;

namespace uGameCore.Menu {
	
	public class Console : MonoBehaviour {

		public class LogMessage
		{
			public	string	text = null ;
			public	string	stackTrace = null;
			public	LogType	logType ;
			public	string	displayText = null;

			public LogMessage (string text, string stackTrace, LogType logType)
			{
				this.text = text;
				this.stackTrace = stackTrace;
				this.logType = logType;
			}
		}

		[System.Serializable]
		public class IgnoreMessageInfo {
			public string text = "";
			public bool ignoreAllLogTypes = false;
			public LogType logType = LogType.Log;
		}


		private	static	bool	m_isConsoleOpened = false ;
		public	static	bool	IsOpened { get { return m_isConsoleOpened; } set { m_isConsoleOpened = value; } }

		private	static	bool	m_wasOpenedLastFrame = false ;

		private	static	bool	m_shouldUpdateDisplayTextWhenConsoleIsOpened = false;

		/// <summary>Key which is used to open/close console.</summary>
		public	KeyCode	openKey = KeyCode.BackQuote ;

		private	static	string	m_logString = "" ;
	//	[SerializeField]	private	int		m_logMessagesBufferLength = 100 ;
		[SerializeField]	private	int		m_maxCharacterCount = 2000 ;
		private	static	LinkedList<LogMessage>	m_logMessages = new LinkedList<LogMessage>() ;
		private	static	System.Text.StringBuilder	m_stringBuilder = null;
		public	static	int		TotalLengthOfMessages { get { return m_stringBuilder.Length; } }

		private	static	LinkedList<LogMessage>	m_messagesArrivedThisFrame = new LinkedList<LogMessage>() ;
	//	private	static	int		m_totalLengthOfMessagesArrivedThisFrame = 0;

		private	static	Vector2		m_consoleScrollPosition = Vector2.zero ;

		private	static	string		m_consoleCommandText = "" ;

		private	static	List<string>	m_history = new List<string> ();
		public	static	IEnumerable<string>	History { get { return m_history; } }
		private	static	int		m_historyBrowserIndex = -1 ;

		public	static	Console	singleton { get ; private set ; }

		public	static	event System.Action	onDrawStats = delegate {};
	//	public	static	event System.Func<string>	onGetStats = delegate { return ""; };
		private	static	List<System.Func<string>>	m_getStatsSubscribers = new List<System.Func<string>>();
		public	static	event System.Action<string>	onTextSubmitted = delegate {};

		private	static	string	m_lastStatsString = "" ;

		public	List<IgnoreMessageInfo>	ignoreMessages = new List<IgnoreMessageInfo> ();
		public	List<IgnoreMessageInfo>	ignoreMessagesThatStartWith = new List<IgnoreMessageInfo> ();

		private	ConsoleCanvas	consoleCanvas = null;
		private	ScrollRect	consoleScrollView = null;
		private	InputField	consoleTextDisplay = null;
		private	Button	consoleSubmitButton = null;
		private	InputField	consoleSubmitInputField = null;
		private	Text	consoleStatsTextControl = null;



		Console() {

			// add some default ignored messages
			this.ignoreMessages.Add(new IgnoreMessageInfo() {text = "HandleTransform no gameObject", ignoreAllLogTypes = true} );
			this.ignoreMessagesThatStartWith.Add (new IgnoreMessageInfo () {text = "Did not find target for sync message for", ignoreAllLogTypes = true});

		}

		void Awake() {

			singleton = this;
			
			// initialize log buffer
			m_stringBuilder = new System.Text.StringBuilder(this.m_maxCharacterCount);

			// register log callback
			Application.logMessageReceived += HandleLog;

			// find UI elements
			this.FindUIElements();

			// register functions for displaying our stats
			RegisterStats( () => { return "FPS: " + GameManager.GetAverageFps() ; } );
			RegisterStats( () => { return "uptime: " + Utilities.Utilities.FormatElapsedTime( Time.realtimeSinceStartup ) ; } );

		}

		void Start () {

			if (this.consoleSubmitInputField != null) {
				
				// detect enter
				this.consoleSubmitInputField.onEndEdit.AddListener ((arg0) => {
					if(Input.GetKeyDown (KeyCode.Return)) {
						// submit
						SubmittedText( singleton.consoleSubmitInputField.text );

						// clear input field
						singleton.consoleSubmitInputField.text = "";

						// set focus to input field
						singleton.consoleSubmitInputField.Select ();
						singleton.consoleSubmitInputField.ActivateInputField ();
					}
				});

				// register submit button handler
				if (this.consoleSubmitButton != null) {
					this.consoleSubmitButton.onClick.AddListener( () =>
						{
							SubmittedText(this.consoleSubmitInputField.text);
							SetInputBoxText ("");
						} );
				}
			}

		}

		private void FindUIElements() {

			this.consoleCanvas = Utilities.Utilities.FindObjectOfTypeOrLogError<ConsoleCanvas>();
			var tr = this.consoleCanvas.transform;
			this.consoleScrollView = tr.GetComponentInChildren<ScrollRect> ();
			this.consoleTextDisplay = tr.FindChildRecursivelyOrLogError ("TextDisplay").GetComponent<InputField>();
			this.consoleSubmitButton = tr.FindChildRecursivelyOrLogError ("SubmitButton").GetComponent<Button>();
			this.consoleSubmitInputField = tr.FindChildRecursivelyOrLogError ("SubmitInputField").GetComponent<InputField>();
			this.consoleStatsTextControl = tr.FindChildRecursivelyOrLogError ("Stats").GetComponent<Text>();

		}

		/// <summary>
		/// Register statistics that will be displayed in console.
		/// </summary>
		public	static	void	RegisterStats( System.Func<string> getStatMethod ) {

			m_getStatsSubscribers.Add (getStatMethod);

		}

		// Callback function for unity logs.
		static	void	HandleLog(string logStr, string stackTrace, LogType type) {

			if (0 == singleton.m_maxCharacterCount)
				return;


			// check if this message should be ignored
			if (ShouldMessageBeIgnored (logStr, type))
				return;


			var logMessage = new LogMessage (logStr, stackTrace, type);
			m_messagesArrivedThisFrame.AddLast (logMessage);

		}

		public	static	bool	ShouldMessageBeIgnored(string logStr, LogType type) {

			foreach (var im in singleton.ignoreMessages) {
				if (im.text == logStr && ( im.ignoreAllLogTypes || im.logType == type )) {
					return true;
				}
			}

			foreach (var im in singleton.ignoreMessagesThatStartWith) {
				if (logStr.StartsWith(im.text) && ( im.ignoreAllLogTypes || im.logType == type )) {
					return true;
				}
			}

			return false;
		}

		public	static	string	GetRichText( LogMessage logMessage ) {
			
			if (logMessage.logType == LogType.Log) {

				return logMessage.text ;

			} else if (logMessage.logType == LogType.Warning) {

				return "<color=yellow>" + logMessage.text + "</color>" ;

			}

			return "<color=red>" + logMessage.text + "\n" + logMessage.stackTrace + "</color>" ;

		}


		public	static	void	ClearLog() {

			m_logMessages.Clear ();
			m_stringBuilder.Length = 0;
			m_logString = "";

			UpdateDisplayText ();

		}

		private	static	void	UpdateDisplayText() {

			if (singleton.consoleTextDisplay != null) {
				singleton.consoleTextDisplay.text = m_stringBuilder.ToString ();
				LayoutRebuilder.MarkLayoutForRebuild (singleton.consoleTextDisplay.GetComponent<RectTransform> ());
			}

		}

		private	static	void	ScrollToEnd() {

			if (singleton.consoleScrollView != null) {
				singleton.consoleScrollView.verticalScrollbar.value = 0f;
			}

		}

		private	void	ScrollToEndNonStatic() {

			ScrollToEnd ();

		}

		private	static	void	SubmittedText( string textToProcess ) {

			// log this text, process command

			Debug.Log ( "> " + textToProcess );

			try {
				// TODO: should we invoke all subscribers exception safe ?
				onTextSubmitted (textToProcess);
			} catch( System.Exception ex ) {
				Debug.LogException (ex);
			}

			if (textToProcess.Length > 0) {
				// add this command to list of executed commands
				m_history.Add (textToProcess);
				if (m_history.Count > 100) {
					m_history.RemoveAt (0);
				}
			}

			// reset history browsing
			m_historyBrowserIndex = -1;

		}

		public	static	void	SetInputBoxText( string text ) {

			m_consoleCommandText = text;

			if (singleton.consoleSubmitInputField != null) {
				singleton.consoleSubmitInputField.text = text;
			}
			
		}

		public	static	void	BrowseHistoryBackwards() {

			if (m_history.Count > 0) {
				if (-1 == m_historyBrowserIndex)
					m_historyBrowserIndex = m_history.Count - 1;
				else
					m_historyBrowserIndex--;

				if (m_historyBrowserIndex < 0)
					m_historyBrowserIndex = 0;

				if (m_historyBrowserIndex < m_history.Count)
					SetInputBoxText (m_history [m_historyBrowserIndex]);
			}

		}

		public	static	void	BrowseHistoryForwards() {

			if (m_history.Count > 0) {
				if (-1 != m_historyBrowserIndex) {
					m_historyBrowserIndex++;
					if (m_historyBrowserIndex >= m_history.Count)
						m_historyBrowserIndex = m_history.Count - 1;
					SetInputBoxText (m_history [m_historyBrowserIndex]);
				}
			}

		}


		void Update () {


			// open/close console

			#if UNITY_ANDROID
			if (Input.GetKeyDown (KeyCode.Menu)) {
			#else
			if (Input.GetKeyDown (singleton.openKey)) {
			#endif

				m_isConsoleOpened = ! m_isConsoleOpened;

			}


			// enable/disable canvas
			Profiler.BeginSample( "SetCanvasEnabledState", this );
			if (m_wasOpenedLastFrame != m_isConsoleOpened) {
				// opened state changed

				m_wasOpenedLastFrame = m_isConsoleOpened ;

				if(this.consoleCanvas != null) {
					var canvas = this.consoleCanvas.GetComponentInChildren<Canvas>(true);
					if(canvas != null) {
						canvas.gameObject.BroadcastMessageNoExceptions("OnPreCanvasStateChanged", m_isConsoleOpened);
						canvas.enabled = m_isConsoleOpened ;
						canvas.gameObject.BroadcastMessageNoExceptions("OnPostCanvasStateChanged", m_isConsoleOpened);
					}
				}

				if(m_isConsoleOpened) {
					// update display text if something was logged in the meantime
					if(m_shouldUpdateDisplayTextWhenConsoleIsOpened) {
						
						m_shouldUpdateDisplayTextWhenConsoleIsOpened = false ;

						UpdateDisplayText();

						// also scroll to end
						ScrollToEnd();
					}
				}

			}
			Profiler.EndSample();

			// check for key events from input field
			if(this.consoleSubmitInputField != null) {
				
				if( this.consoleSubmitInputField.isFocused ) {
					
					if(Input.GetKeyDown(KeyCode.Return)) {
						// enter pressed
						// submit
					//	SubmittedText( this.consoleSubmitInputField.text );
					//	SetInputBoxText("");

					} else if(Input.GetKeyDown(KeyCode.UpArrow)) {
						// browse history backwards
						BrowseHistoryBackwards();

					} else if(Input.GetKeyDown(KeyCode.DownArrow)) {
						// browse history forwards
						BrowseHistoryForwards();
					}
				}
			}

			// update stats
			Profiler.BeginSample( "UpdateStats", this );
			if(m_isConsoleOpened) {
				
				string text = "" ;
				foreach( var method in m_getStatsSubscribers ) {
					string s = method.Invoke();
					if (s.Length > 0)
						text += s + "    ";
				}

				if(m_lastStatsString != text) {
					// stats text changed
					// update UI
					if(consoleStatsTextControl != null) {
						consoleStatsTextControl.text = text ;
					}

					m_lastStatsString = text ;
				}
			}
			Profiler.EndSample();


			Profiler.BeginSample( "UpdateLogMessages", this );
			this.UpdateLogMessages();
			Profiler.EndSample();


		}

		void UpdateLogMessages() {

			if (0 == m_messagesArrivedThisFrame.Count)
				return;


			// loop through newly arrived messages
			// find their total length
			// if needed, remove old messages, and recompute text for display
			// otherwise, just add new messages

			int lengthOfNewMessages = 0;
			foreach(var msg in m_messagesArrivedThisFrame) {
				msg.displayText = GetRichText (msg) + "\n";
				lengthOfNewMessages += msg.displayText.Length ;
			}

			int newLength = TotalLengthOfMessages + lengthOfNewMessages;

			if (newLength > m_maxCharacterCount) {
				// need to remove old messages

				int lengthToRemove = newLength - m_maxCharacterCount ;
				int lengthRemoved = 0;

				while (lengthRemoved < lengthToRemove && m_logMessages.Count != 0) {
					lengthRemoved += m_logMessages.First.Value.displayText.Length ;

					m_logMessages.RemoveFirst ();
				}

				// removed enough messages

			//	TotalLengthOfMessages -= lengthRemoved;

			//	m_logString = m_logString.Remove (0, lengthRemoved);
				m_stringBuilder.Remove( 0, lengthRemoved );

			} else {
				// no need to remove messages
				// just add new messages

			}

			// add new messages

			while (m_messagesArrivedThisFrame.Count != 0) {
				m_logMessages.AddLast (m_messagesArrivedThisFrame.First.Value);

				m_stringBuilder.Append (m_messagesArrivedThisFrame.First.Value.displayText);

				m_messagesArrivedThisFrame.RemoveFirst ();
			}

			// cache string
		//	m_logString = m_stringBuilder.ToString ();

		//	TotalLengthOfMessages += lengthOfNewMessages;

			// update text display

			if (m_isConsoleOpened) {	// only update it if console is opened
				
				UpdateDisplayText ();

			} else {
				// update display text later
				m_shouldUpdateDisplayTextWhenConsoleIsOpened = true ;
			}

			// scroll to the end

			m_consoleScrollPosition.y = Mathf.Infinity;

			if (m_isConsoleOpened) {
				singleton.Invoke ("ScrollToEndNonStatic", 0.1f);
			} else {
				// it will be scrolled later
			}


		}


		void OnGUI() {

			if (m_isConsoleOpened) {
				// Draw console.
			//	DrawConsole ();
			}

		}

		static	void	DrawConsole() {


			int consoleWidth = Screen.width;
			int consoleHeight = Screen.height / 2;

			// Draw rectangle in background.
			GUI.Box( new Rect(0, 0, consoleWidth, consoleHeight) , "");


			// Draw some statistics above console

			GUILayout.BeginArea (new Rect (0, 0, Screen.width, 30));
			GUILayout.BeginHorizontal ();

			// display fps
			GUILayout.Label( "FPS: " + GameManager.GetAverageFps() );

			// display uptime
			if (NetworkStatus.IsServerStarted ()) {
				GUILayout.Label (" uptime: " + Utilities.Utilities.FormatElapsedTime( Time.realtimeSinceStartup ) );
			}

			// let anybody else display their own stats
			try {
				onDrawStats();
			} catch (System.Exception ex) {
				Debug.LogException (ex);
			}

			// Display network statistics for client
			if( NetworkStatus.IsClientConnected () && !NetworkStatus.IsHost() ) {
				NetworkConnection conn = NetManager.GetClient ().connection;
				byte error = 0;
				GUILayout.Label( " ping: " + NetworkTransport.GetCurrentRtt (conn.hostId, conn.connectionId, out error) + " ms" );
				GUILayout.Label( " lost_packets: " + NetworkTransport.GetNetworkLostPacketNum (conn.hostId, conn.connectionId, out error) );
				GUILayout.Label( " received_rate: " + NetworkTransport.GetPacketReceivedRate (conn.hostId, conn.connectionId, out error) );
				GUILayout.Label( " sent_rate: " + NetworkTransport.GetPacketSentRate (conn.hostId, conn.connectionId, out error) );
			}

			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();


			m_consoleScrollPosition = GUILayout.BeginScrollView (
				m_consoleScrollPosition, GUILayout.Width (consoleWidth), GUILayout.Height (consoleHeight) );


			/*
			// Display player information
			if( networkManager.IsServer() ) {

				GUILayout.Label("Players");
				GUILayout.Label("name\t\t | health\t | kills\t | deaths\t | ping");
				foreach ( Player player in networkManager.players ) {

					string s = "";
					s += player.playerName + "\t ";
					if (player.mainNetworkScript != null) {
						s += player.mainNetworkScript.health + "\t " + player.mainNetworkScript.numKills + "\t " + player.mainNetworkScript.numDeaths ;
					}
					s += "\t 0 ms";

					GUILayout.Label( s );
				}
			}
			*/

			// Draw log string
			//	if( logString != "" ) {
			{
				/*
				GUIStyle style = new GUIStyle( GUI.skin.textArea );
			//	style.wordWrap = true;
				style.richText = true ;
				GUILayout.Space(25);
				GUILayout.TextArea( logString, style, GUILayout.MinHeight (consoleHeight - 30) );
				*/

				GUILayout.Space(30);
				GUILayout.Label (m_logString);
			}

			GUILayout.EndScrollView ();


			GUILayout.BeginHorizontal ();

			string textToProcess = "";

			// Edit box for commands input.
			GUI.SetNextControlName("commands_input");
			m_consoleCommandText = GUILayout.TextField( m_consoleCommandText, 1000, GUILayout.Width( Screen.width / 4 ), GUILayout.Height( 40 ) );
			if (Event.current.isKey && GUI.GetNameOfFocusedControl () == "commands_input") {
				if (Event.current.keyCode == KeyCode.UpArrow) {
					// up arrow pressed and edit box is in focus
					BrowseHistoryBackwards();

				} else if (Event.current.keyCode == KeyCode.DownArrow) {
					// down arrow pressed and edit box is in focus
					BrowseHistoryForwards();

				} else if (Event.current.keyCode == KeyCode.Return) {
					// enter pressed
					textToProcess = m_consoleCommandText ;
				}
			}

			// submit button
			//	bool submited = GUILayout.Button( "Submit", GUILayout.Width(60), GUILayout.Height(40) );
			bool submited = GameManager.DrawButtonWithCalculatedSize("Submit");
			if (submited) {
				textToProcess = m_consoleCommandText;
			}

			GUILayout.EndHorizontal ();


			if (textToProcess != "") {

				SubmittedText (textToProcess);

				// clear input text box
				SetInputBoxText("");
			}


		}

	}

}
