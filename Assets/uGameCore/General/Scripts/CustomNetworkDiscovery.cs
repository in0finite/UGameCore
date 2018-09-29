
namespace uGameCore {
	
	public class CustomNetworkDiscovery : UnityEngine.Networking.NetworkDiscovery
	{
		
		public	event	System.Action<string, string>	onReceivedBroadcast = delegate {};


		public override void OnReceivedBroadcast (string fromAddress, string data)
		{
			base.OnReceivedBroadcast (fromAddress, data);

			Utilities.Utilities.InvokeEventExceptionSafe (onReceivedBroadcast, fromAddress, data);
		}

	}

}
