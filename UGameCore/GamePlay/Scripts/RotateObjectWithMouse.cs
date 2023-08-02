using UnityEngine;

namespace uGameCore.GamePlay
{
	public class RotateObjectWithMouse : MonoBehaviour
	{
		/// <summary>
		/// Rotation speed in degrees.
		/// </summary>
		public	float	speed = 120.0f;


		void Update() {

			if (this.IsLocalPlayer ()) {
				if (GameManager.CanGameObjectsReadUserInput ()) {
					this.transform.Rotate (Vector3.up, Input.GetAxisRaw ("Mouse X") * this.speed * Time.deltaTime);
				}
			}

		}

	}
}

