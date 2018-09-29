using UnityEngine;

namespace uGameCore {

	public class PlayingObjectSetter : MonoBehaviour {

		public	GameObject	playingObject = null ;


		void Start () {

			PlayingObjectSpawner.singleton.playingObjectPrefab = this.playingObject ;

		}

	}

}
