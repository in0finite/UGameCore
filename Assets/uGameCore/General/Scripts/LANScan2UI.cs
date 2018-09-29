using UnityEngine;
using uGameCore.Utilities.UI;
using System.Linq;
using System.Collections.Generic;

namespace uGameCore {

	/// <summary>
	/// Starts listening on LAN when specified tab in a TabView is activated, and populates table with found servers.
	/// </summary>
	public class LANScan2UI : MonoBehaviour
	{

		public	TabView	tabView = null;
		public	string	tabName = "LAN";

		public	Table	table = null;

		[Range(1, 10)]
		public	float	refreshTime = 4f ;

		private	readonly	string	delayedStopMethodName = "DelayedStop";

		public	static	LANScan2UI	singleton { get ; private set ; }

		private	static	List<NetBroadcast.BroadcastData>	m_dataToAddToTable = new List<NetBroadcast.BroadcastData>();



		void Awake ()
		{
			if (null == singleton)
				singleton = this;

		}

		void Start ()
		{
			// register to tab-switched event
			if (this.tabView) {
				this.tabView.onSwitchedTab += this.OnTabSwitched ;
			}

			// register to broadcast received event
			NetBroadcast.onReceivedBroadcast += this.OnReceivedBroadcast ;

			if (this.table) {
				AdjustColumnWidthsWhenTableBecomesActive (this.table);
			}

		}

		public	static	void	StopListeningLater() {
			
			singleton.CancelInvoke (singleton.delayedStopMethodName);
			singleton.Invoke (singleton.delayedStopMethodName, singleton.refreshTime);

		}

		private void DelayedStop() {

			if (NetBroadcast.IsListening ())
				NetBroadcast.StopBroadcastingAndListening ();

		}

		/// <summary>
		/// Updates the table in the next frame to fix the bug in unity UI, which doubles the width of table parent.
		/// </summary>
		public	static	void	UpdateTableLater( Table table ) {

			singleton.StartCoroutine( UpdateTableCoroutine(table) );

		}

		private	static	System.Collections.IEnumerator	UpdateTableCoroutine(Table table) {

			yield return null;

			if (table)
				table.UpdateTable ();
		}

		public	static	void	AdjustColumnWidthsWhenTableBecomesActive (Table table) {

			singleton.StartCoroutine (CoroutineAdjustColumnWidthsWhenTableBecomesActive (table));

		}

		private	static	System.Collections.IEnumerator	CoroutineAdjustColumnWidthsWhenTableBecomesActive (Table table) {

			// without pausing one frame, it will not work when called from Start()
			yield return null;

			while (true) {

				if (null == table)
					break;

				if (table.gameObject.activeSelf && table.gameObject.activeInHierarchy) {
					AdjustColumnWidths (table);
					table.UpdateTable ();
					break;
				}

				yield return null;
			}

		}


		void OnTabSwitched() {

			if (this.tabView.ActiveTab && this.tabView.ActiveTab.tabButtonText == this.tabName) {
				// our tab was opened
				if (!NetBroadcast.IsListening ()) {
					// clear table
					if (this.table) {
						this.table.Clear ();
						this.table.UpdateTable ();
						UpdateTableLater (this.table);
					}
					// start listening
					NetBroadcast.StartListening ();
					// stop after some time
					StopListeningLater ();
				}
			}

		}

		void OnReceivedBroadcast( NetBroadcast.BroadcastData data ) {

			m_dataToAddToTable.Add (data);

		}


		void Update ()
		{

			if (null == this.table) {
				m_dataToAddToTable.Clear ();
				return;
			}

			if (this.table.gameObject.activeInHierarchy && this.table.gameObject.activeSelf) {
				// only add data to table if it is active

				foreach (var data in m_dataToAddToTable) {
					HandleBroadcastData (this.table, data);
				}

				m_dataToAddToTable.Clear ();
			}

		}


		public	static	void	HandleBroadcastData( Table table, NetBroadcast.BroadcastData data ) {


			EnsureColumnsMatchBroadcastData( table, data );

			// try to find this server in a table
			TableRow rowWithServer = FindRowWithServer( table, data );

			if (rowWithServer) {
				// this server already exists in table
				// update it's values

				PopulateTableRow (rowWithServer, data);

			} else {
				// this server doesn't exist
				// add it to table

				rowWithServer = table.AddRow ();

				table.UpdateRow (rowWithServer);

				PopulateTableRow (rowWithServer, data);
			}

			table.UpdateTable ();

		}


		public	static	void	EnsureColumnsMatchBroadcastData( Table table, NetBroadcast.BroadcastData broadcastData ) {
			
			var newColumnNames = new List<string>(1 + broadcastData.KeyValuePairs.Count);
			newColumnNames.Add ("IP");
			newColumnNames.AddRange (broadcastData.KeyValuePairs.Select (pair => pair.Key));


			bool sameColumns = table.columns.Select (c => c.columnName).SequenceEqual (newColumnNames);

//			for (int i = 0; i < table.columns.Count; i++) {
//				if (table.columns [i].columnName != newColumnNames [i]) {
//					sameColumns = false;
//					break;
//				}
//			}

			if (!sameColumns) {
				// we'll make this easy: rebuild the whole table :D

				// first, save all table data
				var allData = new List<NetBroadcast.BroadcastData> (table.RowsCount);
				foreach (var row in table.GetAllRows().WhereAlive()) {
					allData.Add (GetBroadcastDataFromRow (row));
				}

				// now rebuild the table

				table.Clear();
				table.DestroyHeader ();

				// create new columns
				var newColumns = new List<Table.Column>();
				foreach (var columnName in newColumnNames) {
					var column = new Table.Column ();
					column.columnName = columnName;
					newColumns.Add (column);
				}
				table.columns = newColumns;
				table.CreateHeader ();

				// adjust column widths
				AdjustColumnWidths( table );

				// restore saved data
				foreach (var rowData in allData) {
					var row = table.AddRow ();
					PopulateTableRow (row, rowData);
				}

				// update table
				table.UpdateTable ();

			}

		}

		public	static	void	AdjustColumnWidths( Table table ) {

		//	if (null == table.GetHeaderRow ())
		//		return;
			
			// make sure header row is updated
		//	table.UpdateRow( table.GetHeaderRow() );

			// now, do some adjustments

			string[] columnsToAdjust = new string[]{"IP", "Port", "Players", "Map"};
			int[] columnWidths = new int[]{140, 60, 60, 140};

			for (int i = 0; i < columnsToAdjust.Length; i++) {
				var column = table.GetColumnByName( columnsToAdjust[i] );
				if(column != null) {
					column.widthType = Table.ColumnWidthType.Absolute;
					column.absoluteWidth = columnWidths [i];
				}
			}

			float tableWidth = table.GetTotalColumnsWidth ();
			float scrollViewWidth = table.transform.parent.parent.parent.GetRectTransform ().GetRect ().width;
			if (tableWidth < scrollViewWidth) {
				// stretch columns so that table width is equal to scrollview width
				if (tableWidth > 0)
					table.StretchColumns (scrollViewWidth / tableWidth);
			}


		}

		public	static	NetBroadcast.BroadcastData	GetBroadcastDataFromRow( TableRow row ) {
			
			string fromAddress = "";
			Dictionary<string, string> dict = new Dictionary<string, string> ();

			TableEntry entryIP = row.FindEntryByColumnName ("IP");
			if (entryIP)
				fromAddress = entryIP.entryText;

			for (int i = 0; i < row.Table.columns.Count; i++) {
				var column = row.Table.columns [i];
				if (column.columnName == "IP")
					continue;
				var entry = row.Entries [i];
				dict.AddOrSet (column.columnName, entry.entryText);
			}


			var broadcastData = new NetBroadcast.BroadcastData (fromAddress, dict);
			return broadcastData;
		}

		public	static	TableRow	FindRowWithServer( Table table, NetBroadcast.BroadcastData broadcastData ) {

			TableEntry entry = table.GetAllEntriesInColumn ("IP").FirstOrDefault( e => e.entryText == broadcastData.FromAddress );
			if (entry)
				return entry.TableRow;
			
			return null;
		}

		public	static	void	PopulateTableRow( TableRow row, NetBroadcast.BroadcastData data ) {
			
			PopulateTableEntry (row, "IP", data.FromAddress);

			foreach (var pair in data.KeyValuePairs) {
				PopulateTableEntry (row, pair.Key, pair.Value);
			}

		}

		public	static	void	PopulateTableEntry( TableRow row, string columnName, string value ) {
			
			TableEntry entry = row.FindEntryByColumnName (columnName);
			if (entry)
				entry.entryText = value;

		}

	}

}
