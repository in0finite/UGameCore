namespace UGameCore
{

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

        private static bool IsNetworkClientConnecting
        {
            get
            {
#if MIRROR
                return NetworkClient.active && !NetworkClient.isConnected;
#else
                return false;
#endif
            }
        }

        public static NetworkClientStatus clientStatus
        {
            get
            {
#if MIRROR
                if (NetworkClient.isConnected)
                    return NetworkClientStatus.Connected;

                if (IsNetworkClientConnecting)
                    return NetworkClientStatus.Connecting;

                return NetworkClientStatus.Disconnected;
#else
                return NetworkClientStatus.Disconnected;
#endif
            }
        }

        public static NetworkServerStatus serverStatus
        {
            get
            {
#if MIRROR
                if (!NetworkServer.active)
                    return NetworkServerStatus.Stopped;

                return NetworkServerStatus.Started;
#else
                return NetworkServerStatus.Started;
#endif
            }
        }

        public static bool IsServerStarted => NetworkStatus.serverStatus == NetworkServerStatus.Started;

        /// <summary>
        /// Is server active ?
        /// </summary>
        public static bool IsServer => NetworkStatus.IsServerStarted;

        /// <summary>
        /// Is host active ?
        /// </summary>
        public static bool IsHost()
        {
            return NetworkStatus.IsServer && NetworkStatus.IsClientConnected();
        }

        public static bool IsClientConnected()
        {

            return clientStatus == NetworkClientStatus.Connected;
        }

        public static bool IsClientConnecting()
        {

            return clientStatus == NetworkClientStatus.Connecting;
        }

        public static bool IsClientDisconnected()
        {

            return clientStatus == NetworkClientStatus.Disconnected;
        }

        /// <summary>
        /// Is client connected ?
        /// TODO: This method should be corrected to return: is client active.
        /// </summary>
        public static bool IsClient()
        {
            return NetworkStatus.IsClientConnected();
        }

        public static bool IsClientActive()
        {
            return !NetworkStatus.IsClientDisconnected();
        }

        public static bool IsClientOnly => !NetworkStatus.IsServer && NetworkStatus.IsClientActive();


        /// <summary>
        /// Throws exception if server is not active.
        /// </summary>
        public static void ThrowIfNotOnServer()
        {
            if (!NetworkStatus.IsServer)
                throw new System.Exception("Not on a server");
        }


    }

}
