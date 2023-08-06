using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UGameCore.Utilities;
using Profiler = UnityEngine.Profiling.Profiler;

namespace UGameCore.Menu
{

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


		private		bool	m_isConsoleOpened = false ;
		public		bool	IsOpened { get { return m_isConsoleOpened; } set { m_isConsoleOpened = value; } }

		private		bool	m_wasOpenedLastFrame = false ;

		private		bool	m_shouldUpdateDisplayTextWhenConsoleIsOpened = false;

		/// <summary>Key which is used to open/close console.</summary>
		public	KeyCode	openKey = KeyCode.BackQuote ;

		[SerializeField]	private	int		m_maxCharacterCount = 2000 ;
		private		LinkedList<LogMessage>	m_logMessages = new LinkedList<LogMessage>() ;
		private		System.Text.StringBuilder	m_stringBuilder = null;
		public		int		TotalLengthOfMessages { get { return m_stringBuilder.Length; } }

		private		LinkedList<LogMessage>	m_messagesArrivedThisFrame = new LinkedList<LogMessage>() ;
	//	private		int		m_totalLengthOfMessagesArrivedThisFrame = 0;

		private		Vector2		m_consoleScrollPosition = Vector2.zero ;

		private		string		m_consoleCommandText = "" ;

		private		List<string>	m_history = new List<string> ();
		public		IReadOnlyList<string>	History { get { return m_history; } }
		private		int		m_historyBrowserIndex = -1 ;

		public		event System.Action	onDrawStats = delegate {};

		public		event System.Action<string>	onTextSubmitted = delegate {};

		public	List<IgnoreMessageInfo>	ignoreMessages = new List<IgnoreMessageInfo> ();
		public	List<IgnoreMessageInfo>	ignoreMessagesThatStartWith = new List<IgnoreMessageInfo> ();

        public ScrollRect	consoleScrollView = null;
        public InputField	consoleTextDisplay = null;
        public Button	consoleSubmitButton = null;
        public InputField	consoleSubmitInputField = null;
        


		void Awake() {

			this.EnsureSerializableReferencesAssigned();

			// initialize log buffer
			m_stringBuilder = new System.Text.StringBuilder(this.m_maxCharacterCount);

			// register functions for displaying our stats
			RegisterStats( () => { return "FPS: " + GameManager.GetAverageFps() ; } );
			RegisterStats( () => { return "uptime: " + Utilities.Utilities.FormatElapsedTime( Time.realtimeSinceStartup ) ; } );

		}

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void Start () {

			if (this.consoleSubmitInputField != null) {
				
				// detect enter
				this.consoleSubmitInputField.onEndEdit.AddListener ((arg0) => {
					if(Input.GetKeyDown (KeyCode.Return)) {
						// submit
						SubmittedText( this.consoleSubmitInputField.text );

						// clear input field
						this.consoleSubmitInputField.text = "";

						// set focus to input field
						this.consoleSubmitInputField.Select ();
						this.consoleSubmitInputField.ActivateInputField ();
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

		public		void	RegisterStats( System.Func<string> getStatMethod ) {

		}

		// Callback function for unity logs.
		void	HandleLog(string logStr, string stackTrace, LogType type) {

			if (0 == this.m_maxCharacterCount)
				return;


			// check if this message should be ignored
			if (ShouldMessageBeIgnored (logStr, type))
				return;


			var logMessage = new LogMessage (logStr, stackTrace, type);
			m_messagesArrivedThisFrame.AddLast (logMessage);

		}

		public		bool	ShouldMessageBeIgnored(string logStr, LogType type) {

			foreach (var im in this.ignoreMessages) {
				if (im.text == logStr && ( im.ignoreAllLogTypes || im.logType == type )) {
					return true;
				}
			}

			foreach (var im in this.ignoreMessagesThatStartWith) {
				if (logStr.StartsWith(im.text) && ( im.ignoreAllLogTypes || im.logType == type )) {
					return true;
				}
			}

			return false;
		}

		public		string	GetRichText( LogMessage logMessage ) {
			
			if (logMessage.logType == LogType.Log) {

				return logMessage.text ;

			} else if (logMessage.logType == LogType.Warning) {

				return "<color=yellow>" + logMessage.text + "</color>" ;

			}

			return "<color=red>" + logMessage.text + "\n" + logMessage.stackTrace + "</color>" ;

		}


		public		void	ClearLog() {

			m_logMessages.Clear ();
			m_stringBuilder.Length = 0;
			
			UpdateDisplayText ();

		}

		private		void	UpdateDisplayText() {

			if (this.consoleTextDisplay != null) {
				this.consoleTextDisplay.text = m_stringBuilder.ToString ();
				LayoutRebuilder.MarkLayoutForRebuild (this.consoleTextDisplay.GetComponent<RectTransform> ());
			}

		}

		private		void	ScrollToEnd() {

			if (this.consoleScrollView != null) {
				this.consoleScrollView.verticalScrollbar.value = 0f;
			}

		}

		private		void	SubmittedText( string textToProcess ) {

			// log this text, process command

			Debug.Log ( "> " + textToProcess );

			onTextSubmitted.InvokeEventExceptionSafe (textToProcess);
			
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

		public		void	SetInputBoxText( string text ) {

			m_consoleCommandText = text;

			if (this.consoleSubmitInputField != null) {
				this.consoleSubmitInputField.text = text;
			}
			
		}

		public		void	BrowseHistoryBackwards() {

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

		public		void	BrowseHistoryForwards() {

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
			if (Input.GetKeyDown (this.openKey)) {
			#endif

				m_isConsoleOpened = ! m_isConsoleOpened;

			}


			// enable/disable canvas
			Profiler.BeginSample( "SetCanvasEnabledState", this );
			if (m_wasOpenedLastFrame != m_isConsoleOpened) {
				// opened state changed

				m_wasOpenedLastFrame = m_isConsoleOpened ;

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
				this.Invoke (nameof(this.ScrollToEnd), 0.1f);
			} else {
				// it will be scrolled later
			}


		}
	}
}
