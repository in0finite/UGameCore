using UnityEngine;
using System.Collections;

namespace uGameCore.Utilities {
	
	public class CameraFollow : MonoBehaviour {

		private	Vector3	startOffset = Vector3.zero ;
		private	float	timeSinceChanged = 0;
		public	bool	updateHeight = true ;

		// Use this for initialization
		void Start () {
		
			if (Camera.main != null) {
				this.startOffset = Camera.main.transform.position - this.transform.position;
			}

		}
		
		// Update is called once per frame
		void Update () {
		
			if (null == Camera.main)
				return;

			this.timeSinceChanged += Time.deltaTime;

			if (this.timeSinceChanged > 5f) {

				Vector3 newPos = Camera.main.transform.position - this.startOffset;
				if (!this.updateHeight) {
					newPos.y = this.transform.position.y;
				}

				this.transform.position = newPos;

				this.timeSinceChanged = 0;
			}

		}
	}

}
