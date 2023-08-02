using UnityEngine;

namespace uGameCore.Menu.Windows {

	public class DisplayMsgBoxWhenFailedToJoinGame : MonoBehaviour
	{

		public	string	title = "Failed to join game";
		public	int		width = 400;
		public	int		height = 300;



		private void OnFailedToJoinGame( Utilities.FailedToJoinGameMessage message ) {

			string text = "";
			if (message.Exception != null)
				text = message.Exception.Message;

			WindowManager.OpenMessageBox ( this.title, text, this.width, this.height );

		}

	}

}
