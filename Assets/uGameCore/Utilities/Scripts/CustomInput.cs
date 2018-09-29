using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace uGameCore.Utilities {


	public static class CustomInput {

		static Dictionary<string,float> axes = new Dictionary<string, float>();
		static Dictionary<string,bool> buttons = new Dictionary<string, bool>();

		static public float GetAxis(string name){
			float value = 0;
			axes.TryGetValue (name, out value);
			return value;
		}

		static public void SetAxis(string name, float value){
			if (!axes.ContainsKey (name)) {
				axes.Add (name, value);
			} else {
				axes [name] = value;
			}
		}

		static public bool GetButton(string name){
			bool value = false;
			buttons.TryGetValue (name, out value);
			return value;
		}

		static public void SetButton(string name, bool pressed){
			if (!buttons.ContainsKey (name)) {
				buttons.Add (name, pressed);
			} else {
				buttons [name] = pressed;
			}
		}

		/// <summary>
		/// Returns the acceleration affected by parameters from MainScript.
		/// </summary>
		public	static	float	GetVerticalAxisFromAcceleration() {

			float upperBound = Mathf.Min( 1, GameManager.singleton.accelerometerVerticalOffset + GameManager.singleton.minAccelerometerVerticalValue );
			float lowerBound = Mathf.Max( -1, GameManager.singleton.accelerometerVerticalOffset - GameManager.singleton.minAccelerometerVerticalValue );
			float acc = -Input.acceleration.z;

			if (acc > upperBound) {

				if (1 == upperBound)
					return 0;
				
				return (acc - upperBound) / (1 - upperBound);

			} else if (acc < lowerBound) {

				float minValue = Mathf.Min (0, GameManager.singleton.accelerometerVerticalOffset - 1);
				if (minValue == lowerBound)
					return 0;

				return - (lowerBound - acc) / (lowerBound - minValue);
			}
		
			return 0;
		}

		/// <summary>
		/// Returns the acceleration affected by parameters from MainScript.
		/// </summary>
		public	static	float	GetHorizontalAxisFromAcceleration() {

			float horizontalAxis = Input.acceleration.x;
			if(Mathf.Abs(horizontalAxis) < GameManager.singleton.minAccelerometerHorizontalValue)
				horizontalAxis = 0;
			
			return horizontalAxis;
		}

	}


}

