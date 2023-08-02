using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace UGameCore.Editor.Maps {
	
	public class MapManagement {


//		[MenuItem("uGameCore/Maps/Map cycle to build settings")]
//		private static void MapCycleToBuildSettings()
//		{
//
//			CheckSingleton ();
//
//			var singleton = FindSingleton ();
//			var mapNames = singleton.mapCycleList.ToList ();
//
//			// this can't be done, because EditorBuildSettings requires path to the scene, not scene name
//			// to fix this, map cycle should store paths, not names - this requires some changes in setup, among other things
//
//			var newSceneSettings = mapNames.Select (m => new EditorBuildSettingsScene (m, true));
//
//			//EditorBuildSettings.scenes = ;
//
//		}

		[MenuItem("uGameCore/Maps/Build settings to map cycle")]
		public static void BuildSettingsToMapCycleMenuFunction()
		{

			if (!EditorUtility.DisplayDialog ("Confirm", "This will assign all scenes from build settings to map cycle. " +
				"It will skip first 2 scenes, because they are startup and offline scene. Do you want to continue ?",
				"Ok",
				"Cancel"
			)) {
				return;
			}

			BuildSettingsToMapCycle ();

		}

		public static void BuildSettingsToMapCycle()
		{
			
			var singleton = Utilities.FindSingletonOrThrow<UGameCore.MapManagement.MapCycle> ();

			var oldMapList = singleton.mapCycleList.ToList();
			var oldTextureList = singleton.mapTextures.ToList();


			// extract new maps from build settings
			var newMaps = EditorBuildSettings.scenes.Skip(2).Where( s => s.enabled ).Select( 
				s => Path.GetFileNameWithoutExtension( s.path ) ).ToList();
			

			// create list of map textures

			var newTextures = new List<Texture>();

			// texture list should have the same number of items as map names list
			for (int i = 0; i < newMaps.Count; i++) {
				newTextures.Add (null);
			}
			
			for (int i = 0; i < oldMapList.Count && i < oldTextureList.Count; i++) {
				if (null == oldTextureList [i])
					continue;
				int newIndex = newMaps.IndexOf (oldMapList [i]);
				if (newIndex >= 0) {
					// move texture to new index
					newTextures[newIndex] = oldTextureList[i];
				}
			}


			// assign new lists
			singleton.mapCycleList = newMaps;
			singleton.mapTextures = newTextures;


			EditorUtility.SetDirty( singleton );

			Debug.Log ("Map cycle set - total of " + newMaps.Count + " scenes");

		}


	}

}
