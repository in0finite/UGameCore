using UnityEngine;

namespace uGameCore.Utilities {

	/// <summary>
	/// Message that can be broadcasted when failed to join a game.
	/// </summary>
	public class FailedToJoinGameMessage
	{

		private	System.Exception	exception = null;

		/// <summary>
		/// Exception that caused the failure.
		/// </summary>
		public System.Exception Exception { get { return this.exception; } }



		public FailedToJoinGameMessage (System.Exception exception)
		{
			this.exception = exception;
		}


	}

}
