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
			public	string	text;
			public	string	stackTrace;
			public	LogType	logType;
			public	string	displayText;
			public ConsoleLogEntryComponent logEntryComponent;

			public LogMessage (string text, string stackTrace, LogType logType)
			{
				this.text = text;
				this.stackTrace = stackTrace;
				this.logType = logType;
				this.displayText = null;
				this.logEntryComponent = null;
            }
		}

		[System.Serializable]
		public class IgnoreMessageInfo {
			public string text = "";
			public bool ignoreAllLogTypes = false;
			public LogType logType = LogType.Log;
		}


		public		bool	IsOpened { get => this.consoleUIRoot.activeInHierarchy; set => this.consoleUIRoot.SetActive(value); }

		public bool IsDetailsAreaOpened { get; set; } = false;

        private bool m_forceUIUpdateNextFrame = false;

		[Tooltip("Key which is used to open/close console")]
        public	KeyCode	openKey = KeyCode.BackQuote ;

		public volatile int maxNumLogMessages = 100;
        public int maxNumPooledLogMessages = 100;

		public volatile int numLinesToDisplayForLogMessage = 2;

        private readonly	Utilities.ConcurrentQueue<LogMessage>	m_messagesArrivedThisFrame = new ConcurrentQueue<LogMessage>();
        
		private readonly Queue<LogMessage> m_logMessages = new Queue<LogMessage>();
        private static LogMessage[] s_logMessagesBuffer;

        private readonly Queue<ConsoleLogEntryComponent> m_pooledLogEntryComponents = new Queue<ConsoleLogEntryComponent>();

		public ConsoleLogEntryComponent SelectedLogEntry { get; private set; }

		public Color selectedLogEntryColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

		private readonly System.Diagnostics.Stopwatch m_stopwatch = System.Diagnostics.Stopwatch.StartNew();

        private		List<string>	m_history = new List<string> ();
        public IReadOnlyList<string> History => m_history;
        private		int		m_historyBrowserIndex = -1 ;

		public		event System.Action	onDrawStats = delegate {};

		public		event System.Action<string>	onTextSubmitted = delegate {};

		public List<IgnoreMessageInfo>	ignoreMessages = new List<IgnoreMessageInfo> ();
		public List<IgnoreMessageInfo>	ignoreMessagesThatStartWith = new List<IgnoreMessageInfo> ();

		public GameObject logEntryPrefab;

		public GameObject consoleUIRoot;
        public ScrollRect	consoleScrollView;
        public ScrollRect detailsScrollView;
        public Button	consoleSubmitButton;
        public InputField	consoleSubmitInputField;
        


        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleLogThreaded;

			m_forceUIUpdateNextFrame = true;
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleLogThreaded;
        }

        void Start () {

            this.EnsureSerializableReferencesAssigned();

            if (this.consoleSubmitInputField != null) {
				
				// detect enter
				this.consoleSubmitInputField.onSubmit.AddListener ((arg0) => {
					
					// submit
					SubmittedText( this.consoleSubmitInputField.text );

					// clear input field
					this.consoleSubmitInputField.text = "";

					// set focus to input field
					this.consoleSubmitInputField.Select ();
					this.consoleSubmitInputField.ActivateInputField ();
					
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
		void	HandleLogThreaded(string logStr, string stackTrace, LogType type) {

			if (this.maxNumLogMessages <= 0)
				return;

            // check if this message should be ignored
            if (ShouldMessageBeIgnored(logStr, type))
                return;

            // keep the buffer limited in size - we don't need more than this number.
            // this also prevents running out of memory.
            m_messagesArrivedThisFrame.DequeueUntilCountReaches(this.maxNumLogMessages);

			var logMessage = new LogMessage (logStr, stackTrace, type);
			double time = m_stopwatch.Elapsed.TotalSeconds;

            logMessage.displayText = GetDisplayText(logStr, time);

            m_messagesArrivedThisFrame.Enqueue(logMessage);
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

		string GetDisplayText(string logStr, double time)
		{
            int lastIndexOfNewLine = -1;

            for (int i = 0; i < this.numLinesToDisplayForLogMessage; i++)
            {
                int newLineIndex = logStr.IndexOf('\n', lastIndexOfNewLine + 1);
                if (newLineIndex < 0)
                {
					lastIndexOfNewLine = -1;
                    break;
                }

                // found new '\n' character
                lastIndexOfNewLine = newLineIndex;
            }

            if (lastIndexOfNewLine != -1)
            {
                logStr = logStr[..lastIndexOfNewLine];
            }

            return $"[{F.FormatElapsedTime(time)}] {logStr}";
        }

		public		void	ClearLog() {

			m_logMessages.Clear();
			m_forceUIUpdateNextFrame = true;

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
			
			if (!textToProcess.IsNullOrWhiteSpace()) {
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

				this.IsOpened = ! this.IsOpened;

			}

			// check for key events from input field
			if(this.consoleSubmitInputField != null) {
				
				if( this.consoleSubmitInputField.isFocused ) {
					
					if(Input.GetKeyDown(KeyCode.UpArrow)) {
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

			this.detailsScrollView.gameObject.SetActive(this.IsDetailsAreaOpened);

            if (m_forceUIUpdateNextFrame && this.IsOpened)
            {
				m_forceUIUpdateNextFrame = false;
                this.RebuildLogUI();
				this.ScrollToEnd();
            }

        }

		void UpdateLogMessages() {

			if (this.maxNumLogMessages <= 0)
			{
				m_messagesArrivedThisFrame.Clear();
                return;
			}

			s_logMessagesBuffer ??= new LogMessage[500];

            int numNewlyAdded = m_messagesArrivedThisFrame.DequeueToArray(
                s_logMessagesBuffer, 0, s_logMessagesBuffer.Length);

            if (0 == numNewlyAdded)
				return;

			for (int i = 0; i < numNewlyAdded; i++)
				m_logMessages.Enqueue(s_logMessagesBuffer[i]);
            
            // limit number of log messages

            while (m_logMessages.Count > this.maxNumLogMessages)
			{
                var logMessage = m_logMessages.Dequeue();
				ReleaseLogMessage(logMessage);
            }

			if (!this.IsOpened)
            {
                m_forceUIUpdateNextFrame = true;
                return;
            }

			// update UI

			if (m_forceUIUpdateNextFrame) // no need to update here, because it will be rebuilt
				return;

            for (int i = 0; i < numNewlyAdded; i++)
			{
				var logMessage = s_logMessagesBuffer[i];
				CreateUIForLogMessage(logMessage);
            }

			// scroll to the end
			this.Invoke (nameof(this.ScrollToEnd), 0.1f);

			System.Array.Clear(s_logMessagesBuffer, 0, numNewlyAdded);
        }

		void RebuildLogUI()
		{
			foreach (var logMessage in m_logMessages)
			{
				ReleaseLogMessage(logMessage);
			}

            foreach (var logMessage in m_logMessages)
            {
                CreateUIForLogMessage(logMessage);
            }

            System.Array.Clear(s_logMessagesBuffer, 0, s_logMessagesBuffer.Length); // release references
        }

		void ReleaseLogMessage(LogMessage logMessage)
		{
            if (null == logMessage.logEntryComponent)
                return;

            // try return to pool
            if (m_pooledLogEntryComponents.Count < this.maxNumPooledLogMessages)
            {
                m_pooledLogEntryComponents.Enqueue(logMessage.logEntryComponent);
                logMessage.logEntryComponent.gameObject.SetActive(false);
            }
            else
            {
                F.DestroyEvenInEditMode(logMessage.logEntryComponent.gameObject);
            }

            logMessage.logEntryComponent.LogMessage = null;
            logMessage.logEntryComponent = null;
        }

		void CreateUIForLogMessage(LogMessage logMessage)
		{
            // try take from pool
            if (!m_pooledLogEntryComponents.TryDequeue(out var logEntryComponent))
            {
                logEntryComponent = this.logEntryPrefab.InstantiateAsUIElement(this.consoleScrollView.content)
                    .GetComponentOrThrow<ConsoleLogEntryComponent>();

                logEntryComponent.eventsPickup.onPointerClick += (ev) => LogEntryOnPointerClick(logEntryComponent, ev);
            }

            logEntryComponent.transform.SetAsLastSibling();
            logEntryComponent.textComponent.text = logMessage.displayText;
			logEntryComponent.textComponent.color = logMessage.logType == LogType.Log ? Color.white : (logMessage.logType == LogType.Warning ? Color.yellow : Color.red);
			logEntryComponent.image.color = this.logEntryPrefab.GetComponentOrThrow<ConsoleLogEntryComponent>().image.color;
            logEntryComponent.gameObject.SetActive(true);
            logEntryComponent.LogMessage = logMessage;

            logMessage.logEntryComponent = logEntryComponent;
        }

        private void LogEntryOnPointerClick(
			ConsoleLogEntryComponent logEntryComponent, UnityEngine.EventSystems.PointerEventData eventData)
        {
			if (this.SelectedLogEntry == logEntryComponent)
				this.SetSelectedLogEntry(null); // deselect
			else
				this.SetSelectedLogEntry(logEntryComponent);
        }

		public void SetSelectedLogEntry(ConsoleLogEntryComponent logEntryComponent)
		{
			if (this.SelectedLogEntry == logEntryComponent)
				return;

            // restore color of previously selected entry
            if (this.SelectedLogEntry != null)
            {
                Color originalColor = this.logEntryPrefab.GetComponentOrThrow<ConsoleLogEntryComponent>().image.color;
                this.SelectedLogEntry.image.color = originalColor;
            }

            this.SelectedLogEntry = logEntryComponent;

			if (this.SelectedLogEntry != null)
				this.SelectedLogEntry.image.color = this.selectedLogEntryColor;

			// update details area
			this.IsDetailsAreaOpened = this.SelectedLogEntry != null;
			if (this.SelectedLogEntry != null)
			{
				var detailsInputField = this.detailsScrollView.content.GetComponentInChildrenOrThrow<InputField>();
				detailsInputField.text = GetDetailsText(this.SelectedLogEntry.LogMessage);
                // need to rebuild layout for InputField, because it's not done automatically
                LayoutRebuilder.MarkLayoutForRebuild(detailsInputField.GetRectTransform());
			}
        }

		string GetDetailsText(LogMessage logMessage)
		{
			return logMessage.text + "\n\n" + logMessage.stackTrace;
		}
    }
}
