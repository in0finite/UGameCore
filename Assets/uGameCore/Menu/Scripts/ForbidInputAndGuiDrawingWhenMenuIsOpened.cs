using UnityEngine;

namespace uGameCore.Menu {
	
	public class ForbidInputAndGuiDrawingWhenMenuIsOpened : MonoBehaviour, IForbidUserInput, IForbidGuiDrawing {

		void Awake() {

			GameManager.RegisterInputForbidHandler (this);
			GameManager.RegisterGuiDrawingForbidHandler (this);

		}


		public bool CanGameObjectsReadInput ()
		{
//			if ("" == MenuController.singleton.inGameMenu)
//				return true;
//			
//			return MenuController.singleton.canvasIndex == MenuController.singleton.inGameMenu;


			return MenuManager.IsInGameMenu ();
		}

		public bool CanGameObjectsDrawGui ()
		{
//			if ("" == MenuController.singleton.inGameMenu)
//				return true;
//
//			return MenuController.singleton.canvasIndex == MenuController.singleton.inGameMenu;


			return MenuManager.IsInGameMenu ();
		}

	}

}
