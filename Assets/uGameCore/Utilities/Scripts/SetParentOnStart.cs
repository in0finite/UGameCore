using UnityEngine;

namespace uGameCore.Utilities {

	public class SetParentOnStart : MonoBehaviour {

		public	Transform	parent = null ;
		public	bool	worldPositionStays = false ;

		void	Start() {

			this.transform.SetParent (parent, worldPositionStays);

		}

	}

}
