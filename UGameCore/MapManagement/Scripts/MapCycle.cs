using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace uGameCore.MapManagement {

	public class MapCycle : MonoBehaviour {

		[SerializeField]	private	bool	m_automaticMapChanging = true;
		public bool AutomaticMapChanging { get { return this.m_automaticMapChanging; } set { m_automaticMapChanging = value; } }

		private	float	m_timePassedSinceStartedMap = 0;
		public	float	TimePassedSinceStartedMap { get { return m_timePassedSinceStartedMap; } }

		public	float	mapChangeInterval = 20 * 60 ;

		private	int		m_currentMapCycleMapIndex = 0;
		public	List<string>	mapCycleList = new List<string>();
		public	List<Texture>	mapTextures = new List<Texture>();

		public	static	MapCycle	singleton { get ; private set ; }


		void Awake () {
			
			if (null == singleton) {
				singleton = this;
			}

		}

		// Use this for initialization
		void Start () {

			if(this.mapCycleList.Count > 0) {
				// set the online scene in network manager, so that when server is started, it automatically loads
				// the first scene from map cycle list
				NetworkManager.singleton.onlineScene = this.mapCycleList [0];
			}

		}

		void OnSceneChanged(SceneChangedInfo info) {

			/*
			var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
			int index = this.mapCycleList.IndexOf (scene.name);
			if (index > 0) {
				m_currentMapCycleMapIndex = (index + 1) % this.mapCycleList.Count;
			}
			*/


			m_timePassedSinceStartedMap = 0;

		}

		// Update is called once per frame
		void Update () {

			if (!NetworkServer.active)
				return;

			if (0 == this.mapCycleList.Count)
				return;
			
			if (NetworkStatus.IsServerStarted () && ! SceneChanger.isLoadingScene) {

				m_timePassedSinceStartedMap += Time.deltaTime;

				// automatic map changing
				if (this.AutomaticMapChanging) {
					if (m_timePassedSinceStartedMap > this.mapChangeInterval) {

						this.ChangeMapToNextMap ();

						//	m_timePassedSinceStartedMap = 0 ;
					}
				}

			}


		}

		public	void	ChangeMapToNextMap() {

			if (0 == this.mapCycleList.Count)
				return;

			if (!SceneChanger.isLoadingScene) {
				// initiate scene changing
				SceneChanger.ChangeScene (this.GetNextMap ());

				m_currentMapCycleMapIndex++;
				m_currentMapCycleMapIndex %= this.mapCycleList.Count;
			}

		}

		public	void	ChangeMapAndUpdateIndexOfCurrentMap( string mapName ) {

			if (SceneChanger.isLoadingScene)
				return;
			
			int index = this.mapCycleList.IndexOf (mapName);
			if (index < 0)
				return;

			if (SceneChanger.ChangeScene (mapName)) {
				m_currentMapCycleMapIndex = index;
			}

		}

		public	void	SetCurrentMapIndex( int index ) {

			if (index < 0 || index >= this.mapCycleList.Count)
				return;

			m_currentMapCycleMapIndex = index;
		}

		public	string	GetCurrentMapName() {

			return NetworkManager.networkSceneName;

		}

		public	string	GetNextMap() {

			if (0 == this.mapCycleList.Count)
				return "";

			int index = (m_currentMapCycleMapIndex + 1) % this.mapCycleList.Count;

			return this.mapCycleList [index];
		}

		public	int		GetTimeLeft() {

			float difference = this.mapChangeInterval - m_timePassedSinceStartedMap;

			if (difference < 0)
				return 0;

			return Mathf.RoundToInt (difference);
		}

		public	string		GetTimeLeftAsString() {

			int timeLeft = this.GetTimeLeft ();

			return Utilities.Utilities.FormatElapsedTime (timeLeft);
		}

		public	bool	IsValidMapName(string name) {

			if (0 == name.Length)
				return false;
			if (name.Length > 100)
				return false;

			return true;
		}

	}

}
