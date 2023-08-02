using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.IO;

namespace uGameCore.Editor {


	// make it a class to prevent scripts to inherit it, and cause potential problems - but it can't be
	// found if it is not a Object
	public	interface IOneClickSetupModule
	{

//		public virtual bool abortIfFailed { get { return false; } }
//
//		public virtual void Run ( string startupScenePath, string offlineScenePath ) { }

		bool abortIfFailed { get ; }

		void Run (string startupScenePath, string offlineScenePath);

	}


	public class OneClickSetup {


		public	static	void	Setup() {
			
			// ask user to save modified scenes
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ()) {
				return;
			}

			// create new scenes as copies of original startup and offline scenes
			string newStartupScenePath = CopyOriginalScene( "StartupScene.unity", SetupWindow.GetDefaultStartupScenePath() );
			string newOfflineScenePath = CopyOriginalScene( "OfflineScene.unity", SetupWindow.GetDefaultOfflineScenePath() );

			Debug.Log ("Created startup and offline scenes: " + newStartupScenePath + ", " + newOfflineScenePath);

			// refresh asset database so that new scenes can be recognized
			AssetDatabase.Refresh ();

			Debug.Log ("Asset database refreshed");

			// add them to build settings
			SetupWindow.ConfigureBuildSettings( newStartupScenePath, newOfflineScenePath );

			// add demo scenes to build settings
			SetupWindow.AddDemoScenesToBuildSettings();

			Debug.Log ("Demo scenes added to build settings");

			// open newly created startup scene
			var openedScene = EditorSceneManager.OpenScene( SetupWindow.ConvertPathToProjectPath( newStartupScenePath ), OpenSceneMode.Single );
			if (!openedScene.IsValid() || !openedScene.isLoaded) {
				Debug.LogError ("Failed to open startup scene");
				return;
			}

			Debug.Log ("Startup scene opened");

			// create modules
			SetupWindow.CreatePrefabs( SetupWindow.SearchForModules().Where( mod => ! mod.isAlreadyCreated ) ) ;

			// group modules
			Utilities.GroupAllModules();
			Utilities.GroupObjects<Canvas> (Utilities.menusContainerName);

			// first mark it as dirty - otherwise, changes will not be saved
		//	EditorSceneManager.MarkSceneDirty( openedScene );

			// assign NetworkManager offline scene
			var nm = Object.FindObjectOfType<UnityEngine.Networking.NetworkManager>();
			if (nm != null) {
				nm.offlineScene = System.IO.Path.GetFileNameWithoutExtension (newOfflineScenePath);
				Debug.Log ("Assigned offline scene in NetworkManager : " + nm.offlineScene);
				EditorUtility.SetDirty(nm);	// mark it as dirty
			} else {
				Debug.LogError ("NetworkManager not found - offline scene is not assigned");
			}

			// map cycle
			SetupMapCycle();

			// save current scene
			if( EditorSceneManager.SaveScene( openedScene ) )
				Debug.Log ("Scene saved");
			else
				Debug.LogError ("Failed to save scene");

			// run setup modules
			//	Debug.Log("Starting modules");
			//	Resources.FindObjectsOfTypeAll<MonoScript>()[0].GetClass();


			Debug.Log ("Setup finished");

		}


		public	static	string	CopyOriginalScene( string defaultName, string originalPath ) {

			string filePath = FindFileName( Application.dataPath, defaultName );
			FileUtil.CopyFileOrDirectory( originalPath, filePath );
			return filePath;
		}

		public	static	string	FindFileName( string dir, string defaultName ) {

			string originalPath = System.IO.Path.Combine (dir, defaultName);
			string originalPathWoExtension = System.IO.Path.Combine (dir,
				                                 System.IO.Path.GetFileNameWithoutExtension (originalPath));
			string extension = System.IO.Path.GetExtension (originalPath);

			string path = originalPath;

			int max = 20;
			for (int i = 0; i < max; i++) {
				var info = new System.IO.FileInfo (path);

				if (!info.Exists) {
					return path;
				}

				path = originalPathWoExtension + i.ToString () + extension;
			}

			throw new System.IO.IOException ("Failed to find file name for " + originalPath);

		//	return "";
		}


		public	static	void	SetupMapCycle() {

			var mapCycle = Object.FindObjectOfType<MapManagement.MapCycle> ();
			if (null == mapCycle) {
				Debug.LogWarning ("Map cycle script not found");
				return;
			}

			var scenes = EditorBuildSettings.scenes.Skip(2).Where( s => s.enabled ).Select( 
				s => Path.GetFileNameWithoutExtension( s.path ) ).ToList();

			mapCycle.mapCycleList = scenes;

			Debug.Log ("Map cycle set - total of " + scenes.Count + " scenes");

			EditorUtility.SetDirty( mapCycle );
		}

	}

}
