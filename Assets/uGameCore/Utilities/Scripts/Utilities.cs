using System;
using UnityEngine;
using System.Linq;

namespace uGameCore.Utilities
{
	public static class Utilities
	{


		public	static	string	GetAssetName() {

			return typeof(Utilities).Assembly.GetName ().Name;

		}

		public	static	string	GetAssetRootFolderName() {

			return GetAssetName ();

		}


		public	static	void	SendMessageToAllMonoBehaviours( string msg, params object[] arguments ) {

			var objects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour> ();

			foreach (var obj in objects) {
				obj.InvokeExceptionSafe (msg, arguments);
			}

		}

		public	static	void	RunExceptionSafe( System.Action function ) {

			try {
				function();
			} catch(System.Exception ex) {
				try {
					Debug.LogException (ex);
				} catch {}
			}

		}

		/// <summary>
		/// Invokes all subscribed delegates, and makes sure any exception is caught and logged, so that all
		/// subscribers will be notified.
		/// </summary>
		public	static	void	InvokeEventExceptionSafe( MulticastDelegate eventDelegate, params object[] parameters ) {

			var delegates = eventDelegate.GetInvocationList ();

			foreach (var del in delegates) {
				if (del.Method != null) {
					try {
						del.Method.Invoke (del.Target, parameters);
					} catch(System.Exception ex) {
						UnityEngine.Debug.LogException (ex);
					}
				}
			}

		}

		public	static	T	FindObjectOfTypeOrLogError<T>() where T : Component {

			var obj = UnityEngine.Object.FindObjectOfType<T> ();

			if (null == obj) {
				Debug.LogError ("Object of type " + typeof(T).ToString() + " can not be found." );
			}

			return obj;
		}


		public	static	Vector2	CalcScreenSizeForContent( GUIContent content, GUIStyle style ) {

			return style.CalcScreenSize (style.CalcSize (content));
		}

		public	static	Vector2	CalcScreenSizeForText( string text, GUIStyle style ) {

			return CalcScreenSizeForContent (new GUIContent (text), style);
		}

		public	static	bool	ButtonWithCalculatedSize( string text ) {

			Vector2 size = CalcScreenSizeForText (text, GUI.skin.button);

			return GUILayout.Button (text, GUILayout.Width (size.x), GUILayout.Height (size.y));
		}

		public	static	bool	DisabledButton( bool isEnabled, string text, params GUILayoutOption[] options ) {

			GUIStyle style = isEnabled ? GUI.skin.button : GUI.skin.box;

			if (GUILayout.Button (text, style, options) && isEnabled)
				return true;

			return false;
		}

		public	static	bool	DisabledButtonWithCalculatedSize( bool isEnabled, string text ) {

			GUIStyle style = isEnabled ? GUI.skin.button : GUI.skin.box;
			Vector2 size = CalcScreenSizeForText (text, style);

			var options = new GUILayoutOption[] { GUILayout.Width (size.x), GUILayout.Height (size.y) };

			if (GUILayout.Button (text, style, options) && isEnabled)
				return true;

			return false;
		}


		/// <summary>
		/// Formats the elapsed time (in seconds) in format hh:mm:ss, but removes the unnecessery prefix values which are zero.
		/// For example, 40 seconds will just return 40, 70 will return 01:10, 3700 will return 01:01:40.
		/// </summary>
		public	static	string	FormatElapsedTime( float elapsedTime ) {

			int elapsedTimeInteger = Mathf.CeilToInt (elapsedTime);

			int hours = elapsedTimeInteger / 3600;
			int minutes = elapsedTimeInteger % 3600 / 60;
			int seconds = elapsedTimeInteger % 3600 % 60 ;

			var sb = new System.Text.StringBuilder (10);

			if (hours > 0) {
				if (hours < 10)
					sb.Append ("0");
				sb.Append (hours);
				sb.Append (":");
			}

			if (hours > 0 || minutes > 0) {
				if (minutes < 10)
					sb.Append ("0");
				sb.Append (minutes);
				sb.Append (":");
			}

			if (seconds < 10)
				sb.Append ("0");
			sb.Append (seconds);

			return sb.ToString ();
		}


		public	static	System.Reflection.Assembly	GetEditorAssembly() {

			return System.AppDomain.CurrentDomain.GetAssemblies ().
				SingleOrDefault (assembly => assembly.GetName ().Name == "UnityEditor");

		}

		public	static	void	MarkObjectAsDirty( UnityEngine.Object obj ) {

			if (!Application.isEditor)
				return;

			if (Application.isPlaying)
				return;

			var asm = Utilities.GetEditorAssembly ();
			if (asm != null) {
				var type = asm.GetType( "UnityEditor.EditorUtility" );
				if (type != null) {
					var method = type.GetMethod ("SetDirty", 
						System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
					if (method != null) {
						method.Invoke (null, new object[]{ obj });
					}
				}
			}

		}


	}
}

