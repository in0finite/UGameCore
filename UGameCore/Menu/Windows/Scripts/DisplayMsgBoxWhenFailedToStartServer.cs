using UnityEngine;

namespace uGameCore.Menu.Windows {
	
	public class DisplayMsgBoxWhenFailedToStartServer : MonoBehaviour
	{

		public	string	title = "Failed to start server";
		public	int		width = 400;
		public	int		height = 300;



		private void OnFailedToStartServer( Utilities.FailedToStartServerMessage message ) {

			string text = "";
			if (message.Exception != null)
				text = message.Exception.Message;

			WindowManager.OpenMessageBox ( this.title, text, this.width, this.height );

		}

	}

}
