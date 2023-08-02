using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


namespace uGameCore {


	public	class ProjectileHitInfo {
		public	Bullet	projectile = null;
		public	RaycastHit	hit;
	}


	public class Bullet : NetworkBehaviour {

		public	float	velocity = 500.0f ;
		public	float	lifeTime = 5 ;
		public	float	damage = 35 ;
		public	float	timeToAvoidCollisionWithPlayerShooter = 0 ;
		public	bool	hideOnStart = true ;
		public	GameObject	explosionPrefab = null ;
		public	float	impactForceStrength = 0 ;

		[System.NonSerialized]	public	Player	playerShooter = null ;
		[System.NonSerialized]	public	bool	shouldApplyDamageOnHit = false ;

		[System.NonSerialized]	public	float	timeAlive = 0.0f ;

		private	Vector3 startPosition = Vector3.zero ;
		private	Behaviour	halo = null ;



		// Use this for initialization
		protected	void Start () {

			this.halo = (Behaviour)this.GetComponent ("Halo");

			this.startPosition = this.transform.position;

			if (this.hideOnStart) {
				this.GetComponent<Renderer> ().enabled = false;
				if( this.halo != null )
					this.halo.enabled = false;
			}



		}
		
		// Update is called once per frame
		void Update () {


			this.timeAlive += Time.deltaTime;

			if (this.timeAlive > this.lifeTime) {
				this.MyDestroy();
				return ;
			}

		//	Rigidbody body = GetComponent<Rigidbody> ();

			if (this.isServer) {
				RaycastHit hitInfo;
				if (Physics.Raycast (transform.position, transform.forward, out hitInfo, this.velocity * Time.deltaTime * 1.1f)) {

					bool ignoreCollision = false;

					if (this.timeToAvoidCollisionWithPlayerShooter != 0) {
						if (this.timeAlive < this.timeToAvoidCollisionWithPlayerShooter) {
							// check if we hit player shooter object
							if (this.playerShooter != null) {
								Player player = PlayerManager.GetPlayerByGameObject (hitInfo.transform.gameObject);
								if (player == this.playerShooter) {
									// we hit player shooter object
									// we should ignore this collision
									ignoreCollision = true ;
								}
							}
						}
					}

					if (!ignoreCollision) {

					//	Debug.Log ("Bullet hit object " + hitInfo.transform.gameObject.name + ", distance " + hitInfo.distance);

						if (this.shouldApplyDamageOnHit) {

							// create explosion
							if (this.explosionPrefab != null) {
								this.explosionPrefab.InstantiateWithNetwork (hitInfo.point, Quaternion.identity);
							}

							// notify game object that it was hit
							ProjectileHitInfo projectileHitInfo = new ProjectileHitInfo();
							projectileHitInfo.projectile = this;
							projectileHitInfo.hit = hitInfo;
							hitInfo.transform.gameObject.BroadcastMessage("OnProjectileHit", projectileHitInfo, SendMessageOptions.DontRequireReceiver );

						}

						this.MyDestroy ();
						return;

					}

				}
			}


			if (this.isServer) {
				this.transform.position += this.transform.forward * this.velocity * Time.deltaTime;
			}


			if (this.hideOnStart) {
				if ((this.transform.position - this.startPosition).sqrMagnitude > 1 * 1) {
					this.GetComponent<Renderer> ().enabled = true;
					if( this.halo != null )
						this.halo.enabled = true;
				}
			}


		}

		private	void	MyDestroy() {

			if (this.isServer) {
				NetworkServer.Destroy (this.gameObject);
			} else {
			//	Destroy (this.gameObject);
			}

		}

		void	OnCollisionEnter(Collision collision) {



		}

		void OnDestroy() {
			
			// This function is called when the MonoBehaviour will be destroyed.
			// OnDestroy will only be called on game objects that have previously been active.



		}



	}


}

