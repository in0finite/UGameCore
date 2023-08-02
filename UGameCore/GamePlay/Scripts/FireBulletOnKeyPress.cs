using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore.GamePlay.Projectiles {

	public class FireBulletOnKeyPress : NetworkBehaviour {

		public	string keyName = "Jump";
		public	GameObject	bulletPrefab = null;
		public	float	fireInterval = 0.2f ;
		public	float	bulletVelocity = 20;
		private	float	m_timeSinceFired = Mathf.Infinity ;


		// Use this for initialization
		void Start () {

		}

		// Update is called once per frame
		void Update () {

			if (this.isLocalPlayer) {

				m_timeSinceFired += Time.deltaTime;

				if (Input.GetButton (this.keyName)) {
					if (GameManager.CanGameObjectsReadUserInput ()) {
						if (m_timeSinceFired >= this.fireInterval) {
							// fire
							this.Fire (this.transform.position + this.transform.forward * 3, this.transform.rotation);
							m_timeSinceFired = 0;
						}
					}
				}
			}

		}

		void Fire( Vector3 position, Quaternion rotation ) {

			if (null == this.bulletPrefab)
				return;

			if (this.isServer) {

				var go = this.bulletPrefab.InstantiateWithNetwork (position, rotation);
				var bullet = go.GetComponentInChildren<Bullet> ();
				if (bullet != null) {
					bullet.shouldApplyDamageOnHit = true;
					var co = GetComponent<ControllableObject> ();
					if (co != null)
						bullet.playerShooter = co.playerOwner;
					bullet.velocity = this.bulletVelocity;
				}

			} else {
				this.CmdFire (position, rotation);
			}

		}

		[Command]
		void CmdFire( Vector3 position, Quaternion rotation ) {

			this.Fire ( position, rotation );

		}

	}

}
