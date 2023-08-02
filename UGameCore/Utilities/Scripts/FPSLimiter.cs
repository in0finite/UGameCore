using UnityEngine;

namespace UGameCore.Utilities {
	
	public class FPSLimiter : MonoBehaviour {

		public	bool	limit = true ;
		public	int		targetFPS = 60 ;

		void Start () {

			if (limit) {
				QualitySettings.vSyncCount = 0;
				Application.targetFrameRate = targetFPS;
			}

		}
		

	}

}
