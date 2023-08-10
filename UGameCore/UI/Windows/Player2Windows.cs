namespace UGameCore.UI.Windows
{

    public class Player2Windows : NetworkBehaviour {


		void OnDisconnectedByServer( string description ) {

			WindowManager.OpenMessageBox (description, true);

		}

		public	void	DisplayMsgBoxOnClient( string title, string text ) {

			if (!this.isServer)
				return;

			this.TargetDisplayMsgBox (this.connectionToClient, title, text);

		}

		[ClientTargetRpc]
		private	void	TargetDisplayMsgBox( NetworkConnection netConn, string title, string text ) {

			var msgBox = WindowManager.OpenMessageBox (text, false);
			if (msgBox)
				msgBox.Title = title;

		}

	}

}
