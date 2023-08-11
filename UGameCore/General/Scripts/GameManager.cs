using UnityEngine;
using System.Collections.Generic;

namespace UGameCore
{

    public	interface IForbidUserInput {

		bool	CanGameObjectsReadInput ();

	}

	public	interface IForbidGuiDrawing {

		bool	CanGameObjectsDrawGui ();

	}


	public class GameManager : MonoBehaviour {
		

		void	Awake() {

			if (null == singleton)
				singleton = this;

		}

		void Start () {
			
			this.fpsStopwatch.Start ();

			Debug.Log (Utilities.Utilities.GetAssetName() + " started");

		}

		void Update () {
			

			// calculate average fps
			float timeElapsed = this.fpsStopwatch.ElapsedMilliseconds / 1000f ;
			if (0f == timeElapsed)
				timeElapsed = float.PositiveInfinity;
			this.fpsStopwatch.Reset ();
			this.fpsStopwatch.Start ();

			float fpsNow = 1.0f / timeElapsed ;
			fpsSum += fpsNow ;
			fpsSumCount ++ ;

			if( Time.time - lastTimeFpsUpdated > secondsToUpdateFps ) {
				// Update average fps
				if( fpsSumCount > 0 ) {
					averageFps = fpsSum / fpsSumCount ;
				} else {
					averageFps = 0 ;
				}

				fpsSum = 0 ;
				fpsSumCount = 0 ;

				lastTimeFpsUpdated = Time.time ;
			}

		}
		

		public	static	bool	CanGameObjectsReadUserInput() {
			return m_forbidInputHandlers.TrueForAll( f => f.CanGameObjectsReadInput() );
		}

		public	static	bool	CanGameObjectsDrawGui() {
			return m_forbidGuiDrawingHandlers.TrueForAll (f => f.CanGameObjectsDrawGui());
		}

		public	static	void	RegisterInputForbidHandler( IForbidUserInput forbidder ) {

			m_forbidInputHandlers.Add (forbidder);

		}

		public	static	void	RegisterGuiDrawingForbidHandler( IForbidGuiDrawing forbidder ) {

			m_forbidGuiDrawingHandlers.Add (forbidder);

		}


        public void ExitApplication()
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit ();
#endif

        }

		public	void	SetMaximumFps( int maxFps, bool changeFixedDeltaTime ) {

			Application.targetFrameRate = maxFps;

			if (changeFixedDeltaTime) {
				Time.fixedDeltaTime = 1.0f / maxFps;
			}

		}

		public	static	float	GetAverageFps() {
			return singleton.averageFps;
		}


		private float averageFps = 0f ;
		private int secondsToUpdateFps = 1 ;
		private float lastTimeFpsUpdated = 0 ;
		private float fpsSum = 0f ;
		private int fpsSumCount = 0 ;
		private	System.Diagnostics.Stopwatch	fpsStopwatch = new System.Diagnostics.Stopwatch();


		[HideInInspector]	public	float	minAccelerometerVerticalValue = 0.3f ;
		[HideInInspector]	public	float	minAccelerometerHorizontalValue = 0.3f ;
		[HideInInspector]	public	float	accelerometerVerticalOffset = 0.3f ;


		private	static	List<IForbidUserInput>	m_forbidInputHandlers = new List<IForbidUserInput> ();
		private	static	List<IForbidGuiDrawing>	m_forbidGuiDrawingHandlers = new List<IForbidGuiDrawing> ();


		public	static	GameManager	singleton { get ; private set ; }

	}
}
