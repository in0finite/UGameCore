using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace uGameCore.Editor {
	
	public class EditorMenu {


		[MenuItem("uGameCore/Setup/Setup guide")]
		public static void Setup()
		{
			var window = EditorWindow.GetWindow<SetupWindow> ();
			window.position = new Rect (new Vector2( 200, 200 ), new Vector2 (650, 600));
		}

		[MenuItem("uGameCore/Setup/One click setup")]
		public static void OneClickSetup()
		{
			if (EditorUtility.DisplayDialog ("One click setup", "Are you sure ?\nThe setup will create new scenes, " +
			   "add them to build settings, and create objects inside those scenes.\nAfter that, you will be able " +
			   "to start the game.", "Ok", "Cancel")) {
				Editor.OneClickSetup.Setup ();
			}
		}


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
