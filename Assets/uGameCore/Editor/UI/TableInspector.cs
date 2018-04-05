using UnityEngine;
using UnityEditor;
using uGameCore.Utilities.UI;
using UnityEngine.UI;

namespace uGameCore.Editor {

	[CustomEditor(typeof(Table))]
	[CanEditMultipleObjects]
	public class TableInspector : UnityEditor.Editor
	{

		int numOfRowsToSet = 0;



		public override void OnInspectorGUI()
		{

			if (null == (target as Table).GetComponent<ScrollRect> ()) {
				EditorGUILayout.HelpBox( "Table script should be attached to scroll view root game object", MessageType.Warning );
			}


			this.DrawDefaultInspector ();


			GUILayout.Space (15);


			if (GUILayout.Button ("Create column headers")) {

				foreach(Table table in this.targets) {
					table.CreateHeaders ();
				}

			}


			GUILayout.Space (5);


			EditorGUILayout.BeginHorizontal ();

			this.numOfRowsToSet = EditorGUILayout.IntField (this.numOfRowsToSet);

			if (GUILayout.Button ("Set number of rows")) {

				foreach(Table table in this.targets) {
					table.SetNumberOfRows (this.numOfRowsToSet);
				}

			}

			EditorGUILayout.EndHorizontal ();


			GUILayout.Space (5);


			if (GUILayout.Button ("Remove all rows")) {

				foreach(Table table in this.targets) {
					table.Clear ();
				}

			}


			GUILayout.Space (10);


			//	serializedObject.Update();
			//	EditorGUILayout.PropertyField(lookAtPoint);
			//	serializedObject.ApplyModifiedProperties();

		}

	}

}
