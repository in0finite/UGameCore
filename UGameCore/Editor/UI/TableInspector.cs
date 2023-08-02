using UnityEngine;
using UnityEditor;
using UGameCore.Utilities.UI;
using UnityEngine.UI;

namespace UGameCore.Editor {

	[CustomEditor(typeof(Table))]
	[CanEditMultipleObjects]
	public class TableInspector : UnityEditor.Editor
	{

		int numOfRowsToSet = 0;



		public override void OnInspectorGUI()
		{
			
			this.DrawDefaultInspector ();


			GUILayout.Space (25);


			if (this.targets.Length == 1) {
				Table table = (Table)this.targets [0];
				EditorGUILayout.LabelField ("Rows: " + table.RowsCount + " Columns: " + table.columns.Count);
				if (table.GetHeaderRow () != null)
					EditorGUILayout.LabelField ("Has header");
				else
					EditorGUILayout.LabelField ("No header");
			}


			GUILayout.Space (10);

			if (GUILayout.Button ("Update table")) {
				foreach(Table table in this.targets) {
					table.UpdateTable ();
				}
			}

			if (GUILayout.Button ("Create column header")) {
				foreach(Table table in this.targets) {
					table.CreateHeader ();
					table.UpdateTable ();
				}
			}

			if (GUILayout.Button ("Delete column header")) {
				foreach(Table table in this.targets) {
					table.DestroyHeader ();
					table.UpdateTable ();
				}
			}

			if (GUILayout.Button ("Set column widths based on text")) {
				foreach(Table table in this.targets) {
					table.SetColumnWidthsBasedOnText ();
					table.UpdateTable ();
				}
			}

			if (GUILayout.Button ("Resize columns to fit parent")) {
				foreach(Table table in this.targets) {
					table.ResizeColumnsToFitParent ();
					table.UpdateTable ();
				}
			}


			EditorGUILayout.BeginHorizontal ();

			this.numOfRowsToSet = EditorGUILayout.IntField (this.numOfRowsToSet);

			if (GUILayout.Button ("Set number of rows")) {

				foreach(Table table in this.targets) {
					table.SetNumberOfRows (this.numOfRowsToSet);
					table.UpdateTable ();
				}

			}

			EditorGUILayout.EndHorizontal ();


			if (GUILayout.Button ("Delete all rows")) {

				foreach(Table table in this.targets) {
					table.Clear ();
					table.UpdateTable ();
				}

			}


			GUILayout.Space (10);

		}

	}

}
