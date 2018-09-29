using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Utilities.UI {

	public class TableRow : MonoBehaviour {

		[HideInInspector]	[SerializeField]	internal	Table	table = null;
		public Table Table { get { return this.table; } }

		[HideInInspector]	[SerializeField]	internal	List<TableEntry> entries = new List<TableEntry>();
		public List<TableEntry> Entries { get { return this.entries; } }

		[HideInInspector]	[SerializeField]	internal	bool	isHeaderRow = false;
		public bool IsHeaderRow { get { return this.isHeaderRow; } }

		private	Color	m_originalImageColor = Color.white;
		public Color OriginalImageColor { get { return this.m_originalImageColor; } }

		private	Image	m_imageComponent = null;
		public	Image	ImageComponent {
			get {
				if (null == m_imageComponent) {
					m_imageComponent = this.GetComponent<Image> ();
					if (m_imageComponent)
						m_originalImageColor = m_imageComponent.color;	// remember original color
				}
				return m_imageComponent;
			}
		}



		void Awake ()
		{
			
		}


		public	TableEntry	FindEntryByColumnName( string columnName ) {

			// find column with this name
			int columnIndex = this.Table.columns.FindIndex( c => c.columnName == columnName );
			if (columnIndex < 0)
				return null;

			return this.Entries [columnIndex];
		}

	}

}
