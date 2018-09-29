using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace uGameCore {

	public	enum CVarDisplayType
	{
		IntegerSlider = 1,
		IntegerTextBox,
		FloatSlider,
		FloatTextBox,
		String,
		Boolean,
		None
	}

	// Variable that can be edited from options window, command line, etc.
//	[System.AttributeUsage(System.AttributeTargets.Field)]
//	public	class CVar : System.Attribute
	public	class CVar
	{
		public CVar() { }

		public	string	displayName = "" ;
		/// <summary> Unique cvar name. If empty, the name of the field is used. </summary>
		public	string	name = "" ;

		public	float	minValue = float.MinValue ;
		public	float	maxValue = float.MaxValue ;

		public	int		minLength = 0 ;		// if it is a string
		public	int		maxLength = 0 ;		// if it is a string
		public	string	unallowedCharacters = "" ;	// if it is a string

	//	internal	CVarDisplayType	displayType = CVarDisplayType.None ;

		public	bool	isInsideCfg = true ;

	//	public	string	currentString = "" ;
	//	public	float	currentFloat = 0.0f ;

		public	object	defaultValue = null ;

	//	private	object	m_currentValue = null;
	//	public	object	currentValue { get { return m_currentValue; } }

		public	System.Func<object> getValue = null;
		public	System.Action<object> setValue = null;
		public	System.Func<object, bool> isValid = null;
		public	System.Action onChanged = null;

		private	System.Type	m_type = null;
		public	System.Type	cvarType {
			get {
				if (m_type != null)
					return m_type;
				m_type = this.getValue ().GetType ();
				return m_type;
			}
		}

	}

	public	enum CVarScanType {
		
		NonStaticMember,
		StaticMember,

	}


	public class CVarManager : MonoBehaviour {


//		public	class CVarFieldInfo
//		{
//			public CVarFieldInfo (FieldInfo field, Object objectOwner)
//			{
//				this.field = field;
//				this.objectOwner = objectOwner;
//
//				m_cvar = (CVar) this.field.GetCustomAttributes (typeof(CVar), true) [0] ;
//			}
//
//			public	CVar	GetCVar () {
//				return m_cvar;
//			}
//
//			public	System.Type	cvarType { get { return field.FieldType; } }
//
//			public	CVarDisplayType	cvarDisplayType {
//				get {
//					var cvar = this.GetCVar ();
//					var type = cvar.displayType;
//
//					if (CVarDisplayType.None == type) {
//						if (this.cvarType == typeof(string)) {
//							type = CVarDisplayType.String;
//						} else if (this.cvarType == typeof(float)) {
//							if (cvar.minValue != float.MinValue && cvar.maxValue != float.MaxValue) {
//								// there is limit on the number -> use slider
//								type = CVarDisplayType.FloatSlider;
//							} else {
//								// there is no limit on the number -> use text box
//								type = CVarDisplayType.FloatTextBox;
//							}
//						}
//					}
//	
//					return type;
//				}
//			}
//
//			public	string	cvarName {
//				get {
//					var cvar = this.GetCVar ();
//					if ("" == cvar.name) {
//						return this.field.Name;
//					}
//					return cvar.name;
//				}
//			}
//			
//			public	FieldInfo field ;
//			public	Object	objectOwner ;
//		//	public	object	defaultValue;
//			private	CVar	m_cvar;
//		}


	//	private	static	Dictionary<Object, List<CVar>>	m_editableVariables = new Dictionary<Object, List<CVar>>();
		private	static	List<CVar>	m_cvars = new List<CVar>();
		public	static	IEnumerable<CVar>	CVars { get { return m_cvars;
			} }
		
		private	static	bool	m_processedConfigurationSinceStartup = false ;
		public	static	event	System.Action	onProcessedConfiguration;

		public	static	event System.Action	onAddCVars = delegate {};

	//	public	List<Object> objectsToScan = new List<Object> ();
	//	public	bool	scanForAttachedComponents = true ;
	//	public	bool	scanForChildren = true ;

	//	[CVar(unallowedCharacters = "135243", minValue = 2f, maxValue = 15f, displayType = CVarDisplayType.FloatTextBox)]
	//	private	float	m_helloReflection = 135f;

	//	private	static	List<CVarFieldInfo>	m_cvarFields = new List<CVarFieldInfo>();
	//	public	static	IEnumerable<CVarFieldInfo>	CVarFields { get { return m_cvarFields; } }

		public	static	CVarManager	singleton { get ; private set ; }


		void Awake () {

			singleton = this;

		}

		void Start () {



		}

		static void Scan() {

//			// obtain all objects which should be scanned
//			var objects = new List<Object> ();
//
//			objects.AddRange (singleton.objectsToScan);
//
//			if (singleton.scanForAttachedComponents) {
//				if (singleton.scanForChildren) {
//					objects.AddRange (singleton.GetComponentsInChildren<Component> ());
//				} else {
//					objects.AddRange (singleton.GetComponents<Component> ());
//				}
//			}
//
//			// scan objects for cvars
//			foreach (var o in objects) {
//				
//				var members = o.GetType().GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
//				var cvars = System.Array.FindAll (members, m => m.GetCustomAttributes (typeof(CVar), true).Length > 0 );
//
//			//	m_cvarFields.AddRange ( System.Array.ConvertAll( cvars, c => (CVar) c.GetCustomAttributes(typeof(CVar), true)[0] ) );
//				m_cvarFields.AddRange( System.Array.ConvertAll( cvars, c => new CVarFieldInfo( c, o ) ) );
//
//				
//			}
//
//			// if cfg names are not set, set them to field names
//			foreach (var cvarField in m_cvarFields) {
//				var cvar = cvarField.GetCVar ();
//				if ("" == cvar.name) {
//					cvar.name = cvarField.field.Name;
//				}
//			}
//
//			// remove duplicates (cvars with same name)
//
//
//			// process cvars
//			foreach (var cvarField in m_cvarFields) {
//				var cvar = cvarField.GetCVar ();
//
//				// set it's default value
//				cvar.defaultValue = GetCVarValue (cvarField);
//
//				// adjust their display name
//				if ("" == cvar.displayName) {
//					cvar.displayName = cvar.name;
//				}
//
//				// adjust display type
//				if (CVarDisplayType.None == cvar.displayType) {
//					if (cvarField.cvarType == typeof(string)) {
//						cvar.displayType = CVarDisplayType.String;
//					} else if (cvarField.cvarType == typeof(float)) {
//						if (cvar.minValue != float.MinValue && cvar.maxValue != float.MaxValue) {
//							// there is limit on the number -> use slider
//							cvar.displayType = CVarDisplayType.FloatSlider;
//						} else {
//							// there is no limit on the number -> use text box
//							cvar.displayType = CVarDisplayType.FloatTextBox;
//						}
//					}
//				}
//			}
//
//			// assign functions
//			foreach (var cvarField in m_cvarFields) {
//				var cvar = cvarField.GetCVar ();
//				cvar.getValue = CVarManager.GetCVarValue (cvarField);
//				cvar.setValue = (newValue) => CVarManager.SetCVarValue (cvarField, newValue);
//				cvar.isValid = (value) => CVarManager.IsCVarValueValid( cvarField, value );
//				cvar.onChanged = CVarManager.CVarChanged (cvarField);
//			}


			onAddCVars ();

		}

		private	static void SetCVarValue( CVar cvar, object newValue ) {

		//	cvarFieldInfo.field.SetValue (cvarFieldInfo.objectOwner, newValue);
			cvar.setValue( newValue );

			CVarChanged (cvar);

		}

		public	static object GetCVarValue( CVar cvar ) {

		//	return cvarFieldInfo.field.GetValue (cvarFieldInfo.objectOwner);
			return cvar.getValue();

		}
		
		// Update is called once per frame
		void Update () {

			if (!m_processedConfigurationSinceStartup) {
				try {
					ProcessConfiguration();
				} finally {
					m_processedConfigurationSinceStartup = true;
				}
			}


		}

		public	static	void	AddCVar( CVar cvar ) {

			if (m_cvars.Exists (c => c.name == cvar.name))	// cvar with the same name already exists
				return;

			// adjust some parameters first

			if ("" == cvar.displayName)
				cvar.displayName = cvar.name;

			// set default value, if it is not set
			if (null == cvar.defaultValue)
				cvar.defaultValue = CVarManager.GetCVarValue (cvar);
			

			// add it to list
			m_cvars.Add( cvar );

		}

		public	static	CVar	GetCVarByName( string name ) {

			return m_cvars.Find (cvar => cvar.name == name);

		}

		/*
		public	static	void	AddCVar( CVar cvar, Object o ) {

			CVar newVar = new CVar ();
			newVar.displayName = cvar.displayName;
			newVar.isInsideCfg = cvar.isInsideCfg;
			newVar.maxLength = cvar.maxLength;
			newVar.maxValue = cvar.maxValue;
			newVar.minLength = cvar.minLength;
			newVar.minValue = cvar.minValue;
			newVar.cfgName = cvar.cfgName;
			newVar.displayType = cvar.displayType;
			newVar.unallowedCharacters = cvar.unallowedCharacters;
			newVar.ownerObject = o;

			// copy current value to default value
			if (cvar.displayType == CVarDisplayType.String)
				newVar.defaultValue = cvar.currentString;
			else if (cvar.displayType == CVarDisplayType.FloatSlider || cvar.displayType == CVarDisplayType.FloatTextBox)
				newVar.defaultValue = cvar.currentFloat;

			// add cvar to collection

//			if (m_editableVariables.ContainsKey (o)) {
//				m_editableVariables [o].Add (newVar);
//			}
//			else {
//				var list = new List<CVar> ();
//				list.Add (newVar);
//				m_editableVariables.Add (o, list);
//			}

			m_cvars.Add (newVar);

		}
		*/

		public	static	void	ChangeCVars(CVar[] cvarsToChange, object[] newValues) {

			if (cvarsToChange.Length != newValues.Length)
				return;

			if (0 == cvarsToChange.Length)
				return;

			for (int i = 0; i < cvarsToChange.Length; i++) {
				var cvar = cvarsToChange [i];

				SetPlayerPrefsValue (cvar.name, newValues [i]);
				SetCVarValue (cvar, newValues [i]);

			}

		}

		private	static	void	ResetAllCVarsToDefaultValues() {

			foreach (var cvar in m_cvars) {
				SetCVarValue (cvar, cvar.defaultValue);
			}

		}

		private	static	void	ProcessConfiguration() {

			Scan ();

			Debug.Log ("Processing configuration - " + m_cvars.Count + " cvars registered.");


			if (! PlayerPrefs.HasKey ("cfg_created")) {
				// cfg is not created
				// set default values, and save it to disk.

				PlayerPrefs.SetString ("cfg_created", "1");

				ResetAllCVarsToDefaultValues ();	// not needed ?

				SaveCVarsToDisk ();

				Debug.Log ("Default configuration saved to disk.");

			} else {
				// cfg is already created

				// check if it contains all variables that this version contains
				// if not, add those variables
				int numCVarsAdded = 0;
				foreach (var cvar in m_cvars) {
					if (cvar.isInsideCfg) {
						if( ! PlayerPrefs.HasKey( cvar.name ) ) {
							// add this key
							var cvarValue = GetCVarValue( cvar );
							SetPlayerPrefsValue (cvar.name, cvarValue);

							numCVarsAdded ++ ;
						}
					}
				}

				if( numCVarsAdded > 0 ) {
					Debug.Log ("Added " + numCVarsAdded + " new cvars.");
					SaveCVarsToDisk ();
				}

				// now read all cvars

			//	MainScript.singleton.PopulateOptionsWindowWithCurrentSettings();	// this will set editable variables
				ReadCVarsFromPlayerPrefs();

//				// notify objects that cvars have changed
//				foreach (var cvarFieldInfo in m_cvarFields) {
//					var cvar = cvarFieldInfo.GetCVar ();
//					if (cvar.isInsideCfg) {
//						CVarChanged( cvarFieldInfo );
//					}
//				}

			}


			onProcessedConfiguration ();

		}


		public	static	void	SaveCVarsToDisk() {

			PlayerPrefs.Save ();

		}

		public	static	void	ReadCVarsFromPlayerPrefs() {

			foreach (var cvar in m_cvars) {
				
				if (!cvar.isInsideCfg)
					continue;
				
				var v = GetPlayerPrefsValue (cvar.name, cvar.cvarType);
				SetCVarValue (cvar, v);

			}

		}

		private	static	object	GetPlayerPrefsValue( string key, System.Type type ) {

			if (type == typeof(string)) {
				return PlayerPrefs.GetString (key, "");
			} else if (type == typeof(float)) {
				return PlayerPrefs.GetFloat (key, 0f);
			} else if (type == typeof(int)) {
				return PlayerPrefs.GetInt (key, 0);
			} else if (type == typeof(bool)) {
				return 0 == PlayerPrefs.GetInt (key) ? false : true;
			}

			return null;
		}

		private	static	void	SetPlayerPrefsValue( string key, object newValue ) {

			if (newValue is string) {
				PlayerPrefs.SetString (key, (string)newValue);
			} else if (newValue is float) {
				PlayerPrefs.SetFloat (key, (float)newValue);
			} else if (newValue is int) {
				PlayerPrefs.SetInt (key, (int)newValue );
			} else if (newValue is bool) {
				PlayerPrefs.SetInt (key, ((bool) newValue) ? 1 : 0);
			}

		}

		public	static	bool	IsCVarValueValid( CVar cvar, object value ) {

			if (null == value)
				return false;

			// first check based on type of cvar and it's parameters

			if (cvar.cvarType == typeof(string)) {
				string str = (string)value;

				if (cvar.minLength != 0 && str.Length < cvar.minLength ) {
					return false;
				}

				if (cvar.maxLength != 0 && str.Length > cvar.maxLength) {
					return false;
				}

				if (str.IndexOfAny (cvar.unallowedCharacters.ToCharArray ()) >= 0) {
					return false;
				}

			} else if (cvar.cvarType == typeof(float)) {
				float f = (float)value;

				if (f < cvar.minValue || f > cvar.maxValue) {
					return false;
				}

			} else if (cvar.cvarType == typeof(int)) {
				int num = (int)value;

				if (num < cvar.minValue || num > cvar.maxValue) {
					return false;
				}

			}


//			if (cvarField.objectOwner != null) {
//				try {
//					var method = cvarField.objectOwner.GetType().GetMethod("OnValidateCVar", BindingFlags.NonPublic | BindingFlags.Public
//						| BindingFlags.Instance );
//
//					if(method != null) {
//						bool valid = (bool) method.Invoke( cvarField.objectOwner, new object[]{ cvarField, value });
//						if(!valid) {
//							return false;
//						}
//					}
//
//				} catch (System.Exception ex) {
//					Debug.LogException (ex);
//				}
//			}

			// now call provided function

			if (cvar.isValid != null) {
				return cvar.isValid (value);
			}

			return true;
		}

		private	static	void	CVarChanged( CVar cvar ) {

//			// notify owner object
//
//			if (cvarFieldInfo.objectOwner != null) {
//				try {
//					var method = cvarFieldInfo.objectOwner.GetType().GetMethod("OnCVarChanged", BindingFlags.NonPublic | BindingFlags.Public
//						| BindingFlags.Instance );
//					if(method != null) {
//						method.Invoke( cvarFieldInfo.objectOwner, new object[]{ cvarFieldInfo });
//					}
//				} catch (System.Exception ex) {
//					Debug.LogException (ex);
//				}
//			}

			// call callback
			if (cvar.onChanged != null) {
				cvar.onChanged ();
			}

		}


	}

}
