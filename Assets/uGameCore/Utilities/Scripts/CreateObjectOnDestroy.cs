using UnityEngine;

namespace uGameCore {
	
	public class CreateObjectOnDestroy : MonoBehaviour {

		public	GameObject	prefab = null ;
		public	Transform	position = null ;

		void OnDestroy() {

			if (null == prefab || null == position)
				return;

			prefab.InstantiateWithNetwork (position.position, position.rotation);

		}


	}

}
