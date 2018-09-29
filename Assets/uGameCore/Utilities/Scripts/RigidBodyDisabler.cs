using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace uGameCore.Utilities {
	
	public class RigidBodyDisabler : NetworkBehaviour {

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public override void OnStartClient ()
		{
			base.OnStartClient ();


			Destroy (this.GetComponent<Rigidbody> ());
		//	this.GetComponent<Rigidbody> ().detectCollisions = false;
		//	this.GetComponent<Rigidbody> ().isKinematic = true;

		}

	}

}
