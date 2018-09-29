using UnityEngine.Networking;

namespace uGameCore {

	public	enum NetworkClientStatus
	{
		Disconnected = 0,
		Connecting = 1,
		Connected

	}

	public	enum NetworkServerStatus
	{
		Started = 1,
		Starting = 2,
		Stopped = 3


	}

	public class NetworkStatus {

		private	static	System.Reflection.FieldInfo	m_networkClientStatusField = null;


		static NetworkStatus() {

			m_networkClientStatusField = typeof(NetworkClient).GetField( "m_AsyncConnect", System.Reflection.BindingFlags.Instance | 
				System.Reflection.BindingFlags.NonPublic );

		}


		private	static	bool	IsNetworkClientConnecting( NetworkClient client ) {

			object enumValue = m_networkClientStatusField.GetValue (client);
			string state = enumValue.ToString ();

			return state == "Resolving" || state == "Connecting";
		}


		public	static	NetworkClientStatus	clientStatus {
			get {
				if (!NetworkClient.active)
					return NetworkClientStatus.Disconnected;
				if (0 == NetworkClient.allClients.Count)
					return NetworkClientStatus.Disconnected;

//				if (!NetworkClient.allClients [0].isConnected)
//					return NetworkClientStatus.Connecting;

				// if there is at least 1 client connected, then status is connected
				if (NetworkClient.allClients.Exists (client => client.isConnected)) {
					return NetworkClientStatus.Connected;
				} else if(NetworkClient.allClients.Exists( client => IsNetworkClientConnecting(client) ) ) {
				//	// we have no way to know if client is still connecting (do we ?), but we will return this status
					return NetworkClientStatus.Connecting;
				}
				
				return NetworkClientStatus.Disconnected;
			}
		}

		public	static	NetworkServerStatus serverStatus {
			get {
				if (!NetworkServer.active)
					return NetworkServerStatus.Stopped;

				return NetworkServerStatus.Started;
			}
		}

		public	static	bool	IsServerStarted() {

			return serverStatus == NetworkServerStatus.Started;
		}

		/// <summary>
		/// Is server active ?
		/// </summary>
		public	static	bool	IsServer() {
			return NetworkStatus.IsServerStarted ();
		}

		/// <summary>
		/// Is host active ?
		/// </summary>
		public	static	bool	IsHost() {

			if (!NetworkStatus.IsServer ())
				return false;

			return NetworkServer.localClientActive;
		}

		public	static	bool	IsClientConnected() {

			return clientStatus == NetworkClientStatus.Connected;
		}

		public	static	bool	IsClientConnecting() {

			return clientStatus == NetworkClientStatus.Connecting;
		}

		public	static	bool	IsClientDisconnected() {

			return clientStatus == NetworkClientStatus.Disconnected;
		}

		/// <summary>
		/// Is client connected ?
		/// TODO: This method should be corrected to return: is client active.
		/// </summary>
		public	static	bool	IsClient() {
			return NetworkStatus.IsClientConnected();
		}

		public	static	bool	IsClientActive() {
			return ! NetworkStatus.IsClientDisconnected ();
		}


	}

}
