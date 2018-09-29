using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Menu {

	public class Menu : MonoBehaviour 
	{

		public	string	menuName = "" ;
		public	string	parentMenu = "" ;

		public	bool	disableInputFieldsWhenClosed = false ;

		private	Canvas	m_canvas = null;
		public Canvas Canvas { get { return m_canvas; } }

		private	bool	m_isOpened = false;
		public bool IsOpened { get { return m_isOpened; } }



		void Awake ()
		{
			m_canvas = this.GetComponent<Canvas> ();

		}

		void Start()
		{

			bool shouldBeOpened = this.ShouldThisMenuBeOpened ();

			m_isOpened = shouldBeOpened;
			m_canvas.enabled = shouldBeOpened;

		}


		// called from MenuManager
		internal	void	OnActiveMenuChanged() {

			bool wasOpened = m_isOpened;

			bool isOpenedNow = this.ShouldThisMenuBeOpened ();

			m_isOpened = isOpenedNow;
			m_canvas.enabled = isOpenedNow;

			if (isOpenedNow != wasOpened) {
				// notify scripts that the state of this menu has changed
				if (isOpenedNow) {
					this.gameObject.BroadcastMessageNoExceptions ("OnMenuOpened");
				} else {
					this.gameObject.BroadcastMessageNoExceptions ("OnMenuClosed");
				}
			}

		}

		protected	virtual	bool	ShouldThisMenuBeOpened() {

			return this == MenuManager.ActiveMenu;

		}


		void OnMenuClosed() {

			if (this.disableInputFieldsWhenClosed) {
				foreach (var inputField in GetComponentsInChildren<InputField>()) {
					inputField.enabled = false;
				}
			}

		}

		void OnMenuOpened() {

			if (this.disableInputFieldsWhenClosed) {
				foreach (var inputField in GetComponentsInChildren<InputField>()) {
					inputField.enabled = true;
				}
			}

		}

	}

}
