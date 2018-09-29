using UnityEngine;

namespace uGameCore
{

	/// <summary>
	/// Broadcasts network events messages to attached game object.
	/// </summary>
	public class LocalNetworkEventsDispatcher : MonoBehaviour
	{
		
		void Awake ()
		{

			NetworkEventsDispatcher.onServerStarted += () => Dispatch ("OnServerStarted");
			NetworkEventsDispatcher.onServerStopped += () => Dispatch ("OnServerStopped");

			NetworkEventsDispatcher.onClientConnected += () => Dispatch ("OnClientConnected");
			NetworkEventsDispatcher.onClientDisconnected += () => Dispatch ("OnClientDisconnected");
			NetworkEventsDispatcher.onClientStartedConnecting += () => Dispatch ("OnClientStartedConnecting");

		}

		private void Dispatch( string msg ) {

			this.gameObject.BroadcastMessageNoExceptions( msg );

		}

	}

}
