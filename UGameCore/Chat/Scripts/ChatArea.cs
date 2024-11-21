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


		void Start()
		{
			this.EnsureSerializableReferencesAssigned();
		}

        public void AddChatMessage(string message, bool bEscape)
        {
            if (bEscape)
                message = this.EscapeString(message);

            this.populator.EventHappened(message);
        }

        string EscapeString(string str)
        {
            return UIExtensions.EscapeStringForTMP(str);
        }
    }
}
