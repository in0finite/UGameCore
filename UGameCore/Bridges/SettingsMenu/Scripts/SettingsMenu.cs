using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Menu {
	
	public class SettingsMenu : MonoBehaviour {

		public	class Entry
		{
			public Transform child = null;
			public ICanvasElement control = null;
			public Text label = null;
			public CVar cvar = null;
			public object editedValue = null;

			//internal Color originalImageColor = Color.white;
		}


		private	RectTransform	settingsMenuScrollViewContent = null;

		public	GameObject	inputFieldPrefab = null;
		public	GameObject	sliderPrefab = null;
		public	GameObject	togglePrefab = null;
		public	GameObject	labelPrefab = null;
		public	GameObject	emptySpacePrefab = null;

		private	Vector2		optionsMenuScrollBarPosition = new Vector2 (0, 0);

	//	public	string	settingsMenuName = "";

		public	static	SettingsMenu	singleton { get ; private set ; }


		void Awake() {

			singleton = this;

			// find scroll view content
			this.settingsMenuScrollViewContent = Utilities.Utilities.FindObjectOfTypeOrLogError<SettingsMenuScrollViewContent> ()
				.GetComponent<RectTransform> ();

		}

		void Start () {

			CVarManager.onProcessedConfiguration += () => {
				this.GenerateSettingsMenuBasedOnCVars ();
				UpdateMenuBasedOnCVars ();
			};

		}

		private	void	GenerateSettingsMenuBasedOnCVars() {

			var en = GetEntries (true, false).GetEnumerator();
			while (en.MoveNext()) {

			}


//			if (null == this.settingsMenuScrollViewContent)
//				return;
//
//			// delete all children
//			for(int i=0; i < this.settingsMenuScrollViewContent.transform.childCount; i++) {
//				Destroy (this.settingsMenuScrollViewContent.transform.GetChild (i));
//			}
//
//			// go through cvars and create UI controls
//			foreach (var cvar in CVarManager.CVars) {
//
//				// create label
//				var label = CreateChild( this.labelPrefab, this.settingsMenuScrollViewContent.transform ).GetComponentInChildren<Text>();
//				label.text = cvar.displayName;
//
//				// create input control
//				var displayType = SettingsMenu.GetCVarDisplayType (cvar) ;
//				if (displayType == CVarDisplayType.String || displayType == CVarDisplayType.FloatTextBox) {
//					// create input field
//
//					var inputField = CreateChild( this.inputFieldPrefab, this.settingsMenuScrollViewContent.transform ).GetComponentInChildren<InputField>();
//					if (cvar.maxLength > 0) {
//						inputField.characterLimit = cvar.maxLength;
//					}
//
//				} else if (displayType == CVarDisplayType.FloatSlider) {
//					// create slider
//
//					var slider = CreateChild (this.sliderPrefab, this.settingsMenuScrollViewContent.transform).GetComponentInChildren<Slider>();
//					slider.minValue = cvar.minValue;
//					slider.maxValue = cvar.maxValue;
//
//					// add script for updating label
//					var updater = slider.gameObject.AddComponent<SettingsMenuSliderLabelUpdate>();
//					updater.cvarName = cvar.displayName;
//					updater.label = label;
//				}
//
//				// create empty space
//				CreateChild( this.emptySpacePrefab, this.settingsMenuScrollViewContent.transform );
//
//			}

		}

		static	GameObject	CreateChild( GameObject prefab, Transform parent ) {

			var go = Instantiate (prefab);
			go.transform.SetParent (parent, false);
			return go;
		}

		public	static	void	UpdateMenuBasedOnCVars() {

//			if (null == singleton.settingsMenuScrollViewContent)
//				return;
//			
//		//	foreach (var cvarField in CVarManager.CVarFields) {
//			foreach(var entry in GetEntries()) {
//				var cvar = entry.cvar;
//				var cvarValue = CVarManager.GetCVarValue (cvar);
//
//				var childLabel = entry.label;
//				var childControl = entry.child;
//
//				// set label text
//				var label = childLabel.GetComponentInChildren<Text>();
//			//	label.text = cvar.name;
//
//				// update input controls
//				var displayType = SettingsMenu.GetCVarDisplayType (cvar) ;
//				if (displayType == CVarDisplayType.String || displayType == CVarDisplayType.FloatTextBox) {
//					
//					var inputField = childControl.GetComponentInChildren<InputField>();
//					inputField.text = cvarValue.ToString ();
//
//				} else if (displayType == CVarDisplayType.FloatSlider) {
//					
//					var slider = childControl.GetComponentInChildren<Slider>();
//					slider.value = (float) cvarValue;
//
//					// also add current value to label
//					label.text = cvar.displayName + " : " + cvarValue.ToString() ;
//				}
//
//			}


			var en = GetEntries (false, true).GetEnumerator();
			while (en.MoveNext()) {

			}

		}

		public	static	IEnumerable<Entry>	GetEntries() {

			return GetEntries (false, false);

		}

		private	static	IEnumerable<Entry>	GetEntries( bool create, bool update ) {

			if (null == singleton.settingsMenuScrollViewContent)
				yield break;

			int i = 0;

			if (create) {
				// delete all children
				for(i=0; i < singleton.settingsMenuScrollViewContent.transform.childCount; i++) {
					Destroy (singleton.settingsMenuScrollViewContent.transform.GetChild (i));
				}
			}

			i = 0;

			foreach (var cvar in CVarManager.CVars) {
				Entry entry = new Entry ();

				entry.cvar = cvar;

				var cvarValue = CVarManager.GetCVarValue (cvar);

				//	var childLabel = singleton.settingsMenuScrollViewContent.transform.GetChild (i);
				Transform childControl = null;
				Text label = null;

				if (create) {
					// create label
					label = CreateChild (singleton.labelPrefab, singleton.settingsMenuScrollViewContent.transform).GetComponentInChildren<Text> ();
					label.text = cvar.displayName;
				} else {
					// elements are already created => we can obtain them from transform
					label = singleton.settingsMenuScrollViewContent.transform.GetChild (i).GetComponentInChildren<Text>();
					childControl = singleton.settingsMenuScrollViewContent.transform.GetChild (i + 1);
				}

				i += 3;	// also skip empty space

				object editedValue = null;

				var displayType = SettingsMenu.GetCVarDisplayType (cvar);

				if (displayType == CVarDisplayType.String || displayType == CVarDisplayType.FloatTextBox || displayType == CVarDisplayType.IntegerTextBox) {

					InputField inputField = null;

					if (create) {
						// create input field

						childControl = CreateChild (singleton.inputFieldPrefab, singleton.settingsMenuScrollViewContent.transform).transform;

						inputField = childControl.GetComponentInChildren<InputField> ();
						if (cvar.maxLength > 0) {
							inputField.characterLimit = cvar.maxLength;
						}
						switch (displayType) {
						case CVarDisplayType.FloatTextBox:
							inputField.contentType = InputField.ContentType.DecimalNumber;
							break;
						case CVarDisplayType.IntegerTextBox:
							inputField.contentType = InputField.ContentType.IntegerNumber;
							break;
						}
					}

					inputField = childControl.GetComponentInChildren<InputField> ();

					if (update) {
						inputField.text = cvarValue.ToString ();
					}

					entry.control = inputField;

					// get current value
					switch (displayType) {
					case CVarDisplayType.String:
						editedValue = inputField.text;
						break;
					case CVarDisplayType.FloatTextBox:
						float floatValue;
						if (float.TryParse (inputField.text, out floatValue))
							editedValue = floatValue;
						break;
					case CVarDisplayType.IntegerTextBox:
						int intValue;
						if (int.TryParse (inputField.text, out intValue))
							editedValue = intValue;
						break;
					}

				} else if (displayType == CVarDisplayType.FloatSlider || displayType == CVarDisplayType.IntegerSlider) {

					Slider slider = null;

					if (create) {
						// create slider

						childControl = CreateChild (singleton.sliderPrefab, singleton.settingsMenuScrollViewContent.transform).transform;

						slider = childControl.GetComponentInChildren<Slider> ();
						slider.minValue = cvar.minValue;
						slider.maxValue = cvar.maxValue;
						slider.wholeNumbers = (displayType == CVarDisplayType.IntegerSlider);

						// add script for updating label
						var updater = slider.gameObject.AddComponent<SettingsMenuSliderLabelUpdate> ();
						updater.cvarName = cvar.displayName;
						updater.label = label;
					}

					slider = childControl.GetComponentInChildren<Slider> ();

					if (update) {
						
						if (displayType == CVarDisplayType.FloatSlider)
							slider.value = (float)cvarValue;
						else
							slider.value = (int)cvarValue;

						// also add current value to label
						label.text = cvar.displayName + " : " + cvarValue.ToString ();
					}

					entry.control = slider;

					// get current value
					switch (displayType) {
					case CVarDisplayType.FloatSlider:
						editedValue = (float)slider.value;
						break;
					case CVarDisplayType.IntegerSlider:
						editedValue = (int)slider.value;
						break;
					}

				} else if (displayType == CVarDisplayType.Boolean) {
					// toggle

					if (create) {
						childControl = CreateChild (singleton.togglePrefab, singleton.settingsMenuScrollViewContent.transform).transform;

						// reduce label height, and set text of toggle label
						label.text = "";
						label.rectTransform.SetNormalizedRectAndAdjustAnchors( Rect.zero );

						childControl.GetComponentInChildren<Text> ().text = " " + cvar.displayName;
					}

					var toggle = childControl.GetComponentInChildren<Toggle> ();

					if (update) {
						toggle.isOn = (bool)cvarValue;
					}

					entry.control = toggle;

					// get current value
					editedValue = toggle.isOn ;

				}


				if (create) {
					// create empty space
					CreateChild (singleton.emptySpacePrefab, singleton.settingsMenuScrollViewContent.transform);
				}


				entry.child = childControl;
				entry.label = label;
				entry.editedValue = editedValue;

				if (create) {
					// add script which will remember original image color
					entry.control.transform.gameObject.AddComponentIfDoesntExist<SettingsMenuEntryScript> ();
				}


				yield return entry;
			}

		}


		void OnGUI() {

//			if ("" == this.settingsMenuName)
//				return;
//			
//			if (this.settingsMenuName != Menu.MenuController.singleton.canvasIndex)
//				return;
			
		//	this.DrawOptionsMenu ();

		}

		private	void	DrawOptionsMenu() {

			int areaWidth = Screen.width / 7 * 3 ;	// 90 (640), 200 (1366), 270 (1920)
			int areaHeight = Screen.height / 2;	// 240 (480), 380 (768), 540 (1080)
			int buttonHeight = (int) (35.0f * Screen.height / 650.0f / 1.5f ) ;
			int x = Screen.width / 2 - areaWidth / 2;
			int y = Screen.height / 2 - areaHeight / 2;


			// Draw box as background.
			int box_offset_x = 30;
			int box_offset_y = 20;
			GUI.Box( new Rect( x - box_offset_x, y - box_offset_y, areaWidth + 2 * box_offset_x, areaHeight + 2 * box_offset_y ), "OPTIONS MENU" );


			GUILayout.BeginArea (new Rect ( x, y, areaWidth, areaHeight));


			this.optionsMenuScrollBarPosition = GUILayout.BeginScrollView (this.optionsMenuScrollBarPosition);

			// display controls to edit cvars

			var cvarsToChange = new List<CVar> ();
			var changedValues = new List<object> ();

			foreach (var cvar in CVarManager.CVars) {
				
				// display name
				string s = cvar.displayName ;
				if( s.Length < 1 )
					s = cvar.name ;
				GUILayout.Label( s );

				var currentCvarValue = CVarManager.GetCVarValue (cvar);
				object editedValue = null;

				var displayType = SettingsMenu.GetCVarDisplayType (cvar);
				if( displayType == CVarDisplayType.String ) {

					editedValue = GUILayout.TextField( (string) currentCvarValue, cvar.maxLength );

				} else if( displayType == CVarDisplayType.FloatSlider ) {

					// display current value
					GUILayout.Label( currentCvarValue.ToString() );
					// display slider
					editedValue = GUILayout.HorizontalSlider( (float) currentCvarValue, cvar.minValue, cvar.maxValue );
				}

				// compare with current value
				if (!currentCvarValue.Equals (editedValue)) {
					// value is changed
					cvarsToChange.Add(cvar);
					changedValues.Add (editedValue);
				}

			}

			GUILayout.EndScrollView ();

			GUILayout.Space (20);

			GUILayout.BeginHorizontal ();

			// OK button
			//	if (GUILayout.Button ("Save", GUILayout.Width (50), GUILayout.Height (20))) {
			if (GameManager.DrawButtonWithCalculatedSize("Save")) {
				
			}

			// Cancel button
			//	if (GUILayout.Button ("Cancel", GUILayout.Width (60), GUILayout.Height (20))) {
			if (GameManager.DrawButtonWithCalculatedSize ("Cancel")) {
				// populate options window with current settings, and exit options menu.

			//	CVarManager.ReadCVarsFromPlayerPrefs ();

				MenuManager.singleton.OpenParentMenu ();
			}

			GUILayout.FlexibleSpace ();

			//	if (GUILayout.Button ("Reset to defaults", GUILayout.Width (100), GUILayout.Height (20))) {
			if (GameManager.DrawButtonWithCalculatedSize ("Reset to defaults")) {

			//	CVarManager.ResetAllCVarsToDefaultValues ();

			//	this.UpdateMenuBasedOnCVars ();

			}

			GUILayout.EndHorizontal ();


			GUILayout.EndArea ();

		}

		/// <summary>
		/// Returns list of indexes of values which are not valid. Returns empty list if all values are correct.
		/// </summary>
		public	static	List<int>	AreSettingsValid( List<CVar> cvars, List<object> values ) {

			var	invalidValuesIndexes = new List<int> ();

			for (int i=0; i < cvars.Count ; i++) {
				if (!CVarManager.IsCVarValueValid (cvars [i], values [i])) {
					invalidValuesIndexes.Add (i);
				}
			}

			return invalidValuesIndexes;
		}

		public	static	void	SetEntryValidState(Entry entry, bool isValid) {

			if (entry.control != null && entry.control.transform != null) {
				var image = entry.control.transform.GetComponent<Image> ();
				if (image) {
					if (isValid)
						image.color = entry.control.transform.GetComponent<SettingsMenuEntryScript>().originalImageColor;
					else
						image.color = Color.red;
				}
			}

		}

		public	static	void	ResetValidStateForAllEntries() {

			foreach(var entry in GetEntries()) {
				SetEntryValidState (entry, true);
			}

		}

		public	static CVarDisplayType	GetCVarDisplayType( CVar cvar ) {

			CVarDisplayType displayType = CVarDisplayType.None;

			if (cvar.cvarType == typeof(string)) {
				
				displayType = CVarDisplayType.String;

			} else if (cvar.cvarType == typeof(float) || cvar.cvarType == typeof(int)) {
				
				if (cvar.minValue != float.MinValue && cvar.maxValue != float.MaxValue) {
					// there is limit on the number -> use slider
					if (cvar.cvarType == typeof(float))
						displayType = CVarDisplayType.FloatSlider;
					else
						displayType = CVarDisplayType.IntegerSlider;
				} else {
					// there is no limit on the number -> use text box
					if (cvar.cvarType == typeof(float))
						displayType = CVarDisplayType.FloatTextBox;
					else
						displayType = CVarDisplayType.IntegerTextBox;
				}
			} else if (cvar.cvarType == typeof(bool)) {
				// toggle
				displayType = CVarDisplayType.Boolean ;
			}

			return displayType;
		}

	}

}
