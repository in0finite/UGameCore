﻿using UnityEngine;
using UGameCore.Utilities;

namespace UGameCore.Editor
{

    public static class Utilities {

		public	static	string	menusContainerName = "MenusContainer" ;
		public	static	string	modulesContainerName = "uGameCore" ;



		public	static	void	GroupAllModules() {

			GroupObjects<ModulePart> (modulesContainerName);

		}

		public	static	void	UnGroupAllModules() {

			UnGroupObjects<ModulePart> ();

		}

		public	static	void	GroupAllMenusAndCanvases() {

			GroupObjects<Menu.Menu> (menusContainerName);
			GroupObjects<Canvas> (menusContainerName);

		}

		public	static	void	UnGroupAllMenusAndCanvases() {

			UnGroupObjects<Menu.Menu> ();
			UnGroupObjects<Canvas> ();

		}

		public	static	void	GroupObjects<T>( string containerName ) where T : Component {
			
			var go = GameObject.Find (containerName);

			if (null == go)
				go = new GameObject (containerName);

			// don't destroy it on scene load
			if (null == go.GetComponent<DontDestroyOnLoad> ())
				go.AddComponent<DontDestroyOnLoad> ();

			var objects = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);

			int count = 0;
			foreach (var obj in objects) {
				if (obj.transform.parent != go.transform)
					obj.transform.SetParent (go.transform, true);
				count++;
			}

			Debug.Log ("Grouped " + count + " objects of type " + typeof(T).ToString() );

		}

		public	static	void	UnGroupObjects<T>() where T : Component {

			var objects = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);

            int count = 0;
			foreach (var obj in objects) {
				obj.transform.SetParent (null, true);
				count++;
			}

			Debug.Log ("Ungrouped " + count + " objects of type " + typeof(T).ToString() );

		}


		public	static	T	FindSingletonOrThrow<T>() where T : MonoBehaviour {

			var obj = Object.FindFirstObjectByType<T>();
			if (null == obj)
				throw new ObjectNotFoundException ("Failed to find singleton of type " + typeof(T));

			return obj;
		}

	}

}