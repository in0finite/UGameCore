using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore {

	/// <summary>
	/// Detects network status change, and invokes appropriate events.
	/// </summary>
	public class NetworkEventsDispatcher : MonoBehaviour {

	//	public	bool	broadcastMessages = true ;

		public	static	event	System.Action	onServerStarted = delegate() {} ;
		public	static	event	System.Action	onServerStopped = delegate() {} ;

		public	static	event	System.Action	onClientStartedConnecting = delegate() {} ;
		public	static	event	System.Action	onClientConnected = delegate() {} ;
		public	static	event	System.Action	onClientDisconnected = delegate() {} ;

		private	NetworkServerStatus	m_lastServerStatus ;
		private	NetworkClientStatus	m_lastClientStatus ;


		// Use this for initialization
		void Start () {
			
			m_lastServerStatus = NetworkStatus.serverStatus;
			m_lastClientStatus = NetworkStatus.clientStatus;

		}

		// Update is called once per frame
		void Update () {

			var newServerStatus = NetworkStatus.serverStatus;
			var newClientStatus = NetworkStatus.clientStatus;


			if (m_lastServerStatus != newServerStatus) {
				// server status changed

				if (newServerStatus == NetworkServerStatus.Started) {

					InvokeEvent (onServerStarted);

					this.Dispatch ("OnServerStarted");

				} else if (newServerStatus == NetworkServerStatus.Starting) {
					
					this.Dispatch ("OnServerStarting");

				} else if (newServerStatus == NetworkServerStatus.Stopped) {

					InvokeEvent (onServerStopped);

					this.Dispatch ("OnServerStopped");

				}

			}

			if (m_lastClientStatus != newClientStatus) {
				// client status changed

				if (newClientStatus == NetworkClientStatus.Connecting) {

					InvokeEvent (onClientStartedConnecting);

					this.Dispatch ("OnClientStartedConnecting");

				} else if (newClientStatus == NetworkClientStatus.Connected) {

					InvokeEvent (onClientConnected);

					this.Dispatch ("OnClientConnected");

				} else if (newClientStatus == NetworkClientStatus.Disconnected) {

					InvokeEvent (onClientDisconnected);

					this.Dispatch ("OnClientDisconnected");

				}

			}


			m_lastServerStatus = newServerStatus;
			m_lastClientStatus = newClientStatus;
		}

		private	void	Dispatch(string message) {

//			if (this.broadcastMessages) {
//				Debug.Log ("Dispatching " + message);
//				this.BroadcastMessage (message, SendMessageOptions.DontRequireReceiver);
//			}

		}

		private	static	void	InvokeEvent(System.MulticastDelegate ev) {

			Utilities.Utilities.InvokeEventExceptionSafe (ev);

		}

	}

}
