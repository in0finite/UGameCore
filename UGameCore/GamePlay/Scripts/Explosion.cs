using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


namespace uGameCore.GamePlay {
	
	public class Explosion : NetworkBehaviour {

		// Use this for initialization
		void Start () {
		


		}
		
		// Update is called once per frame
		void Update () {


			this.timeLived += Time.deltaTime;
			if( this.timeLived > this.lifeTime ) {
				if (this.isServer) {
					NetworkServer.Destroy (this.gameObject);
				} else if (this.isClient) {
					//	Destroy (this.gameObject);
				}
			}


		}



		[HideInInspector]	public	float	timeLived = 0 ;
		public	float	lifeTime = 2 ;


	}

}
