using System;
using UnityEngine;

namespace uGameCore
{
	public class BasicCameraController : CameraController
	{
		public	Vector3	offset = new Vector3( 0, 3, -4 );
		public	float	lookAtHeightOffset = 1.5f ;


		protected override void UpdateCamera ()
		{
			Vector3 newPos = this.transform.position;

			newPos += this.transform.forward * this.offset.z;
			newPos += this.transform.up * this.offset.y;
			newPos += this.transform.right * this.offset.x;

			Camera.main.transform.position = newPos;

			Camera.main.transform.LookAt (this.transform.position + this.transform.up * this.lookAtHeightOffset);
		}

	}
}

