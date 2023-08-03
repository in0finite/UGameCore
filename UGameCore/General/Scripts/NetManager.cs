using System;
using System.Collections.Generic;
using UnityEngine;
#if MIRROR
using Mirror;
#endif
using UGameCore.Utilities;

namespace UGameCore.Net
{

    public class NetManager : MonoBehaviour
    {

        public static int defaultListenPortNumber { get { return 7777; } }

#if MIRROR
        public static int listenPortNumber => Transport.activeTransport switch
        {
            TelepathyTransport telepathyTransport => telepathyTransport.port,
            kcp2k.KcpTransport kcpTransport => kcpTransport.Port,
            _ => throw new NotSupportedException("Can not obtain port number from current transport"),
        };
#else
        public static int listenPortNumber { get; set; }
#endif

        public static bool dontListen
#if MIRROR
        { get { return NetworkServer.dontListen; } set { NetworkServer.dontListen = value; } }
#else
        { get; set; }
#endif

        public static int maxNumPlayers
#if MIRROR
        { get => NetworkManager.singleton.maxConnections; set { NetworkManager.singleton.maxConnections = value; } }
#else
        { get; set; }
#endif

        public static int numConnections =>
#if MIRROR
            NetworkServer.connections.Count;
#else
            0;
#endif

        public static string onlineScene
        {
#if MIRROR
            get
            {
                return NetworkManager.singleton.onlineScene;
            }
            set
            {
                NetworkManager.singleton.onlineScene = value;
            }
#else
            get; set;
#endif
        }

        public static NetManager Instance { get; private set; }

        NetworkClientStatus m_lastClientStatus = NetworkClientStatus.Disconnected;
        public event System.Action onClientStatusChanged = delegate { };

        private NetworkServerStatus m_lastServerStatus = NetworkServerStatus.Stopped;
        public event System.Action onServerStatusChanged = delegate { };


        private static readonly IReadOnlyDictionary<uint, NetworkIdentity> s_emptySpawnedDictionary =
            new Dictionary<uint, NetworkIdentity>();
        private static IReadOnlyDictionary<uint, NetworkIdentity> SpawnedDictionary
        {
            get
            {
#if MIRROR
                if (NetworkServer.active)
                {
                    return NetworkServer.spawned;
                }
                else if (NetworkClient.active)
                {
                    return NetworkClient.spawned;
                }
#endif

                return s_emptySpawnedDictionary;
            }
        }

        public static int NumSpawnedNetworkObjects => SpawnedDictionary.Count;

        public static double NetworkTime =>
#if MIRROR
            Mirror.NetworkTime.time;
#else
            0;
#endif



        NetManager()
        {
            // assign implementation in NetUtils
            // do this in ctor, because it may be too late in Awake() - server can theoretically start before our Awake() is called
            NetUtils.IsServerImpl = () => NetworkStatus.IsServer;
        }


        void Awake()
        {
            if (null == Instance)
                Instance = this;
        }

        void Update()
        {

            NetworkClientStatus clientStatusNow = NetworkStatus.clientStatus;
            if (clientStatusNow != m_lastClientStatus)
            {
                m_lastClientStatus = clientStatusNow;
                F.InvokeEventExceptionSafe(this.onClientStatusChanged);
            }

            NetworkServerStatus serverStatusNow = NetworkStatus.serverStatus;
            if (serverStatusNow != m_lastServerStatus)
            {
                m_lastServerStatus = serverStatusNow;
                F.InvokeEventExceptionSafe(this.onServerStatusChanged);
            }

#if MIRROR
            // we need more agile ping measurement
            // the reason we assign this here, is because Mirror resets the value after scene load
            Mirror.NetworkTime.PingFrequency = 0.5f;
#endif
        }


        public static void StartServer(ushort portNumber, string scene, ushort maxNumPlayers, bool bIsDedicated, bool bDontListen)
        {
            // first start a server, and then change scene

            NetManager.onlineScene = scene;
            NetManager.dontListen = bDontListen;
            NetManager.maxNumPlayers = maxNumPlayers;
            if (bIsDedicated)
                NetManager.StartServer(portNumber);
            else
                NetManager.StartHost(portNumber);

            //NetManager.ChangeScene(scene);

        }

        private static void DoErrorChecksBeforeStartingServer(int portNumber)
        {
            CheckIfNetworkIsStarted();
            CheckIfPortIsValid(portNumber);
            CheckIfOnlineSceneIsAssigned();
            SetupNetworkManger("", portNumber);
        }

        public static void StartServer(int portNumber)
        {

            DoErrorChecksBeforeStartingServer(portNumber);

#if MIRROR
            NetworkManager.singleton.StartServer();
#endif
        }

        public static void StartHost(int portNumber)
        {

            DoErrorChecksBeforeStartingServer(portNumber);

#if MIRROR
            NetworkManager.singleton.StartHost();
#endif
        }

        public static void StopServer()
        {
#if MIRROR
            NetworkManager.singleton.StopServer();
#endif
        }

        public static void StartClient(string ip, int serverPortNumber)
        {

            CheckIfNetworkIsStarted();
            CheckIfIPAddressIsValid(ip);
            CheckIfPortIsValid(serverPortNumber);
            SetupNetworkManger(ip, serverPortNumber);

#if MIRROR
            NetworkManager.singleton.StartClient();
#endif
        }

        public static void StopClient()
        {
#if MIRROR
            NetworkManager.singleton.StopClient();
#endif
        }

        /// <summary>
        /// Stops both server and client.
        /// </summary>
        public static void StopNetwork()
        {
#if MIRROR
            //	NetworkManager.singleton.StopHost ();
            NetworkManager.singleton.StopServer();
            NetworkManager.singleton.StopClient();
#endif
        }


        public static void CheckIfServerIsStarted()
        {

            if (NetworkStatus.IsServerStarted)
                throw new System.Exception("Server already started");

        }

        public static void CheckIfClientIsStarted()
        {

            if (!NetworkStatus.IsClientDisconnected())
                throw new System.Exception("Client already started");

        }

        public static void CheckIfNetworkIsStarted()
        {

            CheckIfServerIsStarted();
            CheckIfClientIsStarted();

        }

        public static void CheckIfPortIsValid(int portNumber)
        {

            if (portNumber < 1 || portNumber > 65535)
                throw new System.ArgumentOutOfRangeException("portNumber", "Invalid port number");

        }

        private static void CheckIfIPAddressIsValid(string ip)
        {

            if (string.IsNullOrEmpty(ip))
                throw new System.ArgumentException("IP address empty");

            //	System.Net.IPAddress.Parse ();

        }

        private static void CheckIfOnlineSceneIsAssigned()
        {

            // we won't use scene management from NetworkManager
            //	if (string.IsNullOrEmpty (NetManager.onlineScene))
            //		throw new System.Exception ("Online scene is not assigned");

        }


        private static void SetupNetworkManger(string ip, int port)
        {
#if MIRROR
            NetworkManager.singleton.networkAddress = ip;

            switch (Transport.activeTransport)
            {
                case TelepathyTransport telepathyTransport:
                    telepathyTransport.port = (ushort)port;
                    break;
                case kcp2k.KcpTransport kcp2kTransport:
                    kcp2kTransport.Port = (ushort)port;
                    break;
                default:
                    throw new NotSupportedException("Can not assign port number to current transport");
            }
#endif
        }

        public static void Spawn(GameObject go)
        {
            NetworkStatus.ThrowIfNotOnServer();

#if MIRROR
            NetworkServer.Spawn(go);
#endif
        }

        public static void ChangeScene(string newScene)
        {
#if MIRROR
            NetworkManager.singleton.ServerChangeScene(newScene);
#endif
        }

        public static GameObject GetNetworkObjectById(uint netId)
        {
            if (!SpawnedDictionary.TryGetValue(netId, out var networkIdentity)) return null;
            return networkIdentity != null ? networkIdentity.gameObject : null;
        }

    }

}