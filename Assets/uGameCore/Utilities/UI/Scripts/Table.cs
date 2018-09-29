using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using uGameCore;
using System.Linq;

namespace uGameCore.Utilities.UI {
	
	public class Table : MonoBehaviour, ILayoutElement {

		// TODO: sort rows, 


		public enum ColumnWidthType {
			Percentage,
			Absolute
		}

		/// <summary>
		/// Contains parameters for a column.
		/// </summary>
		[Serializable]
		public class Column
		{
			public	ColumnWidthType	widthType = ColumnWidthType.Percentage ;
			public	float	widthPercentage = 0.2f;
			public	float	absoluteWidth = 80f ;
			public	string	columnName = "";
			/// <summary> Assign this function if you want to enable sorting for this column. </summary>
			public	Func<TableRow, int, IComparable> compareValueSelector = null ;

			public	float	GetWidth( Table table ) {
				if (this.widthType == ColumnWidthType.Absolute) {
					return this.absoluteWidth;
				} else if (this.widthType == ColumnWidthType.Percentage) {
					return table.rectTransform.rect.width * this.widthPercentage;
				}
				return 0f;
			}

			public	Column	Clone() {
				return (Column) this.MemberwiseClone ();
			}

		}


		private	RectTransform	rectTransform { get { return this.GetComponent<RectTransform>(); } }

		/// <summary>
		/// Place where all table rows are put in.
		/// </summary>
		public	RectTransform	Container { get { return this.rectTransform; } }

		[HideInInspector]	[SerializeField]	private	List<TableRow>	m_rows = new List<TableRow>();

		[HideInInspector]	[SerializeField]	TableRow	m_headerRow = null;

		private	TableRow	m_selectedRow = null;
		public TableRow SelectedRow {
			get {
				return m_selectedRow;
			}
			set {
				this.SelectRow (value, false);
			}
		}

		/// <summary>
		/// Gets all rows in content's first-level children.
		/// </summary>
		public List<TableRow> GetRowsInChildren () {
			var rows = new List<TableRow> (this.Container.transform.childCount);
			foreach (Transform child in this.Container.transform) {
				var row = child.GetComponent<TableRow> ();
				if (row != null)
					rows.Add (row);
			}
			return rows;
		}

		public	List<TableRow>	GetAllRows() {
			return m_rows;
		}

		public	int	RowsCount { get { return m_rows.Count; } }

		public	List<Column>	columns = new List<Column> (0);

		public	GameObject	tableEntryPrefab = null;

		public	GameObject	tableRowPrefab = null;

		public	int rowHeight = 30;

		public	bool	updateParentDimensions = false;

		public	Vector4	headerRowColorDelta = new Vector4 (-1, -1, -1, 1) * 0.12f;
		public	Vector4	selectedRowColorDelta = new Vector4 (-0.4f, -0.4f, -0.2f, 0.5f);


		public	event	Action<TableEntry>	onEntryCreated = delegate {};
		public	event	Action	onColumnHeadersCreated = delegate {};




		private	Table()
		{
			// add some columns
			for (int i = 0; i < 3; i++) {
				Column column = new Column();
				column.columnName = "Column " + i;
				column.widthType = ColumnWidthType.Absolute;
				column.absoluteWidth = 100;
				this.columns.Add( column );
			}

		}


		public	Column	GetColumnByName (string columnName) {

			return this.columns.Find (c => c.columnName == columnName);

		}

		public	IEnumerable<TableEntry>	GetAllEntriesInColumn( string columnName ) {

			int columnIndex = this.columns.FindIndex (c => c.columnName == columnName);
			if (columnIndex < 0)
				yield break;

			foreach (var row in m_rows.WhereAlive()) {
				yield return row.Entries [columnIndex];
			}

		}

		public	float	GetTotalColumnsWidth() {
			float width = 0f;
			for (int i = 0; i < this.columns.Count; i++) {
				width += this.columns [i].GetWidth (this);
			}
			return width;
		}

		public	float	CalculateTableHeight() {

			float height = 0f;

			if (m_headerRow)
				height += this.rowHeight;
			
			height += m_rows.Count * this.rowHeight;

			return height;
		}


		public void CalculateLayoutInputHorizontal () {  }

		public void CalculateLayoutInputVertical () {  }

		public float minWidth { get { return 0; } }

		public float preferredWidth { get { return this.GetTotalColumnsWidth (); } }

		public float flexibleWidth { get { return -1; } }

		public float minHeight { get { return 0; } }

		public float preferredHeight { get { return this.CalculateTableHeight (); } }

		public float flexibleHeight { get { return -1; } }

		public int layoutPriority { get { return 0; } }


		public	virtual	void	SetRowTransform( TableRow row, int rowIndex ) {

			float verticalOffset = 0f;

			// vertical offset of the first row
			float startingVerticalOffset = 0f; // -this.rowHeight / 2f;

			if (row.IsHeaderRow) {
				verticalOffset = startingVerticalOffset;
			} else {
				if (m_headerRow) {
					// leave some space for headers
					verticalOffset = startingVerticalOffset - this.rowHeight * (rowIndex + 1);
				} else {
					verticalOffset = startingVerticalOffset - this.rowHeight * rowIndex;
				}
			}


			float width = this.GetTotalColumnsWidth ();

			// set anchor to upper left corner
			row.GetRectTransform ().anchorMin = new Vector2 (0f, 1f);
			row.GetRectTransform ().anchorMax = new Vector2 (0f, 1f);

			// set offset from upper left corner
			row.GetRectTransform ().offsetMax = new Vector2 (width, verticalOffset);
			row.GetRectTransform ().offsetMin = new Vector2 (0f, verticalOffset - this.rowHeight);

			// set size
			row.GetRectTransform().sizeDelta = new Vector2( width, this.rowHeight );

			MySetDirty (row.GetRectTransform ());

		}

		/*
		public	virtual	void	SetTableTransform() {

			// set size of table
			// it's left and top position will remain the same, only width and height will be changed


			float tableWidth = this.GetTotalColumnsWidth();
			float tableHeight = this.CalculateTableHeight ();
		//	Vector2 tableSizeNormalized = this.Container.NormalizePositionRelativeToParent (new Vector2 (tableWidth, tableHeight));
		//	Vector2 parentSize = this.Container.GetParentDimensions ();

		//	Rect rect = this.Container.GetRect();

		//	float top = parentSize.y - rect.yMin;
		//	float left = rect.xMin;
		//	float offsetToTop = parentSize.y - top;
		//	Vector2 upperLeft = new Vector2 (left, top);
		//	Vector2 upperLeftNormalized = this.Container.NormalizePositionRelativeToParent (upperLeft);

		//	this.Container.anchorMin = this.Container.NormalizePositionRelativeToParent( new Vector2 (left, top - tableHeight) );
		//	this.Container.anchorMax = this.Container.NormalizePositionRelativeToParent( new Vector2 (left + tableWidth, top) );
		//	this.Container.offsetMin = this.Container.offsetMax = Vector2.zero;

			// set anchors to upper left, and adjust offsets
			this.Container.anchorMin = new Vector2 (0f, 1f);
			this.Container.anchorMax = new Vector2 (0f, 1f);
		//	this.Container.offsetMin = new Vector2 (upperLeft.x, - offsetToTop - tableHeight );
		//	this.Container.offsetMax = new Vector2 (upperLeft.x + tableWidth, - offsetToTop );
		//	this.Container.sizeDelta = new Vector2 (tableWidth, tableHeight);

		//	this.Container.SetRectAndAdjustAnchors (new Rect (left, top - tableHeight, tableWidth, tableHeight));

			this.Container.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, tableWidth);
			this.Container.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, tableHeight);

			MySetDirty (this.Container);

		//	Debug.LogFormat ("tableWidth {0} tableHeight {1} parentSize {2} top {3} left {4}", tableWidth, tableHeight, parentSize, top, left);

		}
		*/

		public	virtual	void	UpdateParentDimensions() {

			if (this.transform.parent) {
				var rt = this.transform.parent.GetRectTransform ();
				float width = this.GetTotalColumnsWidth ();
				float height = this.CalculateTableHeight ();
				rt.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, width);
				rt.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, height);
				MySetDirty (rt);
			}

		}


		/// <summary>
		/// Updates the table.
		/// </summary>
		public	void	UpdateTable () {

			m_rows.RemoveAllDeadObjects ();

			// first update parent dimensions, and after that table transform
			if (this.updateParentDimensions) {
				this.UpdateParentDimensions ();
			}

		//	this.SetTableTransform ();

			for (int i = 0; i < m_rows.Count; i++) {
				this.UpdateRow (m_rows [i], i);
			}

			if (m_headerRow) {
				this.UpdateRow (m_headerRow, 0);
			}

			MySetDirty (this);
		}

		/// <summary>
		/// Updates all entries in a row.
		/// </summary>
		public	void	UpdateRow (TableRow row, int rowIndex) {

			row.Entries.RemoveAllDeadObjects ();
			MySetDirty (row);

			// create entries if they are not created
			int numEntriesToCreate = this.columns.Count - row.Entries.Count ;
			for (int i = 0; i < numEntriesToCreate; i++) {
				this.CreateEntry( row );
			}

			// TODO: delete extra entries, or better yet, rearrange columns if needed


			// set row transform
			this.SetRowTransform( row, rowIndex );

			// set row color
			if (row.ImageComponent) {
				Color color = row.OriginalImageColor;

				if (m_selectedRow == row) {
					// this row is selected
					color += (Color) this.selectedRowColorDelta;
				} else if (m_headerRow == row) {
					// this row is header row
					color += (Color) this.headerRowColorDelta;
				}

				// color should be normalized (all values should be between 0 and 1)
				color.NormalizeIfNeeded ();

				if (row.ImageComponent.color != color)
					row.ImageComponent.color = color;
			}

			// update entries
			float leftCoordinate = 0f;
			for (int i = 0; i < row.Entries.Count; i++) {
				UpdateTableEntry (row, rowIndex, i, leftCoordinate);
				leftCoordinate += this.columns [i].GetWidth (this);
			}

		}

		public	void	UpdateRow (TableRow row) {

			if (null == row)
				return;

			if (m_headerRow == row) {
				this.UpdateRow (row, 0);
				return;
			}

			int index = m_rows.IndexOf (row);
			if (index < 0)
				return;

			this.UpdateRow (row, index);

		}


		private	void	Rebuild () {
			
			// destroy entries for each row

			// destroy rows

			// destroy headers


			// create headers

			// create rows

			// create entries for each row


		}


		/// <summary>
		/// Creates entry out of prefab.
		/// </summary>
		public	virtual	TableEntry	CreateEntry ( TableRow row ) {
			
			var go = this.tableEntryPrefab.InstantiateAsUIElement (row.transform);

			var entry = go.AddComponentIfDoesntExist<TableEntry> ();
			entry.tableRow = row;

			// add it to list of entries
			row.Entries.Add (entry);

			MySetDirty (row.transform);
			MySetDirty (go);
			MySetDirty (entry);
			MySetDirty (row);

			return entry;
		}

		private	void	SetEntryPositionAndSize (RectTransform rt, Column column) {



		}

		/// <summary>
		/// Updates position, size and name of table entry.
		/// </summary>
		public	virtual	void	UpdateTableEntry ( TableRow row, int rowIndex, int columnIndex, float leftCoordinate ) {

			var entry = row.Entries [columnIndex];
			var column = this.columns [columnIndex];

			// set it's position and dimensions
			float entryWidth = column.GetWidth (this);
		//	float top = this.GetRowTopCoordinate (row, rowIndex);
			float top = this.rowHeight;
			float bottom = top - this.rowHeight ;
			entry.GetRectTransform().SetRectAndAdjustAnchors( new Rect(leftCoordinate, bottom, entryWidth, this.rowHeight) );

			MySetDirty (entry.GetRectTransform ());

			// set game object's name
			if (entry.gameObject.name != column.columnName) {
				entry.gameObject.name = column.columnName;
				MySetDirty (entry.gameObject);
			}

			if (row.IsHeaderRow) {
				// set entry's text
				var textComponent = entry.textComponent;
				if (textComponent) {
					textComponent.text = column.columnName;
					MySetDirty (textComponent);
				}
			}

		}

		/// <summary>
		/// Retreives text from attached Text component.
		/// </summary>
		public	string	GetEntryText ( TableRow row, int columnIndex ) {

			var textComponent = row.entries [columnIndex].textComponent;

			if (textComponent != null)
				return textComponent.text;

			return "";
		}

		public	TableEntry	GetEntry( int rowIndex, int columnIndex ) {

			return this.GetAllRows () [rowIndex].Entries [columnIndex];

		}


//		public	void	SetColumns( List<Column> newColumns ) {
//
//			if (newColumns == this.columns)
//				return;
//
//			foreach (var newColumn in newColumns) {
//
//				// find this column by name
//				int columnIndex = this.columns.FindIndex( c => c.columnName == newColumn.columnName );
//
//				if (columnIndex < 0) {
//					// column with this name doesn't exist
//					// insert it
//
//				} else {
//					// column with this name exists
//					// replace it to match index
//
//				}
//
//			}
//
//		}

		/// <summary>
		/// Sets width of each column based on it's text. It does not update table.
		/// </summary>
		public	void	SetColumnWidthsBasedOnText() {

			if (null == m_headerRow)
				return;

			for (int i = 0; i < this.columns.Count; i++) {
				var textComponent = m_headerRow.Entries [i].textComponent;

				float preferredWidth = textComponent.preferredWidth;
				float preferredHeight = textComponent.preferredHeight;

				if (preferredHeight > 0) {
					
					// multiply column width by ratio between row height and preffered height => this will maintain aspect ratio of text component
					float columnWidth = preferredWidth * this.rowHeight / (float)preferredHeight;

					this.columns [i].widthType = ColumnWidthType.Absolute;
					this.columns [i].absoluteWidth = columnWidth;
				}
			}

			MySetDirty (this);
		}

		public	void	ResizeColumnsToFitParent() {

			float parentWidth = this.Container.GetParentDimensions ().x;
			float totalColumnsWidth = this.GetTotalColumnsWidth ();

			if (0 == totalColumnsWidth)
				return;

			float multiplier = parentWidth / totalColumnsWidth;

			this.StretchColumns (multiplier);

		}

		public	void	StretchColumns( float multiplier ) {

			foreach (var column in this.columns) {
				float currentWidth = column.GetWidth (this);

				column.widthType = ColumnWidthType.Absolute;
				column.absoluteWidth = currentWidth * multiplier;
			}

			MySetDirty (this);
		}


		/// <summary>
		/// Adds new row to the table.
		/// </summary>
		public	TableRow	AddRow () {
			
			TableRow row = this.CreateRow ();

			m_rows.Add (row);

			MySetDirty (this);

			return row;
		}

		public	void	RemoveRow (int rowIndex) {

			var rows = this.GetAllRows ();

			var row = rows [rowIndex];

			DestroyRow (row);

		//	m_rows.RemoveAt (rowIndex);

		}

		private	void	DestroyRow (TableRow row) {

			// destroy row's game object - it will also destroy all it's entries
			MyDestroy( row.gameObject );

			MySetDirty (this.Container.transform);

		}

		public	void	SelectRow (TableRow newSelectedRow, bool updateAffectedRows) {

			if (newSelectedRow != null && newSelectedRow.Table != this)	// row doesn't belong to this table
				return;

			if (newSelectedRow != null && newSelectedRow.IsHeaderRow)	// header row can not be selected
				return;

			if (newSelectedRow == m_selectedRow)
				return;

			var oldSelectedRow = m_selectedRow;

			m_selectedRow = newSelectedRow;

			if (updateAffectedRows) {
				
				if (oldSelectedRow)
					this.UpdateRow (oldSelectedRow);
				if (newSelectedRow)
					this.UpdateRow (newSelectedRow);
			}

		}

		/// <summary>
		/// Removes all rows from table.
		/// </summary>
		public	void	Clear () {

			foreach (var row in m_rows.WhereAlive ()) {
				DestroyRow (row);
			}

			m_rows.Clear ();

			MySetDirty (this);
		}

		public	void	EnsureNumberOfRows (int numberOfRows)
		{
			m_rows.RemoveAllDeadObjects ();

			int numRowsToAdd = numberOfRows - this.GetAllRows ().Count;

			for (int i = 0; i < numRowsToAdd; i++) {
				this.AddRow ();
			}

			MySetDirty (this);

		}

		/// <summary>
		/// Adds or removes rows from table, so that new number of rows is equal to specified value.
		/// </summary>
		public	void	SetNumberOfRows (int numberOfRows)
		{
			m_rows.RemoveAllDeadObjects ();

			var rows = m_rows;

			int numToDelete = rows.Count - numberOfRows ;
			int numToAdd = - numToDelete ;

			for (int i = 0; i < numToDelete; i++) {
				DestroyRow (rows [rows.Count - 1 - i]);
			}

			for (int i = 0; i < numToAdd; i++) {
				this.AddRow ();
			}

			MySetDirty (this);
		}


		/// <summary>
		/// Creates one row in a table. It creates entries for each column.
		/// </summary>
		protected	virtual	TableRow	CreateRow () {

			GameObject rowGameObject = this.tableRowPrefab.InstantiateAsUIElement (this.Container.transform);

			TableRow row = rowGameObject.AddComponentIfDoesntExist<TableRow> ();
			row.table = this;

			// select row when it is clicked
			rowGameObject.AddComponentIfDoesntExist<UIEventsPickup>().onPointerClick += (obj) => {
				this.SelectRow( row, true );
			};


			// create entries
			for (int i = 0; i < this.columns.Count; i++) {
				
				var column = this.columns [i];

				TableEntry entry = this.CreateEntry (row);

				var entryGameObject = entry.gameObject;

				entryGameObject.name = column.columnName;

				// add layout element if it doesn't exist
			//	if (null == entryGameObject.GetComponentInChildren<ILayoutElement> ())
			//		entryGameObject.AddComponent<LayoutElement> ();
				

				MySetDirty (entryGameObject);
				MySetDirty (entryGameObject.GetRectTransform ());


				// invoke event
				onEntryCreated (entry);
			}


			MySetDirty (this.Container.transform);
			MySetDirty (rowGameObject);
			MySetDirty (row);
			MySetDirty (row.GetRectTransform ());


			return row;
		}


		/// <summary>
		/// Creates header row if it doesn't exist.
		/// </summary>
		public	virtual	void	CreateHeader () {
			
			if (null == m_headerRow) {
				// headers row doesn't exist
				// create it

				m_headerRow = this.CreateRow ();
				m_headerRow.isHeaderRow = true;

				m_headerRow.gameObject.name = "Table header";

				// make it the first child
				m_headerRow.transform.SetAsFirstSibling ();

				MySetDirty (m_headerRow);
				MySetDirty (m_headerRow.gameObject);
				MySetDirty (m_headerRow.transform.parent);

				onColumnHeadersCreated ();
			}

		}

		/// <summary>
		/// Gets the row which represents table header.
		/// </summary>
		public	TableRow	GetHeaderRow () {

			return m_headerRow;

		}

		public	void	DestroyHeader () {
			
			if (m_headerRow != null) {
				MyDestroy (m_headerRow.gameObject);
				MySetDirty (this.Container.transform);
				m_headerRow = null;
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

		protected	internal	static	void	MySetDirty( UnityEngine.Object obj ) {

			if (Application.isEditor && !Application.isPlaying) {
				Utilities.MarkObjectAsDirty (obj);
			}

		}



		void Awake ()
		{


		}

		void Start ()
		{
			
		}


	}

}
