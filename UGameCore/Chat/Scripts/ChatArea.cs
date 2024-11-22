using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore.Chat
{
	/// <summary>
	/// Used to populate scroll view content with chat messages.
	/// </summary>
	public class ChatArea : MonoBehaviour
	{
		public PopulateScrollViewWithEvents populator;
        public bool LogToConsole = true;


        void Start()
		{
			this.EnsureSerializableReferencesAssigned();
		}

        public void AddChatMessage(string message, bool bEscape)
        {
            message ??= string.Empty;

            if (bEscape)
                message = this.EscapeString(message);

            if (this.LogToConsole)
                Debug.Log(message, this);

            this.populator.EventHappened(message);
        }

        string EscapeString(string str)
        {
            return UIExtensions.EscapeStringForTMP(str);
        }
    }
}
