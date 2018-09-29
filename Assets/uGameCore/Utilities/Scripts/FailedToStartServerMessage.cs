using UnityEngine;

namespace uGameCore.Utilities {

	/// <summary>
	/// Message that can be broadcasted when failed to start a server.
	/// </summary>
	public class FailedToStartServerMessage
	{
		
		private	System.Exception	exception = null;

		/// <summary>
		/// Exception that caused the failure.
		/// </summary>
		public System.Exception Exception { get { return this.exception; } }



		public FailedToStartServerMessage (System.Exception exception)
		{
			this.exception = exception;
		}
		

		public	static	void	Broadcast( System.Exception exception ) {

			Utilities.SendMessageToAllMonoBehaviours ("OnFailedToStartServer", 
				new FailedToStartServerMessage (exception));

		}

	}

}
