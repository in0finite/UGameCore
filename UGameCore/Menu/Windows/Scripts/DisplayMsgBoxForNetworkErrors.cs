using UnityEngine;
using System.Collections.Generic;

namespace uGameCore.Menu.Windows {

	/// <summary>
	/// Detects network errors based on logs, and displays message box.
	/// </summary>
	public class DisplayMsgBoxForNetworkErrors : MonoBehaviour
	{

		public	List<string>	exactErrors = new List<string>();
		public	List<string>	errorsThatStartWith = new List<string>();
		public	string	msgBoxTitle = "Network error" ;



		void OnEnable() {

			Application.logMessageReceived += LogHandler;

		}

		void OnDisable() {

			Application.logMessageReceived -= LogHandler;

		}

		void LogHandler(string logMessage, string stackTrace, LogType logType) {

			// check if this is error, exception or assert
			if (logType != LogType.Error && logType != LogType.Exception && logType != LogType.Assert) {
				return;
			}

			bool bShouldDisplay = false;

			if (this.exactErrors.Contains (logMessage)) {
				bShouldDisplay = true;
			} else if (this.errorsThatStartWith.Exists (s => logMessage.StartsWith (s))) {
				bShouldDisplay = true;
			}

			if (bShouldDisplay) {
				var msgBox = WindowManager.OpenMessageBox (logMessage, false);
				msgBox.Title = this.msgBoxTitle;
			}

		}


	}

}
