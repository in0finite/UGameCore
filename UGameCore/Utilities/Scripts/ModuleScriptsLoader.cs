using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace uGameCore.Utilities {
	
	public class ModuleScriptsLoader
	{


		public	static	bool	IsTypeModuleScript( Type type )
		{
			return type.GetInterfaces ().Contains (typeof(IModuleScript)) &&
				( type == typeof(MonoBehaviour) || type.IsSubclassOf (typeof(MonoBehaviour)) ) ;
		}

		public	static	List<Type>	FindModuleScriptsInAssembly (Assembly assembly) {

			return assembly.GetExportedTypes ().Where ( t => IsTypeModuleScript(t) ).ToList ();

		}

		public	static	GameObject	FindGameObjectForWhichToAttachScripts () {

			var scripts = UnityEngine.Object.FindObjectsOfType<ModuleScriptsGameObject> ();

			if (scripts.Length > 1) {
				throw new Exception ("There are multiple scripts of type " + typeof(ModuleScriptsGameObject).ToString() );
			}

			if (0 == scripts.Length) {
			//	Debug.LogError ("There is no script of type " + typeof(ModuleScriptsGameObject).ToString() );
				return null;
			}

			return scripts [0].gameObject;

		}

		public	static	GameObject	CreateGameObjectForModuleScripts()
		{

			GameObject go = new GameObject ("ModuleScripts", typeof(ModuleScriptsGameObject), typeof(DontDestroyOnLoad));

			return go;

		}

		public	static	bool	AttachModuleScripts (List<Assembly> assemblies) {

			GameObject gameObject = FindGameObjectForWhichToAttachScripts ();
			if (null == gameObject) {
				// game object not found, create it
				gameObject = CreateGameObjectForModuleScripts ();
			}


			var moduleScriptTypes = new List<Type> (30);

			foreach (var asm in assemblies) {
				moduleScriptTypes.AddRange (FindModuleScriptsInAssembly (asm));
			}

			// found scripts, now attach them

			int numAttached = 0;
			string logStr = "";

			foreach (var moduleScriptType in moduleScriptTypes) {

				// first check if he exists
				// if yes, skip him
				// otherwise, attach him

				var objects = UnityEngine.Object.FindObjectsOfType( moduleScriptType );
				if (objects.Length > 0)	// already exists
					continue;

				gameObject.AddComponent (moduleScriptType);

				numAttached++;
				logStr += moduleScriptType.ToString () + "\n";
			}

			Debug.Log ("Attached " + numAttached + "/" + moduleScriptTypes.Count + " module scripts:\n\n" + logStr);

			return true;
		}

	}

}
