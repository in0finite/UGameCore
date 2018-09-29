using UnityEngine;

namespace uGameCore.KillEvents {

	/// <summary>
	/// Used to populate scroll view content with kill events.
	/// </summary>
	public class KillEventUI : MonoBehaviour {

		public	Utilities.PopulateScrollViewWithEvents populator = null;


		void Start () {

			if (null == this.populator)
				return;

			NetworkEventsDispatcher.onClientDisconnected += () => this.populator.RemoveAllEventsFromUI() ;
			NetworkEventsDispatcher.onServerStopped += () => this.populator.RemoveAllEventsFromUI ();
			KillEventSync.onKillEvent += (KillEvent killEvent) => this.populator.EventHappened(
				killEvent.killer + "<color=red> => </color>" + killEvent.dier) ;

		}


	}

}
