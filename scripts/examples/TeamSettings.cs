using UnityEngine;

namespace uGameCore.Settings {

	public class TeamSettings : MonoBehaviour
	{

		private	static	bool	FFA { get { return TeamManager.IsFreeForAllModeOn (); } set { TeamManager.SetFreeForAllMode (value); } }

		private	static	bool	FriendlyFire { get { return TeamManager.IsFriendlyFireOn (); } set { TeamManager.singleton.isFriendlyFireOn = value; } }


		void Awake() {
			CVarManager.onAddCVars += this.AddCVars;
		}

		void AddCVars() {

			CVar cvar = new CVar ();

			cvar.name = "FFA";
			cvar.getValue = () => FFA;
			cvar.setValue = (arg) => FFA = (bool) arg;

			CVarManager.AddCVar (cvar);

			cvar = new CVar ();

			cvar.name = "Friendly fire";
			cvar.getValue = () => FriendlyFire;
			cvar.setValue = (arg) => FriendlyFire = (bool) arg;

			CVarManager.AddCVar (cvar);

		}


	}
}

