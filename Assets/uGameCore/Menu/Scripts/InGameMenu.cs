using UnityEngine;

namespace uGameCore.Menu
{
	public class InGameMenu : Menu
	{

		/// <summary>
		/// In game menu is opened always while non-startup scene is opened - this allows for it to be visible
		/// even when other menus are opened (pause menu, settings menu, etc).
		/// </summary>
		protected override bool ShouldThisMenuBeOpened ()
		{
			return MenuManager.IsInGameScene ();
		}

	}
}

