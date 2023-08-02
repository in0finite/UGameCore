using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UGameCore.Utilities;
using System.Reflection;

namespace UGameCore.Editor {
	
	public class ModuleScriptsWindow : EditorWindow
	{

		List<Assembly>	foundAssemblies = new List<Assembly>();
		List<bool>	enabledStates = new List<bool>();


		void ScanAssemblies() {

			foundAssemblies.Clear ();

			// find all assemblies which depend on 'targetAsm'
			Assembly targetAsm = typeof( IModuleScript ).Assembly;

			var dependentAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where( 
				asm => asm.GetReferencedAssemblies().Any( refAsm => refAsm.FullName == targetAsm.FullName ) );

			foundAssemblies.AddRange (dependentAssemblies);

			if (!foundAssemblies.Contains (targetAsm))
				foundAssemblies.Add (targetAsm);


			enabledStates.Clear ();

			for (int i = 0; i < foundAssemblies.Count; i++) {
				enabledStates.Add (false);
			}

		}

		void AttachScriptsFromSelectedAssemblies ()
		{

			var assemblies = new List<Assembly> ();

			for (int i = 0; i < this.foundAssemblies.Count; i++) {

				if (this.enabledStates [i]) {

					assemblies.Add (this.foundAssemblies [i]);

				}

			}


			if (assemblies.Count > 0) {
				
				ModuleScriptsLoader.AttachModuleScripts (assemblies);

			} else {

				Debug.LogWarning ("No assembly is selected");
			}

		}


		void OnGUI()
		{


			GUILayout.Space (30);

			// button for scanning assemblies
			if (GUILayout.Button ("Scan for assemblies")) {
				this.ScanAssemblies ();
			}

			GUILayout.Space (30);

			// display all assemblies and add option to select them

			GUILayout.Label ("List of assemblies:");
			GUILayout.Space (10);

			EditorGUILayout.Separator ();

			int count = this.foundAssemblies.Count;

			for (int i = 0; i < count; i++) {

				this.enabledStates[i] = GUILayout.Toggle (this.enabledStates [i], this.foundAssemblies [i].GetName().Name );

			}

			EditorGUILayout.Separator ();

			GUILayout.Space (30);

			// button for attaching scripts

			if (GUILayout.Button ("Attach scripts from selected assemblies")) {

				AttachScriptsFromSelectedAssemblies ();

			}


		}

	}

}
