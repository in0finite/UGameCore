using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace uGameCore.Utilities.UI {
	
	public class TabView : MonoBehaviour {
		

	//	private	RectTransform	m_rectTransform = null;
	//	public	RectTransform	rectTransform { get { return m_rectTransform; } private set { m_rectTransform = value; } }
		public	RectTransform	rectTransform { get { return this.GetComponent<RectTransform>(); } }

		[SerializeField]	private	List<Tab>	m_tabs = new List<Tab>();

		public List<Tab> TabsInChildren { get {
				var tabs = new List<Tab> (this.transform.childCount / 2);
				foreach (Transform child in this.transform) {
					var tab = child.GetComponent<Tab> ();
					if (tab != null)
						tabs.Add (tab);
				}
				return tabs;
			}
		}

		/// <summary>
		/// List of tabs. You can modify it as you wish, but you need to manually update TabView.
		/// </summary>
		public	List<Tab>	GetTabsList() { return m_tabs; }

		public	int	NumTabs { get { return m_tabs.Count; } }

		private	Tab m_activeTab = null;
		public Tab ActiveTab { get { return this.m_activeTab; } }


	//	public	Func<string, RectTransform>	createTabPanelFunction ;
	//	public	Func<string, RectTransform>	createTabButtonFunction ;

	//	public	Action<RectTransform>	setTabButtonPositionFunction ;
	//	public	Action<RectTransform>	setTabPanelPositionFunction ;

		public	Action<Tab>	activateTabFunction ;
		public	Action<Tab>	deactivateTabFunction ;


		public	event Action<Tab>	onTabAdded = delegate {};
		public	event Action	onSwitchedTab = delegate {};

		public	GameObject	tabButtonPrefab = null;
		public	GameObject	tabPanelPrefab = null;

		public	bool	bestFitForButtonText = false;

		public	Color	activeTabColor = Color.gray ;

		/// <summary> Height of area where tab buttons are placed. </summary>
		public	int		tabHeight = 35 ;

		public	int		tabButtonHeight { get { return this.tabHeight - this.tabsPaddingTop - this.tabsPaddingBottom; } }

		public	int tabsPaddingTop = 5;
		public	int	tabsPaddingLeft = 5;
		public	int	tabsPaddingBottom = 0;

		public	int	spaceBetweenTabButtons = 5;

		/*
		/// <summary>
		/// Tabs shown in inspector. The actual real tabs are located in child objects.
		/// </summary>
		[SerializeField]	private	List<string>	m_tabsForInspector = new List<string>();
		*/



		private	TabView() {

		//	createTabPanelFunction = CreateTabPanel;
		//	createTabButtonFunction = CreateTabButton;
		//	setTabButtonPositionFunction = SetPositionOfTabButton;
		//	setTabPanelPositionFunction = SetPositionOfTabPanel;
			activateTabFunction = ActivateTab;
			deactivateTabFunction = DeactivateTab;

		}

		/// <summary>
		/// Creates new tab. It doesn't set position of tab button or panel.
		/// </summary>
		public Tab AddTab ( string tabName ) {
			
			// add tab button as child of this game object
			var tabButton = CreateTabButton( tabName );

			// panel will also be the child of this game object
			var panel = CreateTabPanel (tabName);


			// attach tab script
			Tab tab = tabButton.gameObject.AddComponentIfDoesntExist<Tab> ();
			tab.tabView = this;
			tab.button = tabButton;
			tab.panel = panel;


			m_tabs.Add (tab);

			MySetDirty (tabButton.gameObject);
			MySetDirty (tab);	// not sure if it is needed
			MySetDirty (this);	// list of tabs is modified

			this.onTabAdded (tab);

			return tab;
		}

		public	Vector2		NormalizeCoordinates (Vector2 coord) {

			return new Vector2( coord.x / GetWidth (), coord.y / GetHeight () );

		}

		public	int	GetWidth () {

			return (int) this.rectTransform.rect.width;
		}

		public	int	GetHeight () {

			return (int) this.rectTransform.rect.height;
		}

//		public	int	GetParentWidth () {
//
//			if (null == this.transform.parent)
//				return Screen.width;
//
//			return (int) (this.transform.parent as RectTransform).rect.width;
//		}
//
//		public	int	GetParentHeight () {
//
//			if (null == this.transform.parent)
//				return Screen.height;
//
//			return (int) (this.transform.parent as RectTransform).rect.height;
//		}

		public	virtual	RectTransform CreateTabButton ( string tabName ) {

		//	var button = DataBinder.CreateButton (this.transform);

			var button = Instantiate (this.tabButtonPrefab);
			button.transform.SetParent (this.transform, false);

			button.gameObject.name = tabName + " button";

			var textComponent = button.GetComponentInChildren<Text> ();

			// set button text
			textComponent.text = tabName ;

			// set best fit for button text
			if (this.bestFitForButtonText) {
				textComponent.resizeTextForBestFit = true;
				textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;	// disable multiline text
				textComponent.verticalOverflow = VerticalWrapMode.Overflow ;	// fit to width
			}

			MySetDirty (button);
			MySetDirty (this.transform);

			return button.GetComponent<RectTransform> ();
		}

		public	virtual	float	GetButtonWidthBasedOnTextComponent( Text textComponent ) {

			float preferredWidth = textComponent.preferredWidth;
			float preferredHeight = textComponent.preferredHeight;

			// multiply button width by ratio between tabs height and preffered height => this will maintain aspect ratio of text component
			float buttonWidth = preferredWidth * this.tabButtonHeight / (float) preferredHeight ;

			return buttonWidth;
		}

		public	virtual	void	SetPositionOfTabButton (Tab tab, float leftCoordinate) {
			
			float buttonWidth = this.GetButtonWidthBasedOnTextComponent (tab.buttonTextComponent);

			float right = leftCoordinate + buttonWidth ;
			float top = this.GetHeight() - this.tabsPaddingTop;
			float bottom = this.GetHeight() - this.tabHeight + this.tabsPaddingBottom;

			tab.button.anchorMin = NormalizeCoordinates (new Vector2 (leftCoordinate, bottom ));
			tab.button.anchorMax = NormalizeCoordinates (new Vector2 (right, top ));
			tab.button.offsetMin = tab.button.offsetMax = Vector2.zero;

			MySetDirty (tab.button);
		}

		public	virtual	RectTransform CreateTabPanel ( string tabName ) {

		//	GameObject panelGameObject = new GameObject (tabName, typeof(RectTransform), typeof(Image));
			GameObject panelGameObject = Instantiate( this.tabPanelPrefab );

			panelGameObject.name = tabName + " panel";

			panelGameObject.transform.SetParent (this.transform, false);

			MySetDirty (panelGameObject);
			MySetDirty (this.transform);

			return panelGameObject.GetComponent<RectTransform> ();
		}

		public	virtual	void	SetPositionOfTabPanel (Tab tab) {
			
			int right = this.GetWidth ();
			int top = this.GetHeight () - this.tabHeight;

			tab.panel.anchorMin = Vector2.zero ;
			tab.panel.anchorMax = NormalizeCoordinates (new Vector2 ( right, top ));
			tab.panel.offsetMin = tab.panel.offsetMax = Vector2.zero ;

			MySetDirty (tab.panel);
		}


		public	void	UpdatePositionsOfTabs() {

			this.UpdatePositionsOfTabs (m_tabs.WhereAlive ().ToList ());

		}

		/// <summary>
		/// Updates positions of specified tabs, as if they are the only tabs in TabView.
		/// </summary>
		public	virtual	void	UpdatePositionsOfTabs( List<Tab> tabs ) {

			float left = this.tabsPaddingLeft;

			foreach (var tab in tabs) {
				
				this.SetPositionOfTabButton (tab, left);

				left += tab.button.rect.width + this.spaceBetweenTabButtons;

				// update position of panel

				this.SetPositionOfTabPanel (tab);

			}

		}


		protected	static	void	MyDestroy( UnityEngine.Object obj ) {

			if (Application.isEditor && !Application.isPlaying) {
				// edit mode => we have to destroy objects using DestroyImmediate
				DestroyImmediate (obj, false);
			} else {
				Destroy (obj);
			}

		}

		protected	static	void	MySetDirty( UnityEngine.Object obj ) {

			if (Application.isEditor && !Application.isPlaying) {
				Utilities.MarkObjectAsDirty (obj);
			}

		}


		public	void	DeleteAllTabsAndPanels () {
			
			foreach (var tab in m_tabs.WhereAlive ()) {
				
				DeleteTabAndHisPanel (tab);

			}

			m_tabs.Clear ();

		//	m_activeTab = null;

			MySetDirty (this.transform);
			MySetDirty (this);	// list of tabs is modified

		}

		public	void	DeleteTabAndHisPanel( Tab tab ) {

			MyDestroy (tab.button.gameObject);
			MyDestroy (tab.panel.gameObject);

			MySetDirty (this.transform);
		}


		public	void	SwitchTab (Tab newActiveTab) {

			if (newActiveTab == m_activeTab)
				return;
			
			foreach( var tab in m_tabs.WhereAlive () ) {

				if (tab == newActiveTab) {
					// this is the new active tab
					// activate new tab
					activateTabFunction (tab);
				} else {
					// this is not the active tab
					// deactivate it
					deactivateTabFunction (tab);
				}
			}

			m_activeTab = newActiveTab;

			Utilities.InvokeEventExceptionSafe (this.onSwitchedTab);

		}

		public	static	void	ActivateTab (Tab tab) {
			tab.panel.gameObject.SetActive (true);
			tab.buttonImageComponent.color = tab.tabView.activeTabColor;

			MySetDirty (tab.panel.gameObject);
			MySetDirty (tab.buttonImageComponent);
		}

		public	static	void	DeactivateTab (Tab tab) {
			tab.panel.gameObject.SetActive (false);
			tab.buttonImageComponent.color = tab.originalButtonColor;

			MySetDirty (tab.panel.gameObject);
			MySetDirty (tab.buttonImageComponent);
		}


		public	void	ApplyTabsFromList () {

			m_tabs.RemoveAllDeadObjects ();

			var allTabs = this.TabsInChildren;

			// remove duplicates
			m_tabs = m_tabs.Distinct().ToList();

			// delete all tabs that are not in the new list
			foreach (var tab in allTabs) {
				if (!m_tabs.Contains (tab)) {
					this.DeleteTabAndHisPanel (tab);
				}
			}

			// update positions of new tabs
			this.UpdatePositionsOfTabs();

			MySetDirty (this);	// list of tabs is modified

		}



		void Start () {

			// if there is no active tab, activate first one
			if (null == m_activeTab) {
				var tabs = m_tabs;
				if (tabs.Count > 0)
					SwitchTab (tabs [0]);
			}

		}


	}

}
