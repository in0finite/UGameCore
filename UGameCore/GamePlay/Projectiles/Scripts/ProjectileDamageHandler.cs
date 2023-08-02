using UnityEngine;
using System.Collections;

namespace uGameCore {

	/// <summary>
	/// Catches OnProjectileHit message and inflicts damage to Damagable which is attached to the same game object.
	/// </summary>
	public class ProjectileDamageHandler : MonoBehaviour {


		private void OnProjectileHit( ProjectileHitInfo hit ) {

			var d = GetComponent<Damagable> ();
			if (d != null) {
				d.Damage (hit.projectile.damage, hit.projectile.playerShooter);
			}

		}


	}

}

