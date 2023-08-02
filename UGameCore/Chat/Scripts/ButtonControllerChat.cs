using UnityEngine;

namespace uGameCore.Chat {

	public class ButtonControllerChat : MonoBehaviour {


		public	void	SendChatMessage( UnityEngine.UI.InputField inputField ) {

			if (!NetworkStatus.IsClient ()) {
				// this is not client (nor host), so we will send message as server
				ChatManager.SendChatMessageToAllPlayersAsServer (inputField.text);
			} else {
				// there is an active local player
				ChatManager.SendChatMessageToAllPlayersAsLocalPlayer( inputField.text );
			}

			// clear input field
			inputField.text = "";

		}

	}

}
