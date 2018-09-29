using UnityEngine;
using System.Collections;

namespace uGameCore.GamePlay.Projectiles {
	
	public class BulletAfterFireSoundStarter : MonoBehaviour {

		public	AudioSource	fireSound = null ;
		public	AudioSource	afterFireSound = null ;

		private	bool	startedAfterFireSound = false ;
		private	GameObject	bullet = null ;


		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
			if (null == fireSound || null == afterFireSound)
				return;
			
			if (!startedAfterFireSound) {

				if (!fireSound.isPlaying) {
					// fire sound finished playing
					afterFireSound.Play ();
					startedAfterFireSound = true;
				}

			} else {
				// if bullet is destroyed, destroy this object too
				if (null == this.bullet) {
					Destroy (this.gameObject);
				}
			}

		}

		void OnSoundDetached(GameObject go) {

			this.bullet = go;

		}

	}

}
