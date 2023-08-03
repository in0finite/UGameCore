using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UGameCore.Editor {
	
	public class EditorMenu {

		[MenuItem("uGameCore/Grouping/Group all module parts")]
		public static void GroupModuleParts()
		{
			Utilities.GroupAllModules ();
		}

		[MenuItem("uGameCore/Grouping/Ungroup all module parts")]
		public static void UnGroupModuleParts()
		{
			Utilities.UnGroupAllModules ();
		}


		[MenuItem("uGameCore/Grouping/Group all menus and canvases")]
		public static void GroupMenusAndCanvases()
		{
			Utilities.GroupAllMenusAndCanvases ();
		}

		[MenuItem("uGameCore/Grouping/Ungroup all menus and canvases")]
		public static void UnGroupMenusAndCanvases()
		{
			Utilities.UnGroupAllMenusAndCanvases ();
		}


		[MenuItem("uGameCore/Module Scripts Window")]
		public static void OpenModuleScriptsWindow()
		{
			EditorWindow.GetWindow<ModuleScriptsWindow> ();
		}

	}

}
