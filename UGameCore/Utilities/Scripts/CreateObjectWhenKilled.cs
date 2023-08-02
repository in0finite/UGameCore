using UnityEngine;

namespace uGameCore {

	public class CreateObjectWhenKilled : MonoBehaviour {

		public	GameObject	prefab = null ;
		public	Transform	position = null ;


		void OnKilled( InflictedDamageInfo info ) {

			if (null == prefab || null == position)
				return;

			prefab.InstantiateWithNetwork (position.position, position.rotation);

		}


	}

}
