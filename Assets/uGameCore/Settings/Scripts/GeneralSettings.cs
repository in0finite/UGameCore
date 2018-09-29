using UnityEngine;

namespace uGameCore.Settings {

	public class GeneralSettings : MonoBehaviour {

		private	static	GeneralSettings	singleton = null;

//		[CVar]
//		private string nick = "Player";
//
//		[CVar(minValue = 1, maxValue = 100)]
//		private	float	fps_max = 60 ;
//
//		[CVar(minValue = 0, maxValue = 2000)]
//		private	float	mouse_sensitivity_x = 500 ;
//
//		[CVar(minValue = 0, maxValue = 2000)]
//		private	float	mouse_sensitivity_y = 500 ;
//
//		[CVar(minValue = 0, maxValue = 1, name = "Accelerometer minimum horizontal value")]
//		private	float	minAccHorizontalValue = 0.3f ;
//
//		[CVar(minValue = 0, maxValue = 1, name = "Accelerometer minimum vertical value")]
//		private	float	minAccVerticalValue = 0.3f ;
//
//		[CVar(minValue = 0, maxValue = 1, name = "Accelerometer vertical offset")]
//		private	float	accVerticalOffset = 0.3f ;


		private	static	string	m_nick = "Player" ;
		public static string Nick { get { return m_nick; } set {
				m_nick = value;
				// Nick is changed.
				// Update it on server.
				if (Player.local != null) {
					Player.local.ChangeNickOnServer ( (string) value );
				}
			} }

		private	static	int	m_fpsMax = 60;
		public static int Fps_max { get { return m_fpsMax; } set { m_fpsMax = value; GameManager.singleton.SetMaximumFps ( (int)value, false); } }

		public	bool	registerInputCvars = true;

		public static float Mouse_sensitivity_x { get ; set ; }

		public static float Mouse_sensitivity_y { get ; set ; }

		public static float MinAccHorizontalValue { get ; set ; }

		public static float MinAccVerticalValue { get ; set ; }

		public static float AccVerticalOffset { get ; set ; }


		GeneralSettings() {

			// set default values for properties

			Mouse_sensitivity_x = 500;
			Mouse_sensitivity_y = 500;
			MinAccHorizontalValue = 0.3f;
			MinAccVerticalValue = 0.3f;
			AccVerticalOffset = 0.3f;

		}

		void Awake() {

			singleton = this;


			CVarManager.onAddCVars += this.AddCvars;

		}

		void AddCvars() {

			CVar cvar = new CVar ();

			cvar.name = "nick";
			cvar.getValue = () => Nick;
			cvar.setValue = (arg) => Nick = (string) arg;
			cvar.isValid = (arg) => PlayerManager.IsValidPlayerName ( (string) arg );

			CVarManager.AddCVar (cvar);

			cvar = new CVar ();
			cvar.name = "fps_max";
			cvar.minValue = 1;
			cvar.maxValue = 100;
			cvar.getValue = () => Fps_max;
			cvar.setValue = (arg) => Fps_max = (int) arg;

			CVarManager.AddCVar (cvar);

			if (this.registerInputCvars) {

				cvar = new CVar ();
				cvar.name = "mouse_sensitivity_x";
				cvar.minValue = 0;
				cvar.maxValue = 2000;
				cvar.getValue = () => Mouse_sensitivity_x;
				cvar.setValue = (arg) => Mouse_sensitivity_x = (float)arg;

				CVarManager.AddCVar (cvar);

				cvar = new CVar ();
				cvar.name = "mouse_sensitivity_y";
				cvar.minValue = 0;
				cvar.maxValue = 2000;
				cvar.getValue = () => Mouse_sensitivity_y;
				cvar.setValue = (arg) => Mouse_sensitivity_y = (float)arg;

				CVarManager.AddCVar (cvar);

				cvar = new CVar ();
				cvar.name = "Accelerometer minimum horizontal value";
				cvar.minValue = 0;
				cvar.maxValue = 1;
				cvar.getValue = () => MinAccHorizontalValue;
				cvar.setValue = (arg) => MinAccHorizontalValue = (float)arg;

				CVarManager.AddCVar (cvar);

				cvar = new CVar ();
				cvar.name = "Accelerometer minimum vertical value";
				cvar.minValue = 0;
				cvar.maxValue = 1;
				cvar.getValue = () => MinAccVerticalValue;
				cvar.setValue = (arg) => MinAccVerticalValue = (float)arg;

				CVarManager.AddCVar (cvar);

				cvar = new CVar ();
				cvar.name = "Accelerometer vertical offset";
				cvar.minValue = 0;
				cvar.maxValue = 1;
				cvar.getValue = () => AccVerticalOffset;
				cvar.setValue = (arg) => AccVerticalOffset = (float)arg;

				CVarManager.AddCVar (cvar);

			}

		}

//		void	OnCVarChanged( CVar cvar ) {
//			
//			var newValue = CVarManager.GetCVarValue (cvar);
//
//
//			if ("fps_max" == cvar.name) {
//				
//				GameManager.singleton.SetMaximumFps ((int)(float)newValue, false);
//
//			} else if ("nick" == cvar.name) {
//				// Nick is changed.
//				// Update it on server.
//				if (NetworkStatus.IsClientConnected ()) {
//					Player.local.ChangeNickOnServer ( (string) newValue );
//				}
//			}
//
//		}

//		bool	OnValidateCVar( CVar cvar, object value ) {
//			
//			if ("nick" == cvar.name) {
//				return PlayerManager.IsValidPlayerName ((string)value);
//			}
//
//			return true;
//		}

	}

}
