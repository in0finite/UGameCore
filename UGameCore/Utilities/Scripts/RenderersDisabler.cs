using UnityEngine;
using System.Collections;

namespace UGameCore.Utilities {
	
	public class RenderersDisabler : MonoBehaviour {

		// Use this for initialization
		void Start () {
		
			Renderer[] renderers = this.GetComponentsInChildren<Renderer> ();
			foreach (Renderer r in renderers) {

				Destroy( r );

			}

		}
		

	}

}
