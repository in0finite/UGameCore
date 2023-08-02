using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace uGameCore.Menu {

	/// <summary>
	/// Controls button text component for better visual effects. It detects when button is hovered or clicked,
	/// and changes it's text font size and color, and plays sound accordingly. It also provides methods for menu and
	/// network integration.
	/// </summary>
	public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
	{
		
		public	bool	modifyFontSize = false;
		public	int	regularFontSize = 15;
		public	int	highlightedFontSize = 18;

		public	bool	modifyTextColor = false;
		public	Color	regularTextColor = Color.white;
		public	Color	highlightedTextColor = Color.blue;

		public	bool	playHoverSound = false;
		public	AudioClip	hoverSound = null;

		public	bool	playClickSound = false;
		public	AudioClip	clickSound = null;

		private	Text	m_buttonTextComponent = null;



		void Awake () {

			m_buttonTextComponent = GetComponentInChildren<Text> ();

		}

		void Start () {

			if (modifyFontSize) {
				m_buttonTextComponent.fontSize = regularFontSize;
			}

			if (modifyTextColor) {
				m_buttonTextComponent.color = regularTextColor;
			}

		}


		public void OnPointerDown (PointerEventData eventdata)
		{

			if (playClickSound) {
				var audioSource = GetComponent<AudioSource> ();
				audioSource.clip = clickSound;
				audioSource.Play ();
			}

		}

		public void OnPointerEnter (PointerEventData eventdata)
		{
			
			if (modifyFontSize) {
				m_buttonTextComponent.fontSize = highlightedFontSize;
			}

			if (modifyTextColor) {
				m_buttonTextComponent.color = highlightedTextColor;
			}

			if (playHoverSound) {
				var audioSource = GetComponent<AudioSource> ();
				audioSource.clip = hoverSound ;
				audioSource.Play ();
			}

		}

		public void OnPointerExit (PointerEventData eventdata)
		{

			if (modifyFontSize) {
				m_buttonTextComponent.fontSize = regularFontSize;
			}

			if (modifyTextColor) {
				m_buttonTextComponent.color = regularTextColor ;
			}

		}


		public	void	StartServer() {
			NetManager.StartHost (NetManager.defaultListenPortNumber);
		}

		public	void	StartServerWithSpecifiedOptions( RectTransform menuObject ) {

			MenuManager.StartServerWithSpecifiedOptions (true);
		}

		public	void	ConnectToServerWithParameters() {
			
			MenuManager.singleton.ConnectToServerWithParameters ();

		}


		public	void	ExitGame() {
			GameManager.singleton.ExitApplication ();
		}

		public	void	QuitToMenu() {
			MenuManager.QuitToMainMenu ();
		}

		public	void	ReturnToGame() {
			MenuManager.SwitchToInGameMenu ();
		}

		public	void	Resign() {
			MenuManager.Resign ();
		}

		public	void	OpenMenu( string menuName ) {
			MenuManager.SwitchMenu (menuName);
		}

		public	void	OpenMenuAndSetItsParentToCurrentMenu( string menuName ) {

			var menu = MenuManager.FindMenuByName (menuName);
			if (null == menu)
				return;

			menu.parentMenu = MenuManager.ActiveMenuName;

			MenuManager.SwitchMenu (menu);
		}

		public	void	GoToParent() {

			MenuManager.singleton.OpenParentMenu ();

		}

	}

}
