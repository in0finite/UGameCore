using UnityEngine;
using UnityEditor;
using UGameCore.Utilities.UI;

namespace UGameCore.Editor {
	
	[CustomEditor(typeof(TabView))]
	[CanEditMultipleObjects]
	public class TabViewInspector : UnityEditor.Editor
	{

		string	addTabText = "";


		public override void OnInspectorGUI()
		{

			this.DrawDefaultInspector ();

		//	EditorGUILayout.PropertyField( serializedObject.FindProperty("m_tabsForInspector"), new GUIContent() );

			GUILayout.Space (15);

			if (GUILayout.Button ("Refresh list of tabs")) {

				foreach(TabView tabView in this.targets) {
					var list = tabView.GetTabsList ();
					list.Clear ();
					list.AddRange( tabView.TabsInChildren );
				//	tabView.ApplyTabsFromList ();
					UGameCore.Utilities.Utilities.MarkObjectAsDirty( tabView );
				}

			}

			if (GUILayout.Button ("Apply")) {
				
				foreach(TabView tabView in this.targets) {
					tabView.ApplyTabsFromList ();
				}

			}

			if (GUILayout.Button ("Delete all tabs")) {

				foreach(TabView tabView in this.targets) {
					tabView.DeleteAllTabsAndPanels ();
				}

			}

			EditorGUILayout.BeginHorizontal ();

			this.addTabText = EditorGUILayout.TextField (this.addTabText);

			if (GUILayout.Button ("Add tab")) {

				foreach(TabView tabView in this.targets) {
					tabView.AddTab (this.addTabText);
					tabView.UpdatePositionsOfTabs ();
				}

			}

			EditorGUILayout.EndHorizontal ();

		}

	}

}
