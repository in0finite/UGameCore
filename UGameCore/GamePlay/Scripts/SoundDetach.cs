using UnityEngine;
using System.Collections;

namespace uGameCore.GamePlay {
	
	public class SoundDetach : MonoBehaviour {

	//	public	AudioSource	audioSource = null ;
		public	GameObject	audioObjectPrefab = null ;
		public	bool	delayedDestroy = true;

		private	static	GameObject	soundsContainer = null;

		private	GameObject	detachedObject = null;


		// Use this for initialization
		void Start () {
		
			if (audioObjectPrefab != null) {

				AudioSource audioSource = this.audioObjectPrefab.GetComponentInChildren<AudioSource> ();
				if (audioSource != null) {

					if (null == soundsContainer) {
						soundsContainer = new GameObject ("SoundsContainer");
					}

					GameObject go = (GameObject) Instantiate (this.audioObjectPrefab, this.transform.position, Quaternion.identity);
					go.transform.SetParent (soundsContainer.transform);
				//	go.transform.position = this.transform.position;

					if (this.delayedDestroy) {
						// destroy game object after the sound has finished playing
						Destroy (go, audioSource.clip.length);
					}

					go.BroadcastMessage ("OnSoundDetached", this.gameObject, SendMessageOptions.DontRequireReceiver);

					// copy component
				//	AudioSource newAudioSource = go.AddComponent<AudioSource> ();
				//	audioSource.CopyTo (newAudioSource);

					// start playing the sound
				//	newAudioSource.Play ();
				
					// destroy component attached to this game object
				//	Destroy (audioSource);

					this.detachedObject = go;
				}
			}

		}

		void Update() {

			if (this.detachedObject != null) {
				this.detachedObject.transform.position = this.transform.position;
			}

		}

	}

}
