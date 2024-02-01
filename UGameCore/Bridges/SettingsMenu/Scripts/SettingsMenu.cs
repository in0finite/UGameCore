using System.Collections.Generic;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Menu
{

    public class SettingsMenu : MonoBehaviour {

        public enum CVarDisplayType
        {
            IntegerSlider = 1,
            IntegerTextBox,
            FloatSlider,
            FloatTextBox,
            String,
            Boolean,
            None
        }

        public	class Entry
		{
			public Transform child = null;
			public ICanvasElement control = null;
			public Text label = null;
			public ConfigVar cvar = null;
			public ConfigVarValue editedValue;

			//internal Color originalImageColor = Color.white;
		}


		public CVarManager configVarManager;

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

		void Start()
		{
			this.EnsureSerializableReferencesAssigned();

            this.GenerateSettingsMenuBasedOnCVars();
            UpdateMenuBasedOnCVars();
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

			foreach (var pair in singleton.configVarManager.ConfigVars)
			{
				ConfigVar cvar = pair.Value;

                Entry entry = new Entry ();

				entry.cvar = cvar;

				ConfigVarValue cvarValue = cvar.GetValue();

				//	var childLabel = singleton.settingsMenuScrollViewContent.transform.GetChild (i);
				Transform childControl = null;
				Text label = null;

				if (create) {
					// create label
					label = CreateChild (singleton.labelPrefab, singleton.settingsMenuScrollViewContent.transform).GetComponentInChildren<Text> ();
					label.text = cvar.FinalSerializationName;
				} else {
					// elements are already created => we can obtain them from transform
					label = singleton.settingsMenuScrollViewContent.transform.GetChild (i).GetComponentInChildren<Text>();
					childControl = singleton.settingsMenuScrollViewContent.transform.GetChild (i + 1);
				}

				i += 3; // also skip empty space

                ConfigVarValue editedValue = default;

				var displayType = SettingsMenu.GetCVarDisplayType (cvar);

				if (displayType == CVarDisplayType.String || displayType == CVarDisplayType.FloatTextBox || displayType == CVarDisplayType.IntegerTextBox) {

					InputField inputField = null;

					StringConfigVar stringConfigVar = (StringConfigVar)cvar;

					if (create) {
						// create input field

						childControl = CreateChild (singleton.inputFieldPrefab, singleton.settingsMenuScrollViewContent.transform).transform;

						inputField = childControl.GetComponentInChildren<InputField> ();
						if (stringConfigVar.maxNumCharacters > 0) {
							inputField.characterLimit = stringConfigVar.maxNumCharacters;
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
						inputField.text = cvar.SaveValueToString(cvarValue);
					}

					entry.control = inputField;

					// get current value
					switch (displayType) {
					case CVarDisplayType.String:
						editedValue.ReferenceValue = inputField.text;
						break;
					case CVarDisplayType.FloatTextBox:
						float floatValue;
						if (float.TryParse (inputField.text, out floatValue))
							editedValue.Union16Value.Part1.FloatValuePart1 = floatValue;
						break;
					case CVarDisplayType.IntegerTextBox:
						int intValue;
						if (int.TryParse (inputField.text, out intValue))
							editedValue.Union16Value.Part1.IntValuePart1 = intValue;
						break;
					}

				} else if (displayType == CVarDisplayType.FloatSlider || displayType == CVarDisplayType.IntegerSlider) {

					Slider slider = null;

					if (create) {
						// create slider

						childControl = CreateChild (singleton.sliderPrefab, singleton.settingsMenuScrollViewContent.transform).transform;

						var floatConfigVar = cvar as FloatConfigVar;
                        var intConfigVar = cvar as IntConfigVar;

                        float minValue = floatConfigVar?.MinValue.Value ?? intConfigVar.MinValue.Value;
                        float maxValue = floatConfigVar?.MaxValue.Value ?? intConfigVar.MaxValue.Value;

                        slider = childControl.GetComponentInChildren<Slider> ();
						slider.minValue = minValue;
						slider.maxValue = maxValue;
						slider.wholeNumbers = (displayType == CVarDisplayType.IntegerSlider);

						// add script for updating label
						var updater = slider.gameObject.AddComponent<SettingsMenuSliderLabelUpdate> ();
						updater.cvarName = cvar.FinalSerializationName;
						updater.label = label;
					}

					slider = childControl.GetComponentInChildren<Slider> ();

					if (update) {
						
						if (displayType == CVarDisplayType.FloatSlider)
							slider.value = cvarValue.Union16Value.Part1.FloatValuePart1;
						else
							slider.value = cvarValue.Union16Value.Part1.IntValuePart1;

						// also add current value to label
						label.text = cvar.FinalSerializationName + " : " + cvar.SaveValueToString(cvarValue);
					}

					entry.control = slider;

					// get current value
					switch (displayType) {
					case CVarDisplayType.FloatSlider:
						editedValue.Union16Value.Part1.FloatValuePart1 = slider.value;
						break;
					case CVarDisplayType.IntegerSlider:
						editedValue.Union16Value.Part1.IntValuePart1 = (int)slider.value;
						break;
					}

				} else if (displayType == CVarDisplayType.Boolean) {
					// toggle

					if (create) {
						childControl = CreateChild (singleton.togglePrefab, singleton.settingsMenuScrollViewContent.transform).transform;

						// reduce label height, and set text of toggle label
						label.text = "";
						label.rectTransform.SetNormalizedRectAndAdjustAnchors( Rect.zero );

						childControl.GetComponentInChildren<Text> ().text = " " + cvar.FinalSerializationName;
					}

					var toggle = childControl.GetComponentInChildren<Toggle> ();

					if (update) {
						toggle.isOn = cvarValue.Union16Value.Part1.BoolValue;
					}

					entry.control = toggle;

					// get current value
					editedValue.Union16Value.Part1.BoolValue = toggle.isOn;

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

		/// <summary>
		/// Returns list of indexes of values which are not valid. Returns empty list if all values are correct.
		/// </summary>
		public	static	List<int>	AreSettingsValid( List<ConfigVar> cvars, List<ConfigVarValue> values ) {

			var	invalidValuesIndexes = new List<int> ();

			for (int i=0; i < cvars.Count ; i++) {
				if (!singleton.configVarManager.IsCVarValueValid (cvars [i], values [i])) {
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

		public static CVarDisplayType GetCVarDisplayType(ConfigVar cvar)
		{
			CVarDisplayType displayType = CVarDisplayType.None;

			if (cvar is StringConfigVar)
			{
				displayType = CVarDisplayType.String;
			}
			else if (cvar is IntConfigVar intConfigVar)
			{
				if (intConfigVar.MinValue.HasValue && intConfigVar.MaxValue.HasValue)
				{
					// there is limit on the number -> use slider
					displayType = CVarDisplayType.IntegerSlider;
				}
				else
				{
					// there is no limit on the number -> use text box
					displayType = CVarDisplayType.IntegerTextBox;
				}
			}
            else if (cvar is FloatConfigVar floatConfigVar)
            {
                if (floatConfigVar.MinValue.HasValue && floatConfigVar.MaxValue.HasValue)
                {
                    // there is limit on the number -> use slider
                    displayType = CVarDisplayType.FloatSlider;
                }
                else
                {
                    // there is no limit on the number -> use text box
                    displayType = CVarDisplayType.FloatTextBox;
                }
            }
            else if (cvar is BoolConfigVar)
			{
				// toggle
				displayType = CVarDisplayType.Boolean;
			}

			return displayType;
		}
	}
}
