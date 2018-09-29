using UnityEngine;

namespace uGameCore.Chat {

	/// <summary>
	/// Used to populate scroll view content with chat messages.
	/// </summary>
	public class ChatArea : MonoBehaviour {
		
		public	Utilities.PopulateScrollViewWithEvents populator = null;


		void Start () {

			if (null == this.populator)
				return;

			NetworkEventsDispatcher.onClientDisconnected += () => this.populator.RemoveAllEventsFromUI() ;
			NetworkEventsDispatcher.onServerStopped += () => this.populator.RemoveAllEventsFromUI ();
			ChatManager.onChatMessage += (ChatMessage chatMsg) => this.populator.EventHappened(
				"<color=blue>" + chatMsg.sender + "</color> : " + chatMsg.msg) ;
			
		}


	}

}
