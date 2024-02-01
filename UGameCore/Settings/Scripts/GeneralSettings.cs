using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore.Settings {

	public class GeneralSettings : MonoBehaviour, IConfigVarRegistrator {

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
		}

        public void Register(IConfigVarRegistrator.Context context)
        {
            ConfigVar cvar = new StringConfigVar()
            {
                SerializationName = "nick",
                GetValueCallback = () => new ConfigVarValue { StringValue = Nick },
                SetValueCallback = (arg) => Nick = arg.StringValue,
                ValidateCallback = (arg) => PlayerManager.ValidatePlayerName(arg.StringValue),
                DefaultValue = new ConfigVarValue { StringValue = Nick },
            };

            context.ConfigVars.Add(cvar);

            cvar = new IntConfigVar()
            {
                SerializationName = "fps_max",
                MinValue = 5,
                MaxValue = 1000,
                GetValueCallbackInt = () => Fps_max,
                SetValueCallbackInt = (arg) => Fps_max = arg,
                DefaultValueInt = Fps_max,
            };

            context.ConfigVars.Add(cvar);

            if (this.registerInputCvars)
            {
                cvar = new FloatConfigVar()
                {
                    SerializationName = "mouse_sensitivity_x",
                    MinValue = 0,
                    MaxValue = 2000,
                    GetValueCallbackFloat = () => Mouse_sensitivity_x,
                    SetValueCallbackFloat = (arg) => Mouse_sensitivity_x = arg,
                    DefaultValueFloat = Mouse_sensitivity_x,
                };

                context.ConfigVars.Add(cvar);

                cvar = new FloatConfigVar()
                {
                    SerializationName = "mouse_sensitivity_y",
                    MinValue = 0,
                    MaxValue = 2000,
                    GetValueCallbackFloat = () => Mouse_sensitivity_y,
                    SetValueCallbackFloat = (arg) => Mouse_sensitivity_y = arg,
                    DefaultValueFloat = Mouse_sensitivity_y,
                };

                context.ConfigVars.Add(cvar);

                cvar = new FloatConfigVar()
                {
                    Description = "Accelerometer minimum horizontal value",
                    MinValue = 0,
                    MaxValue = 1,
                    GetValueCallbackFloat = () => MinAccHorizontalValue,
                    SetValueCallbackFloat = (arg) => MinAccHorizontalValue = arg,
                    DefaultValueFloat = MinAccHorizontalValue,
                };

                context.ConfigVars.Add(cvar);

                cvar = new FloatConfigVar()
                {
                    Description = "Accelerometer minimum vertical value",
                    MinValue = 0,
                    MaxValue = 1,
                    GetValueCallbackFloat = () => MinAccVerticalValue,
                    SetValueCallbackFloat = (arg) => MinAccVerticalValue = arg,
                    DefaultValueFloat = MinAccVerticalValue,
                };

                context.ConfigVars.Add(cvar);

                cvar = new FloatConfigVar()
                {
                    Description = "Accelerometer vertical offset",
                    MinValue = 0,
                    MaxValue = 1,
                    GetValueCallbackFloat = () => AccVerticalOffset,
                    SetValueCallbackFloat = (arg) => AccVerticalOffset = arg,
                    DefaultValueFloat = AccVerticalOffset,
                };

                context.ConfigVars.Add(cvar);
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
