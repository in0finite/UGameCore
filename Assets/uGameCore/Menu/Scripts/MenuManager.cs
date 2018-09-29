using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using uGameCore;

namespace uGameCore.Menu {

	public class MenuManager : MonoBehaviour 
	{

		public string goBackButton = "Cancel";

		public	static	string	ActiveMenuName {
			get {
				if (m_activeMenu)
					return m_activeMenu.menuName;
				else
					return "";
			}
		}

		private	static	Menu	m_activeMenu = null;
		public	static	Menu	ActiveMenu { get { return m_activeMenu; } }

	//	private	static	List<Menu>	m_menus = new List<Menu> ();

	//	internal	static	event System.Action	onActiveMenuChangedInternal = delegate {};
		public	static	event System.Action	onActiveMenuChanged = delegate {};

		public	string	startupMenuName = "";
		public	string	inGameMenuName = "";
		public	string	pauseMenuName = "" ;
		public	string	gameOverMenuName = "";

		public	static	MenuManager	singleton { get ; private set ; }



		void Awake() {

			if (null == singleton) {
				singleton = this;
			}

		}

		void Start ()
		{

			if (null == EventSystem.current) {
				Debug.LogWarning ("EventSystem not found");
			}

			SwitchMenu (this.startupMenuName);

		}

		void OnSceneChanged( SceneChangedInfo info ) {

			if (IsInGameScene ()) {
				SwitchToInGameMenu ();
			} else {
				SwitchMenu (startupMenuName);
			}

		}

		/// <summary>
		/// Are we in a game scene, or in the startup scene ?
		/// </summary>
		public	static	bool	IsInGameScene() {

			Scene scene = SceneManager.GetActiveScene ();

			if (!scene.isLoaded)
				return false;

			return scene.buildIndex != 0 && scene.buildIndex != 1 ;
		}

		public	static	bool	IsInGameMenu() {

			return singleton.inGameMenuName != "" && MenuManager.ActiveMenuName == singleton.inGameMenuName ;

		}

		public	static	void	QuitToMainMenu() {

			NetManager.StopNetwork ();

		}

		public	static	void	SwitchMenu(string menuName) {

			Menu menu = FindMenuByName (menuName);
			if (null == menu)
				return;

			SwitchMenu (menu);

		}

		public	static	void	SwitchMenu(Menu newMenu) {

			if (m_activeMenu == newMenu)
				return;

			m_activeMenu = newMenu;

			// enable event system
		//	this.isInputEnabled = true ;

			RemoveFocus ();

			// notify menus
			var allMenus = GetAllMenus ();
			foreach (var m in allMenus) {
				m.OnActiveMenuChanged ();
			}

			Utilities.Utilities.InvokeEventExceptionSafe (onActiveMenuChanged);

		}

		public	static	void	SwitchToInGameMenu() {
			
			SwitchMenu (singleton.inGameMenuName);

		}

		public	static	void	Resign() {

			SwitchMenu (singleton.gameOverMenuName);

			// reset text
			SetGameOverMenuDescriptionText( "" );

		}

		public	static	void	SetGameOverMenuDescriptionText( string descriptionText ) {

			Menu menu = FindMenuByName( singleton.gameOverMenuName );
			if (menu) {
				var descriptionTransform = menu.transform.FindChild ("GameSummary");
				if (descriptionTransform) {
					var textComponent = descriptionTransform.GetComponentInChildren<Text> ();
					if (textComponent)
						textComponent.text = descriptionText;
				}
			}

		}

		/// <summary> Opens parent of the current menu, if it exists. </summary>
		public	void	OpenParentMenu() {

			if (m_activeMenu != null) {
				string parentMenuName = m_activeMenu.parentMenu;
				if (parentMenuName != "") {
					// switch to parent menu
					SwitchMenu (parentMenuName);
				}
			}

		}

		/// <summary>
		/// Removes the focus from the current element.
		/// </summary>
		protected	static	void	RemoveFocus() {

			UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject (null);

		}


		void Update()
		{
			
			if (Input.GetButtonDown (this.goBackButton)) {
				OpenParentMenu ();
			}

		}


		public	static	Menu[]	GetAllMenus() {

			return FindObjectsOfType<Menu> ();

		}

		public	static	Menu	FindMenuByName( string menuName ) {

			if (string.IsNullOrEmpty (menuName))
				return null;

			Menu foundMenu = System.Array.Find ( GetAllMenus (), m => m.menuName == menuName);
			return foundMenu;

		}

		/// <summary>
		/// Finds the menu by name, and throws exception if it is not found.
		/// </summary>
		public	static	Menu	FindMenuByNameOrThrow( string menuName ) {

			var menu = MenuManager.FindMenuByName (menuName);

			if (null == menu)
				throw new System.Exception ("Failed to find a menu with name: " + menuName);

			return menu;
		}

		public	static	Menu	FindJoinGameMenu() {

			return MenuManager.FindMenuByName ("JoinGameMenu");

		}


		public Transform	FindChildOfMenu(RectTransform menu, string childName) {

			return System.Array.Find (menu.GetComponentsInChildren<Transform> (), t => t.name == childName);
		}

		public Transform	FindChildOfMenu(Canvas menu, string childName) {
			return FindChildOfMenu (menu.GetComponent<RectTransform> (), childName);
		}

		private	string	ReadInputField(RectTransform menu, string childName) {

			return FindChildOfMenu( menu, childName).GetComponent<InputField>().text;

		}


		public	static	void	StartServerWithSpecifiedOptions( bool asHost ) {
			
			if(asHost)
				NetManager.StartHost (NetManager.defaultListenPortNumber);
			else
				NetManager.StartServer (NetManager.defaultListenPortNumber);
			
		}

		/// <summary>
		/// Tries to connect to server with parameters from UI, and notifies scripts in case of failure.
		/// </summary>
		public	void	ConnectToServerWithParameters() {

			try {

				var menu = MenuManager.FindMenuByNameOrThrow ("JoinGameMenu");

				var tabView = menu.GetComponentInChildren<Utilities.UI.TabView>();
				if(null == tabView)
					throw new Utilities.ObjectNotFoundException("Failed to find TabView");

				if(null == tabView.ActiveTab)
					throw new System.Exception("No tab is selected");

				string ip = "";
				int port = 0;

				string activeTabText = tabView.ActiveTab.tabButtonText;

				if (activeTabText == "Direct") {
					// read ip and port from input controls

					RectTransform rectTransform = tabView.ActiveTab.panel.GetRectTransform ();

					ip = ReadInputField( rectTransform, "Ip");
					port = int.Parse (ReadInputField (rectTransform, "PortNumber"));

				} else if (activeTabText == "LAN") {
					// read ip and port from table

					var table = tabView.ActiveTab.panel.GetComponentInChildren<Utilities.UI.Table>();
					if(null == table)
						throw new Utilities.ObjectNotFoundException("Table with LAN servers not found");

					if(table.RowsCount < 1)
						throw new System.Exception("No servers available");

					if(null == table.SelectedRow)
						throw new System.Exception("Select a server first");

					var serverData = LANScan2UI.GetBroadcastDataFromRow( table.SelectedRow );
					ip = serverData.FromAddress ;
					port = int.Parse( serverData.KeyValuePairs["Port"] );

				} else {
					throw new System.Exception("Unknown tab opened");
				}


				NetManager.StartClient (ip, port);

			} catch( System.Exception ex ) {

				Debug.LogException (ex);

				// notify scripts
				Utilities.Utilities.SendMessageToAllMonoBehaviours ("OnFailedToJoinGame", 
					new Utilities.FailedToJoinGameMessage (ex));
			}

		}

	}

}
