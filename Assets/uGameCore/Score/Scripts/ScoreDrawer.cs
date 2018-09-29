using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;

namespace uGameCore.Score {
	
	public class ScoreDrawer : MonoBehaviour {

		public	static	ScoreDrawer	singleton { get ; private set ; }

		private	static	bool	m_isScoreDrawingToggled = false ;
		public	static	bool	IsScoreboardOpened { get { return m_isScoreDrawingToggled; } set { m_isScoreDrawingToggled = value; } }

		public	KeyCode	toggleKey = KeyCode.Tab ;

		private	Canvas	scoreCanvas = null;
		private	RectTransform	scorePanel = null;
		public	GameObject	tableEntryPrefab = null;

		[SerializeField]	private	int	m_horizontalMargin = 3 ;
		[SerializeField]	private	int	m_verticalMargin = 3 ;
		public	static int HorizontalMargin { get { return singleton.m_horizontalMargin; } set { singleton.m_horizontalMargin = value; } }
		public	static int VerticalMargin { get { return singleton.m_verticalMargin; } set { singleton.m_verticalMargin = value; } }

		[SerializeField]	private	string	m_sortingColumn = "" ;
		public	static	string	SortingColumn { get { return singleton.m_sortingColumn; } set { singleton.m_sortingColumn = value; } }
		[SerializeField]	private	bool	m_descendingSort = true ;
		public	static	bool	DescendingSort { get { return singleton.m_descendingSort; } set { singleton.m_descendingSort = value; } }
		public	string	sortingColumnDescendingTextSuffix = " ▼";
		public	string	sortingColumnAscendingTextSuffix = " ▲";

		public	struct TableEntryData
		{
			public	string	text ;
			public	Rect	rect ;
			public	int fontSize ;
			public	Color	color ;
			public	bool	isHeader;
		//	public	TableEntryData() { text = ""; rect = Rect.zero; fontSize = 0; color = Color.black; }
		}

		private	static	int	m_currentColumnCount = 0;
		private	static	List<Text>	m_tableEntries = new List<Text>();

		public struct RowsPerTeam
		{
			public string team ;
			public List<object> rows ;
			public RowsPerTeam (string team, List<object> rows)
			{
				this.team = team;
				this.rows = rows;
			}
		}

		public	static	System.Func<List<float>>	func_getColumnsWidthPercentages = GetColumnWidthPercentages;
		public	static	System.Func<List<string>>	func_getColumnsNames = GetColumnNames;
		public	static	System.Func<List<RowsPerTeam>>	func_getRowsPerTeam = GetRowsPerTeam;
		public	static	System.Func<object, List<object>>	func_getValuesForRow = GetValuesForRow;
		/// <summary> Gets compare value for sorting rows. Second argument is index of sorting column. </summary>
		public	static	System.Func<object, int, System.IComparable>	func_getCompareValuesForSorting = GetCompareValueForSorting;




		void Awake () {

			singleton = this;

			// find UI elements
			this.scoreCanvas = Utilities.Utilities.FindObjectOfTypeOrLogError<ScoreCanvas>().GetComponent<Canvas>();
			this.scorePanel = this.scoreCanvas.transform.FindChild ("ScorePanel").GetComponent<RectTransform> ();

		}

		void Start () {


		}

		void OnSceneChanged( SceneChangedInfo info ) {

			m_isScoreDrawingToggled = false;

		}

		void Update () {


			// open/close score view
			if(Input.GetKeyDown(singleton.toggleKey)) {
				if (GameManager.CanGameObjectsReadUserInput () && (NetworkStatus.IsClientConnected ()
					|| NetworkStatus.IsServerStarted ()) ) {
					// toggle score view
					m_isScoreDrawingToggled = ! m_isScoreDrawingToggled ;
				}
			}

			this.scoreCanvas.enabled = m_isScoreDrawingToggled;

			if (this.scoreCanvas.enabled) {
				UpdateUI ();
			}


		}

		public	static	bool	ShouldDisplayScore() {

		//	return GameManager.CanGameObjectsDrawGui () && Input.GetKey (KeyCode.Tab) && (NetworkStatus.IsClientConnected ()
		//	|| NetworkStatus.IsServerStarted ());

			return m_isScoreDrawingToggled ;
			
		}

		private	static	void	ConstructUI() {

			// destroy children
			foreach( var child in singleton.scoreCanvas.transform ) {
				Destroy (child as Object);
			}




		}

		private	static	void	UpdateUI() {

			// compare current positions with old one
			// if they are different, reconstruct UI, and then update it


			var widthPercentages = func_getColumnsWidthPercentages ();
			if (null == widthPercentages)
				return;

			// check if column count changed
			int newColumnCount = widthPercentages.Count ;
			if (m_currentColumnCount != newColumnCount) {
				// column count changed
				// reconstruct the whole UI
				m_tableEntries.ForEach( entry => { Destroy(entry); } );	// delete all entries
				m_tableEntries.Clear ();
				m_currentColumnCount = newColumnCount;

			//	Debug.Log ("Column count changed to " + newColumnCount + ", rect " + GetTableRect() + ", width " + GetTableWidth()
			//		+ ", height " + GetTableHeight() );
			}

//			int newRowCount = CalculateTableRowsCount ();
//			if (newRowCount != m_tableEntries.Count) {
//				// add or remove some rows
//				bool delete = newRowCount < m_tableEntries.Count ;
//				int count = Mathf.Abs (newRowCount - m_tableEntries.Count);
//
//				for (int i = 0; i < count; i++) {
//					if (delete) {
//						DestroyRow (m_tableEntries [i]);
//					} else {
//						m_tableEntries.Add (CreateRow (Vector2.zero, widthPercentages));
//					}
//				}
//
//				if(delete) {
//					m_tableEntries.RemoveRange( 0, count );
//				}
//			}

			Rect tableRect = GetTableRect ();

			// update rows
		//	var en = m_tableEntries.GetEnumerator();
			int i = 0;
			foreach(var entry in singleton.GetTableEntries()) {
			//	en.MoveNext ();

				if (i >= m_tableEntries.Count) {
					// not enough entries
					// create new entry
					var entryControl = CreateEntry();
					m_tableEntries.Add( entryControl );

					if (entry.isHeader) {
						// add click handler which will change sorting
						var pickup = entryControl.gameObject.AddComponent<Utilities.UIEventsPickup> ();
						pickup.onPointerClick += (arg) => {
							string s = entryControl.text ;
							if(s.EndsWith(singleton.sortingColumnAscendingTextSuffix))
								s = s.Remove( s.Length - singleton.sortingColumnAscendingTextSuffix.Length );
							else if(s.EndsWith(singleton.sortingColumnDescendingTextSuffix))
								s = s.Remove( s.Length - singleton.sortingColumnDescendingTextSuffix.Length );

							if (s == ScoreDrawer.SortingColumn)
								ScoreDrawer.DescendingSort = !ScoreDrawer.DescendingSort;	// only change order of sorting
							else
								ScoreDrawer.SortingColumn = s;	// change sorting column
						};
					}
				}

				// for each property, check if it should be changed

				var existingEntry = m_tableEntries [i];


				if (existingEntry.text != entry.text) {
					existingEntry.text = entry.text;
				}

				// rect is in canvas space - convert it to screen space
			//	Rect entryScreenRect = entry.rect;
			//	entryScreenRect.x += tableRect.x;
			//	entryScreenRect.y += tableRect.y;

			//	if (GetScreenRectOfRectTransform( existingEntry.rectTransform ) != entryScreenRect) {
			//	if( existingEntry.rectTransform.anchoredPosition != entry.rect.center || existingEntry.rectTransform.sizeDelta != entry.rect.size) {
					// reposition and change size
				existingEntry.rectTransform.anchoredPosition = new Vector2( entry.rect.center.x, - entry.rect.center.y) ;
				existingEntry.rectTransform.anchorMin = new Vector2 (0, 1);
				existingEntry.rectTransform.anchorMax = new Vector2 (0, 1);
			//	existingEntry.rectTransform.offsetMin = entry.rect.min ;
			//	existingEntry.rectTransform.offsetMax = entry.rect.max;
					existingEntry.rectTransform.sizeDelta = entry.rect.size;
			//	}

				if (entry.fontSize != existingEntry.fontSize) {
					existingEntry.fontSize = entry.fontSize;
				}

				if (entry.color != existingEntry.color) {
					existingEntry.color = entry.color;
				}


				i++;
			}

			// delete unnecessery entries
			int numNeededEntries = i ;
			for(int e = m_tableEntries.Count - 1; e >= numNeededEntries ; e--) {
				Destroy (m_tableEntries [e]);
			}
			m_tableEntries.RemoveRange (numNeededEntries, m_tableEntries.Count - numNeededEntries);


		//	Debug.Log ("UpdateUI finished - " + m_tableEntries.Count + " entries");

		}

		protected	static	Text	CreateEntry() {

			var go = Instantiate (singleton.tableEntryPrefab);
			go.transform.SetParent( singleton.scorePanel, true );
			return go.GetComponentInChildren<Text> ();

		}

		protected	static	void	DestroyRow( Text[] row ) {
			
			foreach(var t in row) { Destroy(t); }

		}

		public	static	Rect	GetTableRect() {
			
			if (singleton.scorePanel != null) {
				var rect = GetScreenRectOfRectTransform (singleton.scorePanel.transform as RectTransform);
				// margins
			//	rect.position += new Vector2 (5, 5);
			//	rect.size -= new Vector2 (5, 5);
				return rect;
			}

			int width = GetTableWidth ();
			int height = GetTableHeight ();
			return new Rect (Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height);
		}

		public	static	int	GetTableWidth() {
			if (singleton.scorePanel != null)
				return (int)GetTableRect ().width;
			return (int) (Screen.width * 0.75f) ; // 600;
		}

		public	static	int	GetTableHeight() {
			if (singleton.scorePanel != null)
				return (int)GetTableRect ().height;
			return (int) (GetTableWidth() * 9.0f / 16.0f) ; // 400;
		}

		public	virtual	IEnumerable<TableEntryData>	GetTableEntries() {

			int width = GetTableWidth ();
			int height = GetTableHeight ();

			width -= HorizontalMargin * 2;
			height -= VerticalMargin * 2;


			// gather necessery info
			var rowsPerTeam = func_getRowsPerTeam ();

			int numPlayers = rowsPerTeam.Sum (rpt => rpt.rows.Count);

			int numRows = numPlayers + rowsPerTeam.Count * 2;


			// from 0 to 4
			int spaceBetween = Mathf.FloorToInt (4 - numRows / 10.0f);
			if (spaceBetween < 0)
				spaceBetween = 0;

			int totalSpaceBetween = (numRows - 1) * spaceBetween;

			//	float labelHeightF = 18 * (height - totalSpaceBetween) / 400.0f ;
			//	if (numRows != 0)
			//		labelHeightF *= 20.0f / numRows;
			float labelHeightF = (height - totalSpaceBetween) ;
			if (numRows != 0)
				labelHeightF /= numRows;
			if (labelHeightF > 30)
				labelHeightF = 30;

			float fontSizeF = labelHeightF * 0.6f ; // * height / 400.0f ;
			if (fontSizeF > 15)
				fontSizeF = 15;

			//	if (numPlayers > 20) {
			// if num players is higher than 20, font size and label height need to be decreased

			//		fontSizeF *= 20.0f / numPlayers ;
			//		labelHeightF *= 20.0f / numPlayers ;
			//	}

			int fontSize = Mathf.FloorToInt (fontSizeF);


			var widthPercentages = func_getColumnsWidthPercentages ();
			var columnNames = func_getColumnsNames ();


			/*
			string headerStr = string.Format( "{0,-35} {1,-7} {2,-8} {3,-6}", new object[] {"name", "kills", "deaths", "ping"} ) ;
			// add on server: health, net id of net control object and playing object, status, ip
			if (NetworkManager.singleton.IsServer ()) {
				headerStr += string.Format ("{0,-9} {1,-7} {2,-7} {3,-15} {4,-15}", new object[] { "health", "NetControlObjectId", "PlayingObjectId", "status", "ip" });
			}
			headerStr += "\n";
			*/


		//	float x = rect.x;
		//	float y = rect.y;
			float x = HorizontalMargin;
			float y = VerticalMargin;
			TableEntryData entryData = new TableEntryData ();
			entryData.color = Color.black;
			entryData.fontSize = fontSize;
			entryData.isHeader = false;


			//	GUI.Label( new Rect( x, y, rect.width, labelHeightF ), "font size: " + fontSize + ", label height: " + labelHeightF, labelStyle );
			//	y += labelHeightF + spaceBetween;

			foreach(var pair in rowsPerTeam) {

				string team = pair.team;

				// team name
				entryData.color = Color.red;
				entryData.rect = new Rect (x, y, width, labelHeightF);
				entryData.text = team ;

				yield return entryData;

				y += labelHeightF + spaceBetween;

				// column header names
				entryData.isHeader = true;
				for (int j = 0; j < widthPercentages.Count; j++) {

					entryData.color = Color.red;
					entryData.text = columnNames [j];
					if (SortingColumn == entryData.text) {
						// add a little sign, so that we know this is the sorting column
						if (DescendingSort)
							entryData.text += singleton.sortingColumnDescendingTextSuffix;
						else
							entryData.text += singleton.sortingColumnAscendingTextSuffix;
					}
					entryData.rect = new Rect (x, y, width * widthPercentages [j], labelHeightF);

					yield return entryData;

					x += width * widthPercentages [j];
				}
				entryData.isHeader = false;

			//	x = rect.x ;
				x = HorizontalMargin;
				y += labelHeightF + spaceBetween;

				// rows

				SortRows( pair.rows );

				foreach (var row in pair.rows) {
					
					foreach (var e in this.GetRowEntries (row, x, y, labelHeightF, width, widthPercentages, fontSize))
						yield return e;
					
					y += labelHeightF + spaceBetween;
				}

			}


		}


		public	static	void	SortRows( List<object> rows ) {

			if (0 == rows.Count)
				return;

			int sortingColumnIndex = func_getColumnsNames ().IndexOf (SortingColumn);
			if (sortingColumnIndex < 0)
				return;

			// extracts comparison value from row
			System.Func<object, System.IComparable> valueSelector = (row) => func_getCompareValuesForSorting (row, sortingColumnIndex);

			if (null == valueSelector (rows [0]))	// function can not sort using this column
				return;


			System.Comparison<object> comparisonAscending = (p1, p2) => {
				return valueSelector (p1).CompareTo (valueSelector (p2));
			};
			System.Comparison<object> comparisonDescending = (p1, p2) => {
				return - valueSelector (p1).CompareTo (valueSelector (p2));
			};


			rows.Sort ( DescendingSort ? comparisonDescending : comparisonAscending );

		}

		public	static	System.IComparable	GetCompareValueForSorting (object row, int sortingColumnIndex) {
			
//			Player player = row as Player;
//
//			if (srtClmn == "kills")
//				return player.GetComponent<Score> ().NumKills;
//			else if (srtClmn == "deaths")
//				return player.GetComponent<Score> ().NumDeaths;
//			else if (srtClmn == "name")
//				return player.playerName;
//			else if (srtClmn == "ping")
//				return player.Ping;


			// return the value at specified index
			return (System.IComparable) func_getValuesForRow (row) [sortingColumnIndex];

		}


		void OnGUI() {
			
		//	if(ShouldDisplayScore()) {
		//		this.DrawScore ();
		//	}

		}

		void	DrawScore() {


			GUI.Box (GetTableRect(), "");
			//	GUILayout.BeginArea (rect);

			GUIStyle labelStyle = new GUIStyle (GUI.skin.label);
			labelStyle.fontStyle = FontStyle.Bold;

			foreach (var entryData in GetTableEntries()) {
				labelStyle.fontSize = entryData.fontSize;
				// how to set color ?
				GUI.Label( entryData.rect, entryData.text, labelStyle );
			}

			//	GUILayout.EndArea ();

		}

		//	public	void	DrawScoreForPlayer( Player netControl, 
		//		List<string> teamsFoundSoFar, List<string> teamStrings, string headerString ) {
		public	IEnumerable<TableEntryData>	GetRowEntries( object row, float x, float y, float labelHeight,
			int areaWidth, List<float> widthPercentages, int fontSize ) {

			TableEntryData entryData = new TableEntryData ();
			entryData.fontSize = fontSize;

			Player player = row as Player;
			bool isDead = false;
			if (player != null)
				isDead = null == player.PlayerGameObject;

			List<object> values = func_getValuesForRow (row);

			for (int i=0; i < widthPercentages.Count ; i++) {
				
				entryData.color = Color.white;
				if (isDead)
					entryData.color = Color.black;

				entryData.text = values [i].ToString ();
				
//				switch (i) {
//				case 0:
//					s += player.playerName;
//					break ;
//				case 1:
//					s += player.GetComponent<Score>().numKills;
//					break;
//				case 2:
//					s += player.GetComponent<Score>().numDeaths;
//					break;
//				case 3:
//					s += player.Ping;
//					break;
//				case 4:
//					s += player.health;
//					break;
//				case 5:
//					s += player.GetComponent<NetworkIdentity> ().netId.Value;
//					break;
//				case 6:
//					if (!isDead && player.PlayerGameObject != null)
//						s += player.PlayerGameObject.GetComponent<NetworkIdentity> ().netId.Value.ToString ();
//					break;
//				case 7:
//					s += player.status.ToString ();
//					break;
//				case 8:
//					if (player.conn != null)
//						s += player.conn.address.ToString ();
//					break;
//				}


				entryData.rect = new Rect (x, y, areaWidth * widthPercentages [i], labelHeight);

				yield return entryData;

				x += areaWidth * widthPercentages [i];
			}


			/*
			string s = "";
			if (isDead)
				s += "<color=black>";
			s += string.Format ("{0,-35} {1,-7} {2,-8} {3,-6}", new object[] {
				netControl.playerName,
				netControl.numKills,
				netControl.numDeaths,
				netControl.ping + " ms"
			});
			// add on server: health, net id of net control object, net id of playing object, status, ip
			if (NetworkManager.singleton.IsServer ()) {
				// {0,-9} {1,-7} {2,-7} {3,-15} {4,-15}

				Player player = NetworkManager.singleton.GetPlayerByName (netControl.playerName);
				string playingObjectIdStr = "" ;
				if (!isDead && netControl.playerGameObject != null)
					playingObjectIdStr = netControl.playerGameObject.GetComponent<NetworkIdentity> ().netId.Value.ToString ();
				string addressStr = "";
				if (player.conn != null)
					addressStr = player.conn.address.ToString ();

				s += string.Format ("{0,-9} {1,-7} {2,-7} {3,-15} {4,-15}", new object[] {
					netControl.health,
					netControl.GetComponent<NetworkIdentity>().netId.Value,
					playingObjectIdStr,
					player.status.ToString(),
					addressStr
				});

			}

			if (isDead)
				s += "</color>";

			int index = teamsFoundSoFar.FindIndex (teamName => teamName == netControl.team);
			if (index < 0) {
				teamsFoundSoFar.Add (netControl.team);
				teamStrings.Add ("<color=red>Team: " + netControl.team + "\n" + headerString + "</color>" + s + "\n");
			} else {
				// already found this team
				teamStrings [index] += s + "\n";
			}
			*/


		}


		public	static	List<RowsPerTeam>	GetRowsPerTeam () {

			var rowsPerTeam = new List<RowsPerTeam>();
			var spectatingPlayers = new List<object> ();

			foreach (var p in PlayerManager.players) {
				
				if (p.IsSpectator ()) {
					spectatingPlayers.Add (p);
				} else {
					int index = rowsPerTeam.FindIndex (r => r.team == p.Team);
					if (index < 0) {
						rowsPerTeam.Add (new RowsPerTeam (p.Team, new List<object> (){ p }));
					} else {
						rowsPerTeam [index].rows.Add (p);
					}
				}
			}

			if (spectatingPlayers.Count > 0) {
				rowsPerTeam.Add ( new RowsPerTeam( "Spectators", spectatingPlayers ) );	// special case for spectators
			}
			

			return rowsPerTeam;
		}

		public	static	List<float>	GetColumnWidthPercentages() {

			List<float> widthPercentages = null;

			if (NetworkStatus.IsClient ())
				widthPercentages = new List<float>(){ 0.4f, 0.15f, 0.15f, 0.3f };
			else if (NetworkStatus.IsServer ())
				widthPercentages = new List<float>(){ 0.25f, 0.05f, 0.05f, 0.05f, 0.05f, 0.05f, 0.05f, 0.225f, 0.225f };

			return widthPercentages;
		}

		public	static	List<string>	GetColumnNames() {

			if (NetworkStatus.IsClient ()) {
				return new List<string>(){ "name", "kills", "deaths", "ping" };
			} else if (NetworkStatus.IsServer ()) {
				return new List<string>() {"name", "kills", "deaths", "ping", "health", "net id",
					"obj id", "status", "ip"
				};
			}

			return null;
		}

		public	static	List<object>	GetValuesForRow( object row ) {

			Player player = row as Player ;

			uint playingObjectId = 0;
			if (player.PlayerGameObject != null)
				playingObjectId = player.PlayerGameObject.GetComponent<NetworkIdentity> ().netId.Value ;

			var values = new List<object> () { player.playerName, player.GetComponent<Score> ().NumKills,
				player.GetComponent<Score> ().NumDeaths, player.Ping
			};

			if (NetworkStatus.IsServer ()) {
				// add stuff for dedicated server
				values.AddRange( new object[] {
					player.health, player.netId.Value, playingObjectId, "", player.conn.address
				});
			}

			return values;
		}


		public	static	Rect	GetScreenRectOfRectTransform( RectTransform rectTransform )
		{
			Vector3[] corners = new Vector3[4];

			rectTransform.GetWorldCorners(corners);

			float xMin = float.PositiveInfinity;
			float xMax = float.NegativeInfinity;
			float yMin = float.PositiveInfinity;
			float yMax = float.NegativeInfinity;

			for (int i = 0; i < 4; i++)
			{
				// For Canvas mode Screen Space - Overlay there is no Camera; best solution I've found
				// is to use RectTransformUtility.WorldToScreenPoint) with a null camera.

				Vector3 screenCoord = RectTransformUtility.WorldToScreenPoint(null, corners[i]);

				if (screenCoord.x < xMin)
					xMin = screenCoord.x;
				if (screenCoord.x > xMax)
					xMax = screenCoord.x;
				if (screenCoord.y < yMin)
					yMin = screenCoord.y;
				if (screenCoord.y > yMax)
					yMax = screenCoord.y;
			}

			Rect result = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);

			return result;
		}

	}

}
