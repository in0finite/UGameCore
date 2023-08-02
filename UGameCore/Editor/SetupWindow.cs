using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

namespace uGameCore.Editor
{
	using Utilities2 = uGameCore.Utilities.Utilities ;


	public class SetupWindow : StepByStepWindow
	{

		public class PrefabInfo
		{
			public	bool	checkboxState = true ;
			public	bool	isAlreadyCreated = false;
			public	GameObject	prefab = null;
			public	string	prefabPath = "";
		}

//		private	List<bool>	checkboxStates = new List<bool>();
//		private	List<GameObject>	foundPrefabs = new List<GameObject>();
//		private	List<bool>	isAlreadyCreated = new List<bool>();

		private	List<PrefabInfo>	m_foundPrefabs = new List<PrefabInfo>();

		private	bool	m_groupPrefabsAfterCreating = true ;

	//	private	Vector2	m_scrollViewPosFoundPrefabs = Vector2.zero ;

		private	string	m_startupScenePath = "";
		private	string	m_offlineScenePath = "";
		private	int		m_startupSceneChosenOption = 0;
		private	int		m_offlineSceneChosenOption = 0;

		private	Vector2	m_scrollViewPosDemoScenes = Vector2.zero ;



		public SetupWindow() {
			
			// create steps
			var steps = new List<StepInfo>();


			StepInfo step = new StepInfo ();
			step.title = "uGameCore setup";
			step.onGUI = OnGUIWelcome;

			steps.Add (step);

			step = new StepInfo ();
			step.title = "Setup scenes";
			step.onGUI = OnGUISetupScenes;
			step.canSkip = true;

			steps.Add (step);

			step = new StepInfo ("Demo scenes", OnGUIDemoScenes);
			steps.Add (step);

			step = new StepInfo ();
			step.title = "Create prefabs";
			step.onGUI = OnGUICreatePrefabs;

			steps.Add (step);

			// assign next and previous indexes
			for (int i = 0; i < steps.Count; i++) {
				if( i < steps.Count - 1 )
					steps [i].nextIndex = i + 1;
				if( i > 0 )
					steps [i].previousIndex = i - 1;
			}
			
			m_steps = steps.ToArray ();


			this.titleContent = new GUIContent ("Setup");
		//	this.minSize = new Vector2 (500, 600);

		}

		private void OnGUIWelcome() {

			string text = "Welcome to setup.\n" +
				"This step by step guide will help you get up and running in just a few clicks.\n" +
				"Click next to proceed.";

			GUILayout.Label (text);

		}

		private void OnGUISetupScenes() {
			
			string text = "The package requires that you have 2 separate scenes.\n" +
				"First scene is the startup scene which" +
			              "is loaded only once at startup, and the second is offline scene.";
		//	GUILayout.Label ( text, GUILayout.MaxWidth(this.position.size.x - 15) );
			GUILayout.Label( text, GUILayout.Height( 40 ) );

			GUILayout.Space (10);

			GUILayout.Label ("You can create new scenes, or use existing ones.");

			GUILayout.Space (20);

			DisplayForScene ("startup scene", "MyStartupScene", ref m_startupScenePath, ref m_startupSceneChosenOption,
				GetDefaultStartupScenePath() );

			GUILayout.Space (20);

			DisplayForScene ("offline scene", "MyOfflineScene", ref m_offlineScenePath, ref m_offlineSceneChosenOption,
				GetDefaultOfflineScenePath ());
			
			GUILayout.Space (50);


			// check if current build settings are valid
			// if they aren't, add button to configure them

//			foreach (var s in EditorBuildSettings.scenes) {
//				GUILayout.Label (s.path);
//			}

			bool areSettingsValid = false;

			if (!string.IsNullOrEmpty (m_startupScenePath) && !string.IsNullOrEmpty (m_offlineScenePath)) {
				// user has selected scenes
				// check if current settings are valid

				if (m_startupScenePath == m_offlineScenePath) {
					// the scenes are the same
					EditorGUILayout.HelpBox ("Scenes are the same.", MessageType.Error);

				} else {

					areSettingsValid = AreSceneBuildSettingsValid (m_startupScenePath, m_offlineScenePath);

					if (!areSettingsValid) {
						
						GUILayout.Label ("Scene build settings are not cofigured. You have to configure them before proceeding.\n" +
						"NOTE: This will modify your build settings");
					//	GUILayout.Label("Cofigure scenes.\nThis will modify your build settings, and assign offline scene in " +
					//		"Network Manager.");

						if (Utilities2.ButtonWithCalculatedSize ("Configure scene settings")) {

							ConfigureBuildSettings (m_startupScenePath, m_offlineScenePath);

						}

						GUILayout.Space (10);
					}

				}
			}


			GUILayout.Space (15);

		//	if (areSettingsValid) {
				if (OnGUIEnsureStartupSceneIsLoaded ("Startup scene must be opened to setup Network Manager")) {
					OnGUINetworkManager ();
				}
		//	}


			// enable/disable next button
			this.GetCurrentStep().allowsNext = areSettingsValid ;

		}

		private	static	void	DisplayForScene( string sceneDisplayName, string sceneDefaultName, 
			ref string selectedScenePath, ref int selectedOption, string defaultScenePath ) {

			GUILayout.Label ("selected " + sceneDisplayName + ": " + selectedScenePath);


			int oldOption = selectedOption;

			string[] options = new string[]{ "Create new", "Use current", "Choose existing" };
			selectedOption = GUILayout.SelectionGrid(selectedOption, options, 1, EditorStyles.radioButton);

			if (selectedOption != oldOption) {
				// option changed
				// reset scene path
				selectedScenePath = "" ;
			}


		//	string stringToRemove = "";


			switch (selectedOption) {
			case 0:
				if (Utilities2.ButtonWithCalculatedSize ("Create " + sceneDisplayName)) {
					
					if (CreateScene ("Create " + sceneDisplayName, sceneDefaultName, ref selectedScenePath)) {
						// copy default scene to selected path

					//	bool copied = false;
						try {
							FileUtil.CopyFileOrDirectory ( defaultScenePath, selectedScenePath );

							AssetDatabase.Refresh();	// so that scene can be recognized
						//	copied = true ;
							Debug.Log ("Copied " + defaultScenePath + " to " + selectedScenePath);
						} catch(System.Exception ex) {
						//	copied = false;
							// reset scene path
							selectedScenePath = "" ;
							Debug.LogException (ex);
						}

					}

					EditorGUIUtility.ExitGUI ();
				}
				break;
			case 1:
				selectedScenePath = EditorSceneManager.GetActiveScene ().path;
//				stringToRemove = "Assets/";
//				if (selectedScenePath.StartsWith (stringToRemove))
//					selectedScenePath = selectedScenePath.Substring (stringToRemove.Length);
				break;
			case 2:
				if (Utilities2.ButtonWithCalculatedSize ("Select " + sceneDisplayName)) {
					SelectScene ("Select " + sceneDisplayName, ref selectedScenePath);

					EditorGUIUtility.ExitGUI ();
				}

			//	EditorGUILayout.ObjectField( "", null, typeof(SceneAsset), false );

				break;
			}


//			// remove trailing '.unity', because scene paths inside build settings don't have it
//			stringToRemove = ".unity" ;
//			if (selectedScenePath.EndsWith (stringToRemove)) {
//				selectedScenePath = selectedScenePath.Substring (0, selectedScenePath.Length - stringToRemove.Length);
//			}

		}

		private	static	bool	CreateScene( string title, string defaultName, ref string path ) {

			path = EditorUtility.SaveFilePanel (title, Application.dataPath, defaultName, "unity");

			if (!string.IsNullOrEmpty (path)) {
				// user selected a scene
				return CheckScenePath( ref path );
			}

			return false;
		}

		private	static	bool	SelectScene( string title, ref string path ) {

			path = EditorUtility.OpenFilePanel (title, Application.dataPath, "unity");

			if (!string.IsNullOrEmpty (path)) {
				// user selected a scene
				return CheckScenePath( ref path );
			}

			return false;
		}

		private	static	bool	CheckScenePath( ref string path ) {

			if (!IsPathInsideProject (path)) {
				path = "";
				Debug.LogError ("Scene is not inside project.");
				return false;
			}

			path = ConvertPathToProjectPath (path);

			return true ;
		}

		/// <summary>
		/// Determines whether the specifed absolute path is inside project.
		/// </summary>
		private	static	bool	IsPathInsideProject( string path ) {

			if (!path.StartsWith (Application.dataPath))
				return false;
			
			return true;
		}

		/// <summary>
		/// Converts the path to project path. The resulting path has 'Assets' at the beginning.
		/// If path is not inside project folder, function has no effects.
		/// </summary>
		public	static	string	ConvertPathToProjectPath( string path ) {
			
			string toReplace = Application.dataPath ;
			string toRemove = "Assets";
			if (toReplace.EndsWith (toRemove))
				toReplace = toReplace.Substring (0, toReplace.Length - toRemove.Length);

			return path.Replace (toReplace, "");

		}

		public	static	string	ConvertScenePathToBuildSettingsPath( string path ) {

			path = ConvertPathToProjectPath (path);

			path = path.Replace ("\\", "/");

		//	if (path.StartsWith ("Assets/")) {
		//		return path.Remove (0, "Assets/".Length);
		//	}

			return path;
		}

		private	static	bool	AreSceneBuildSettingsValid( string startupScenePath, string offlineScenePath ) {

			if (!string.IsNullOrEmpty (startupScenePath) && !string.IsNullOrEmpty (offlineScenePath)) {
				if (EditorBuildSettings.scenes.Length >= 2) {
					if (EditorBuildSettings.scenes [0].enabled && EditorBuildSettings.scenes [0].path == ConvertScenePathToBuildSettingsPath( startupScenePath )
						&& EditorBuildSettings.scenes [1].enabled && EditorBuildSettings.scenes [1].path == ConvertScenePathToBuildSettingsPath( offlineScenePath ) ) {
						return true;
					}
				}
			}

			return false;
		}

		public	static	void	ConfigureBuildSettings( string startupScenePath, string offlineScenePath ) {

			var list = EditorBuildSettings.scenes.ToList ();


			string[] scenePaths = new string[]{ startupScenePath, offlineScenePath };
			// convert paths to build settings paths
			for (int i = 0; i < scenePaths.Length; i++) {
				scenePaths [i] = ConvertScenePathToBuildSettingsPath (scenePaths [i]);
			}


			for (int i=0; i < scenePaths.Length ; i++) {
				var scenePath = scenePaths [i];

				var item = list.Find (s => s.path == scenePath);

				if (null == item) {
					// add scene to build settings
					Debug.Log("Adding scene " + scenePath + " to build settings");

					item = new EditorBuildSettingsScene (scenePath, true);
					list.Insert (i, item);
				}

				// check if it is enabled
				if(!item.enabled)
					item.enabled = true ;

				// adjust index
				int itemIndex = list.IndexOf( item );
				if( itemIndex != i ) {
					Debug.Log("Adjusting index of " + scenePath + " inside build settings");

					var tmp = list [i];
					list [i] = item;
					list [itemIndex] = tmp;
				}

			}

			// apply
			EditorBuildSettings.scenes = list.ToArray();

			Debug.Log ("Build settings configured");

		}

		private	static	void	AssignOfflineSceneInNetworkManager( string offlineScenePath ) {

			var nm = FindObjectOfType<UnityEngine.Networking.NetworkManager> ();

			if (nm != null) {

				string name = GetSceneNameFromPath (offlineScenePath);
				nm.offlineScene = name;

				Debug.Log ("Offline scene assigned in Network Manager.");

//				if (Utilities2.DisabledButtonWithCalculatedSize ( nm != null && nm.offlineScene != name, "Assign offline scene in Network Manager")) {
//					nm.offlineScene = name ;
//				}
//				GUILayout.Label ("");
			} else {
				// network manager not found in scene
			//	EditorGUILayout.HelpBox ("Network manager not found in scene.", MessageType.Error);
				Debug.LogError("Network Manager not found in scene.");
			}

		}

		private	static	void	OnGUINetworkManager() {

			// assign offline scene in NetworkManager

			if (EditorBuildSettings.scenes.Length > 1) {

				var nm = FindObjectOfType<UnityEngine.Networking.NetworkManager> ();
				string name = GetSceneNameFromPath (EditorBuildSettings.scenes [1].path);

				if (Utilities2.DisabledButtonWithCalculatedSize (nm != null && nm.offlineScene != name, "Assign offline scene in Network Manager")) {
					nm.offlineScene = name;
					Debug.Log ("Assigned offline scene in Network Manager : " + nm.offlineScene);
					EditorUtility.SetDirty (nm);
				}

				GUILayout.Space (10);

				if (null == nm) {
					EditorGUILayout.HelpBox ("Network Manager not found in scene.", MessageType.Warning);
				}

			} else {

				EditorGUILayout.HelpBox ("Offline scene is not added to build settings.", MessageType.Warning);
			}

		}

		private	static	string	GetSceneNameFromPath( string scenePath ) {
			
			return System.IO.Path.GetFileNameWithoutExtension (scenePath);

		}

//		private	static	string	RemoveStringFromEnd( string str, string strToRemove ) {
//
//
//		}


		private void OnGUIDemoScenes() {
			
			GUILayout.Label ("Would you like to also add demo scenes to build settings ?", m_centeredLabelStyle);

			GUILayout.Label ("This is recommended if you are installing for the first time.", m_centeredLabelStyle);

			GUILayout.Space (20);


			var demoScenes = GetDemoScenePaths ();
			var buildSettingsScenes = EditorBuildSettings.scenes.ToList();
			int numNotAdded = 0;


			GUILayout.Label ("Available demo scenes:");
			GUILayout.Space (10);

			m_scrollViewPosDemoScenes = EditorGUILayout.BeginScrollView (m_scrollViewPosDemoScenes, GUILayout.MaxHeight(150) );

			foreach (var demoScene in demoScenes) {

				if (buildSettingsScenes.Any (s => s.path == demoScene)) {
					// already in build settings
					GUILayout.Label( "(added) " + demoScene );
				} else {
					GUILayout.Label( "+ " + demoScene );
					numNotAdded++;
				}

			}

			EditorGUILayout.EndScrollView ();

			GUILayout.Space (20);

			if(Utilities2.DisabledButtonWithCalculatedSize( numNotAdded > 0, "Add demo scenes" )) {

				AddDemoScenesToBuildSettings ();

			}

		}

		public	static	void	AddDemoScenesToBuildSettings() {

			var demoScenes = GetDemoScenePaths ();
			var buildSettingsScenes = EditorBuildSettings.scenes.ToList();


			bool addedAnyScene = false ;

			foreach (var demoScene in demoScenes) {

				if (buildSettingsScenes.Any (s => s.path == demoScene)) {
					// already in build settings

				} else {
					// add to build settings
					Debug.Log("Adding " + demoScene + " to build settings");

					buildSettingsScenes.Add( new EditorBuildSettingsScene(demoScene, true) );
					addedAnyScene = true;
				}

			}

			if (addedAnyScene) {
				// apply
				EditorBuildSettings.scenes = buildSettingsScenes.ToArray ();
			}

		}

		public	static	List<string>	GetDemoScenePaths() {

			return new List<string> () { "Assets/" + Utilities2.GetAssetRootFolderName() + 
				"/GamePlay/Scenes/demo.unity" };

		}

		public	static	string	GetDefaultStartupScenePath() {

			return "Assets/" + Utilities2.GetAssetRootFolderName () + "/General/Scenes/startup_scene.unity";

		}

		public	static	string	GetDefaultOfflineScenePath() {

			return "Assets/" + Utilities2.GetAssetRootFolderName () + "/General/Scenes/offline_scene.unity";

		}


		private	static	bool	OnGUIEnsureStartupSceneIsLoaded( string warningMsg ) {

			if (0 == EditorBuildSettings.scenes.Length) {
				EditorGUILayout.HelpBox ( "There are no scenes in build settings", MessageType.Warning );
				return false;
			}


			var activeScene = EditorSceneManager.GetActiveScene ();

			if (activeScene.buildIndex != 0) {
				// this is not the startup scene

				EditorGUILayout.HelpBox (warningMsg, MessageType.Warning);

				GUILayout.Space (10);

				if (Utilities2.ButtonWithCalculatedSize ("Open startup scene")) {

					if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ()) {

						EditorSceneManager.OpenScene (EditorBuildSettings.scenes [0].path, OpenSceneMode.Single);

						// clear list
					//	this.ClearFoundPrefabsList ();
					}

					EditorGUIUtility.ExitGUI ();
				}

			} else {

				return true;
			}

			return false;
		}


		private void OnGUICreatePrefabs() {

			string s = "This asset is consisted of so called modules (which are actually prefabs)." + 
				" Each prefab has it's own features which bring functionality to the game.\n\n" +
				"To see all available prefabs, click Find prefabs.\n" +
				"After that, you can select which ones you want to create.\n" + 
				"When you are done, you can create them by clicking Create prefabs.";
			GUILayout.Label (s);

			GUILayout.Space (20);

			GUILayout.Label ("Note that this may take a while depending on the size of your project.");

			if (Utilities2.ButtonWithCalculatedSize ("Find prefabs")) {
				this.FindPrefabs ();
			}

			GUILayout.Space (20);

			// display some stats
			GUILayout.Label( "Found prefabs: " + m_foundPrefabs.Count + ", new: " + m_foundPrefabs.Count( item => !item.isAlreadyCreated ) );

			GUILayout.Space (10);

			// display found prefabs in a scroll view

		//	m_scrollViewPosFoundPrefabs = EditorGUILayout.BeginScrollView( m_scrollViewPosFoundPrefabs,
		//		GUILayout.MinHeight( m_foundPrefabs.Count * 15 ), GUILayout.MaxHeight( 450 ), GUILayout.Width(this.position.width - 15) );

			for (int i = 0; i < m_foundPrefabs.Count; i++) {
				var foundPrefab = m_foundPrefabs [i];

				string text = foundPrefab.prefab.name + "        " + foundPrefab.prefabPath;

				if (foundPrefab.isAlreadyCreated) {
					// this one is already created
					GUILayout.Label (text);
				} else {
					// add option to select him
					foundPrefab.checkboxState = GUILayout.Toggle( foundPrefab.checkboxState, text );
				}

			}

		//	EditorGUILayout.EndScrollView ();

			GUILayout.Space (20);

			if (OnGUIEnsureStartupSceneIsLoaded("This is not the startup scene, so prefabs can not be created.")) {
				// this is the startup scene

				m_groupPrefabsAfterCreating = EditorGUILayout.Toggle ( "group prefabs after creating", m_groupPrefabsAfterCreating);

				// prefabs can be created
				if (Utilities2.DisabledButtonWithCalculatedSize ( m_foundPrefabs.Any( p => p.checkboxState && !p.isAlreadyCreated ), "Create prefabs")) {

					this.CreatePrefabs ();

					if (m_groupPrefabsAfterCreating) {
						Utilities.GroupAllModules ();
						Utilities.GroupAllMenusAndCanvases ();
					}

					// clear list
				//	this.ClearFoundPrefabsList ();
				}

			}

			GUILayout.Space (10);

		}


		public	static	List<PrefabInfo>	SearchForModules() {

			var list = new List<PrefabInfo> ();

			// find all created prefabs
			var existingModules = FindObjectsOfType<ModulePart>();
			var existingModulePrefabs = existingModules.Select (mod => PrefabUtility.GetPrefabParent (mod.gameObject))
				.OfType<GameObject> ().ToList();

			// find all prefabs in a project
			var guids = AssetDatabase.FindAssets("t:Prefab");

			foreach (var guid in guids) {

				var path = AssetDatabase.GUIDToAssetPath (guid);

				// filter modules
				var modulePrefabs = AssetDatabase.LoadAllAssetsAtPath (path).OfType<GameObject> ()
					.Where (go => go.GetComponent<ModulePart> () != null).ToList ();

				// add them to list
				foreach (var modulePrefab in modulePrefabs) {
					
					PrefabInfo prefabInfo = new PrefabInfo ();
					prefabInfo.prefabPath = path;
					prefabInfo.prefab = modulePrefab;
					prefabInfo.isAlreadyCreated = existingModulePrefabs.Contains( modulePrefab ) ;

					list.Add (prefabInfo);
				}

			}

			return list;
		}

		void ClearFoundPrefabsList() {

			m_foundPrefabs.Clear ();

		}

		void FindPrefabs() {

			// clear current list
			m_foundPrefabs.Clear();

			m_foundPrefabs = SearchForModules ();


		}

		public	static	List<GameObject>	CreatePrefabs( IEnumerable<PrefabInfo> prefabs ) {

			var createdPrefabs = new List<GameObject> ();

			foreach (var prefab in prefabs) {

				var go = PrefabUtility.InstantiatePrefab (prefab.prefab) as GameObject;
				if (go != null)
					createdPrefabs.Add (go);

			}

			Debug.Log ("Created " + createdPrefabs.Count + " prefabs");

			return createdPrefabs;
		}

		void CreatePrefabs() {
			
			CreatePrefabs (m_foundPrefabs.Where (p => p.checkboxState && !p.isAlreadyCreated));

			// refresh list
			this.FindPrefabs();

		}

	}

}
