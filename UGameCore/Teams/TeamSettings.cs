using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore.Settings {

	public class TeamSettings : MonoBehaviour, IConfigVarRegistrator
	{

		private	static	bool	FFA { get { return TeamManager.IsFreeForAllModeOn (); } set { TeamManager.SetFreeForAllMode (value); } }

		private	static	bool	FriendlyFire { get { return TeamManager.IsFriendlyFireOn (); } set { TeamManager.singleton.isFriendlyFireOn = value; } }


        void IConfigVarRegistrator.Register(IConfigVarRegistrator.Context context)
        {
            var cvar = new BoolConfigVar()
            {
                SerializationName = "FFA",
                GetValueCallback = () => new ConfigVarValue { BoolValue = FFA },
                SetValueCallback = (arg) => FFA = arg.BoolValue,
                DefaultValueBool = FFA,
            };

            context.ConfigVars.Add(cvar);

            cvar = new BoolConfigVar()
            {
                SerializationName = "friendly_fire",
                GetValueCallback = () => new ConfigVarValue { BoolValue = FriendlyFire },
                SetValueCallback = (arg) => FriendlyFire = arg.BoolValue,
                DefaultValueBool = FriendlyFire,
            };

            context.ConfigVars.Add(cvar);
        }
    }
}

