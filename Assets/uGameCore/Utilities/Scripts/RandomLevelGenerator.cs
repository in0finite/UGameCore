using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace uGameCore.Utilities {
	
	public class RandomLevelGenerator : MonoBehaviour {

		public	bool	generateOnStart = true ;
		public	GameObject	objectPrefab = null;
		public	GameObject	spawnPositionPrefab = null;
		public	Vector3		distancePerAxis = new Vector3( 10, 10, 10 ) ;
		public	int[]		numberOfObjectsPerAxis = new int[] { 15, 1, 15 } ;
		public	Vector3	startPosition = Vector3.zero ;
		public	bool	randomScale = true ;

		private	static	RandomLevelGenerator	singleton = null;
		private	static	List<GameObject>	generatedObjects = new List<GameObject> (100);



		void Awake() {

			singleton = this;

		}

		// Use this for initialization
		void Start () {
		
			if (this.generateOnStart) {
				Generate ();
			}

		}
		
		public	static	void	Generate() {

			if (null == singleton.objectPrefab)
				return;

			Random.seed = (int) (Time.realtimeSinceStartup * 1000);

			int numObjectsToCreate = singleton.numberOfObjectsPerAxis [0] * singleton.numberOfObjectsPerAxis [1]
			                         * singleton.numberOfObjectsPerAxis [2];
			int numObjectsCreated = 0;

			Vector3 maxPos = singleton.startPosition + Vector3.Scale (new Vector3 (singleton.numberOfObjectsPerAxis [0], singleton.numberOfObjectsPerAxis [1],
				                 singleton.numberOfObjectsPerAxis [2]), singleton.distancePerAxis);
			Vector3 offset = Vector3.zero;
			float scaleLength = singleton.objectPrefab.transform.localScale.magnitude;

			for (int i = 0; i < singleton.numberOfObjectsPerAxis [0]; i++) {
				for (int j = 0; j < singleton.numberOfObjectsPerAxis [1]; j++) {
					for (int k = 0; k < singleton.numberOfObjectsPerAxis [2]; k++) {

					//	Vector3 pos = singleton.startPosition + Vector3.Scale (offset, Vector3.one);
					//	Vector3 pos = singleton.startPosition + offset * Random.Range( 0.8f, 1.2f ) ;
						Vector3 pos = Vector3.Scale( maxPos - singleton.startPosition, Random3ValuesAsVector3() );

						GameObject go = (GameObject) Instantiate (singleton.objectPrefab, pos, Random.rotation);

						if (singleton.randomScale)
						//	go.transform.localScale = Vector3.Scale (go.transform.localScale, Vector3.one / 2.0f + Random3ValuesAsVector3() );
							go.transform.localScale = scaleLength * Random3ValuesAsVector3() ;

						if (NetworkServer.active && go.GetComponentInChildren<NetworkIdentity>() != null)
							NetworkServer.Spawn (go);

						generatedObjects.Add (go);

						numObjectsCreated++;
						if (numObjectsCreated % (numObjectsToCreate / 20) == 0) {
							// create spawn position
							Vector3 eulers = Vector3.zero ;
							eulers.y = Random.Range (0, 360);

							Instantiate (singleton.spawnPositionPrefab, pos + Vector3.up * singleton.distancePerAxis.y / 2,
								Quaternion.Euler (eulers));
						}

						offset += Vector3.forward * singleton.distancePerAxis.z ;
					}
					offset.z = 0;
					offset += Vector3.up * singleton.distancePerAxis.y ;
				}
				offset.z = 0;
				offset.y = 0;
				offset += Vector3.right * singleton.distancePerAxis.x ;
			}

		}

		public	static	void	DestroyAllGeneratedObjects() {

			foreach (GameObject go in generatedObjects) {
				if (null == go)
					continue;
				
				if (NetworkServer.active)
					NetworkServer.Destroy (go);
				else
					Destroy (go);
			}

			generatedObjects.Clear ();
		}

		/// <summary>
		/// Generates 3 random values and returns them as Vector3.
		/// </summary>
		public	static	Vector3	Random3ValuesAsVector3() {

			return new Vector3 (Random.value, Random.value, Random.value);

		}

	}

}
