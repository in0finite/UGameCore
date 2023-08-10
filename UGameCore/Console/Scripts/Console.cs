﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UGameCore.Utilities;
using Profiler = UnityEngine.Profiling.Profiler;
using System.Linq;
using UnityEngine.EventSystems;

namespace UGameCore.Menu
{
    public class Console : MonoBehaviour {

		public class LogMessage
		{
			public	string	text;
			public	string	stackTrace;
			public	LogType	logType;
			public	string	displayText;
			public double time;
			public ConsoleLogEntryComponent logEntryComponent;

			public LogMessage (string text, string stackTrace, LogType logType, double time)
			{
				this.text = text;
				this.stackTrace = stackTrace;
				this.logType = logType;
				this.displayText = null;
				this.time = time;
				this.logEntryComponent = null;
            }
		}

		[System.Serializable]
		public class IgnoreMessageInfo {
			public string text = "";
			public bool ignoreAllLogTypes = false;
			public LogType logType = LogType.Log;
		}


		public		bool	IsOpened
        {
            get => this.consoleUIRoot.activeInHierarchy;
			set
            {
                this.consoleUIRoot.SetActive(value);

				if (value)
                {
                    this.consoleSubmitInputField.Select();
                    this.consoleSubmitInputField.ActivateInputField();
                }

                // need to immediately rebuild layout, otherwise scrollbars may return to top, even if we assign their values
                if (value)
					LayoutRebuilder.ForceRebuildLayoutImmediate(this.consoleScrollView.GetRectTransform());
            }
        }

        public bool IsDetailsAreaOpened { get; set; } = false;

        private bool m_forceUIUpdateNextFrame = false;

        private float m_scrollToValue = 0f;
		public float[] scrollBarUpdateDelays = new float[] { 0.05f, 0.1f };

        [Tooltip("Auto open/close console when open key is pressed")]
        public bool autoOpenConsole = true;

        [Tooltip("Key which is used to open/close console")]
        public	KeyCode	openKey = KeyCode.BackQuote;

        [Tooltip("Key which is used to open/close console on mobile and console platforms")]
        public KeyCode openKeyMobileAndConsole = KeyCode.Menu;

        public volatile int maxNumLogMessages = 100;
        public int maxNumPooledLogMessages = 100;

		public volatile int numLinesToDisplayForLogMessage = 3;
        public volatile int maxCharsInLogMessage = 250;

        private readonly	Utilities.ConcurrentQueue<LogMessage>	m_threadedBuffer = new ConcurrentQueue<LogMessage>();
        
		private readonly Queue<LogMessage> m_logMessages = new Queue<LogMessage>();
        private static List<LogMessage> s_logMessagesBufferList = new List<LogMessage>();

		System.Text.StringBuilder m_displayTextStringBuilder = new System.Text.StringBuilder();

        private readonly Queue<ConsoleLogEntryComponent> m_pooledLogEntryComponents = new Queue<ConsoleLogEntryComponent>();

		public ConsoleLogEntryComponent SelectedLogEntry { get; private set; }

		public Color selectedLogEntryColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
		private Color m_originalLogEntryColor;

		private readonly System.Diagnostics.Stopwatch m_stopwatch = System.Diagnostics.Stopwatch.StartNew();

        private		List<string>	m_history = new List<string> ();
        public IReadOnlyList<string> History => m_history;
        private		int		m_historyBrowserIndex = -1 ;

		public		event System.Action	onDrawStats = delegate {};

		public List<IgnoreMessageInfo>	ignoreMessages = new List<IgnoreMessageInfo> ();
		public List<IgnoreMessageInfo>	ignoreMessagesThatStartWith = new List<IgnoreMessageInfo> ();

		public CommandManager commandManager;

		public GameObject logEntryPrefab;

		public GameObject consoleUIRoot;
        public ScrollRect	consoleScrollView;
        public ScrollRect detailsScrollView;
        public Button	consoleSubmitButton;
        public InputField	consoleSubmitInputField;
        


        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleLogThreaded;

			m_forceUIUpdateNextFrame = true; // not sure if needed, but just in case
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleLogThreaded;
        }

        void Start () {

            this.EnsureSerializableReferencesAssigned();

			m_originalLogEntryColor = this.logEntryPrefab.GetComponentOrThrow<ConsoleLogEntryComponent>().image.color;

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
            m_threadedBuffer.DequeueUntilCountReaches(this.maxNumLogMessages);

            double time = m_stopwatch.Elapsed.TotalSeconds;
            var logMessage = new LogMessage (logStr, stackTrace, type, time);
			
            m_threadedBuffer.Enqueue(logMessage);
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
			bool bLimited = false;
			int numCharsToTake = 0;

            m_displayTextStringBuilder.Clear();
			m_displayTextStringBuilder.Append("[");
            m_displayTextStringBuilder.Append(F.FormatElapsedTime(time));
            m_displayTextStringBuilder.Append("] ");

            // limit num characters

            if (logStr.Length > this.maxCharsInLogMessage)
            {
				bLimited = true;
				numCharsToTake = Mathf.Max(0, this.maxCharsInLogMessage);
            }
			else
			{
				numCharsToTake = logStr.Length;
            }

            // limit num lines

            int lastIndexOfNewLine = -1;

            for (int i = 0; i < this.numLinesToDisplayForLogMessage; i++)
            {
                int newLineIndex = logStr.IndexOf('\n', lastIndexOfNewLine + 1);
                if (newLineIndex < 0 || newLineIndex >= this.maxCharsInLogMessage)
                {
					lastIndexOfNewLine = -1;
                    break;
                }

                // found new '\n' character
                lastIndexOfNewLine = newLineIndex;
            }

            if (lastIndexOfNewLine != -1)
            {
				bLimited = true;
				numCharsToTake = Mathf.Min(numCharsToTake, lastIndexOfNewLine);
            }

			m_displayTextStringBuilder.Append(logStr, 0, numCharsToTake);

            if (bLimited)
                m_displayTextStringBuilder.Append("  ...");

            return m_displayTextStringBuilder.ToString();
        }

		public		void	ClearLog() {

			foreach (LogMessage logMessage in m_logMessages)
			{
				ReleaseLogMessage(logMessage);
			}

			m_logMessages.Clear();

			m_threadedBuffer.Clear();
			
			m_forceUIUpdateNextFrame = true;

			this.SetSelectedLogEntry(null);

		}

		private		void	ScrollToPredefined() {

			if (this.consoleScrollView != null) {
				this.consoleScrollView.verticalScrollbar.value = m_scrollToValue;
			}

		}

        private void ScrollToDelayed(float value)
        {
            if (this.consoleScrollView != null)
            {
				m_scrollToValue = value;
				foreach (float delay in this.scrollBarUpdateDelays)
					this.Invoke(nameof(this.ScrollToPredefined), delay);
            }
        }

        private		void	SubmittedText( string textToProcess ) {

			// log this text, process command

			Debug.Log ( "> " + textToProcess );

			this.HandleTextSubmitted(textToProcess);
			
			if (!textToProcess.IsNullOrWhiteSpace()) {
				// add this command to list of executed commands
				m_history.Add (textToProcess);
				if (m_history.Count > 100) {
					m_history.RemoveRange (0, 20);
				}
			}

			// reset history browsing
			m_historyBrowserIndex = -1;

		}

		void HandleTextSubmitted(string text)
		{
            // process it as a command

            var context = CreateCommandContext(text);
            if (null == context)
                return;

            var result = this.commandManager.ProcessCommand(context);

            if (result.response != null)
            {
                if (result.IsSuccess)
                    Debug.Log(result.response, this);
                else
                    Debug.LogError(result.response, this);
            }
        }

        void AutoComplete()
        {
			// auto-complete current command

			int caretPosition = this.consoleSubmitInputField.caretPosition;

            if (caretPosition <= 0)
                return;

			string text = this.consoleSubmitInputField.text;

            string textBeforeCaret = text[..caretPosition];

            var context = CreateCommandContext(textBeforeCaret);
            if (null == context)
                return;

            var possibleCompletions = new List<string>();
            this.commandManager.AutoCompleteCommand(context, out string exactCompletion, possibleCompletions);

            // log all possible completions
            if (possibleCompletions.Count > 0)
                Debug.Log(string.Join("\t\t", possibleCompletions), this);

            // assign the exact completion into the InputField, respecting the caret position

            if (exactCompletion == null)
                return;

            string textAfterCaret = text[caretPosition..];

            this.consoleSubmitInputField.text = exactCompletion + textAfterCaret;
            this.consoleSubmitInputField.caretPosition = exactCompletion.Length;

        }

        CommandManager.ProcessCommandContext CreateCommandContext(string text)
        {
            // Commands are always executed locally (ie. not sent to server).
            // The actual command callback can decide what to do based on network state, and potentially
            // send the command to server.

            if (text.IsNullOrWhiteSpace())
                return null;

            var player = Player.local;

            var context = new CommandManager.ProcessCommandContext
            {
                command = text,
                hasServerPermissions = player != null ? player.IsServerAdmin : true, // only give perms if offline or on dedicated server
                executor = player,
                lastTimeExecutedCommand = player != null ? player.LastTimeExecutedCommand : null,
            };

            return context;
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

			this.UpdateOpenClose();

			// check for key events from input field
			if (this.consoleSubmitInputField != null && this.consoleSubmitInputField.isFocused)
			{
				if (Input.GetKeyDown(KeyCode.UpArrow))
				{
					BrowseHistoryBackwards();
				}
				else if (Input.GetKeyDown(KeyCode.DownArrow))
				{
					BrowseHistoryForwards();
				}
				else if (Input.GetKeyDown(KeyCode.Tab))
				{
					this.AutoComplete();
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
				//this.ScrollToEnd();
            }

        }

        void UpdateOpenClose()
		{
			// open/close console

			if (!this.autoOpenConsole)
				return;

            KeyCode keyCode = Application.isMobilePlatform || Application.isConsolePlatform
                ? this.openKeyMobileAndConsole
                : this.openKey;

            if (Input.GetKeyDown(keyCode)
                && (!F.UIHasFocus()
                    || (this.consoleSubmitInputField.isFocused
                        && (this.consoleSubmitInputField.text.IsNullOrWhiteSpace() || this.consoleSubmitInputField.text[0] == (char)keyCode))))
            {
                this.IsOpened = !this.IsOpened;

                if (!this.IsOpened)
                {
                    this.consoleSubmitInputField.text = string.Empty; // open-key will remain in InputField if we don't set this
                    this.consoleSubmitInputField.DeactivateInputField();
                    if (EventSystem.current != null)
                        EventSystem.current.SetSelectedGameObject(null); // have to do this, otherwise InputField remains focused
                }
            }
        }

        void UpdateLogMessages() {

			if (this.maxNumLogMessages <= 0)
			{
				m_threadedBuffer.Clear();
                return;
			}

			// no need to update anything if Console is not opened
			// threaded handler will keep an eye on buffer size
            if (!this.IsOpened)
            {
                return;
            }

            s_logMessagesBufferList.Clear();

            int numNewlyAdded = m_threadedBuffer.DequeueToList(s_logMessagesBufferList, this.maxNumLogMessages + 10);

            if (0 == numNewlyAdded)
				return;

			foreach (var logMessage in s_logMessagesBufferList.TakeLast(Mathf.Min(numNewlyAdded, this.maxNumLogMessages)))
			{
				logMessage.displayText = GetDisplayText(logMessage.text, logMessage.time);
                m_logMessages.Enqueue(logMessage);
            }

            // limit number of log messages

            while (m_logMessages.Count > this.maxNumLogMessages)
			{
                var logMessage = m_logMessages.Dequeue();
				ReleaseLogMessage(logMessage);
            }

			// update UI

			if (m_forceUIUpdateNextFrame) // no need to update here, because it will be rebuilt
				return;

			foreach (var logMessage in s_logMessagesBufferList.TakeLast(Mathf.Min(numNewlyAdded, this.maxNumLogMessages)))
			{
                CreateUIForLogMessage(logMessage);
            }

			this.ScrollToDelayed(0f);

			s_logMessagesBufferList.Clear();
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

            s_logMessagesBufferList.Clear(); // release references
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
			ConsoleLogEntryComponent logEntryComponent = logMessage.logEntryComponent;

            // try take from pool
            if (null == logEntryComponent && !m_pooledLogEntryComponents.TryDequeue(out logEntryComponent))
            {
                logEntryComponent = this.logEntryPrefab.InstantiateAsUIElement(this.consoleScrollView.content)
                    .GetComponentOrThrow<ConsoleLogEntryComponent>();

                logEntryComponent.eventsPickup.onPointerClick += (ev) => LogEntryOnPointerClick(logEntryComponent, ev);
            }

            logEntryComponent.transform.SetAsLastSibling();
            logEntryComponent.textComponent.text = logMessage.displayText;
			logEntryComponent.textComponent.color = logMessage.logType == LogType.Log ? Color.white : (logMessage.logType == LogType.Warning ? Color.yellow : Color.red);
			logEntryComponent.image.color = m_originalLogEntryColor;
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
                this.SelectedLogEntry.image.color = m_originalLogEntryColor;
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
