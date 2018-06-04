using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace uGameCore.UI {
	
	public class ParametersView : MonoBehaviour {

		public	GameObject	entryPrefab = null;
		public	GameObject	inputFieldPrefab = null;
		public	GameObject	sliderPrefab = null;
		public	GameObject	togglePrefab = null;
		public	GameObject	labelPrefab = null;
		public	GameObject	emptySpacePrefab = null;

		private	RectTransform	m_container { get { return this.GetRectTransform (); } }

		private	List<ParametersViewEntry>	m_entries = new List<ParametersViewEntry>();

		[SerializeField]	private	TextAlignment	m_alignment = TextAlignment.Center;

		public	float	paddingTop = 8f;


		public enum EntryDisplayType
		{
			IntegerSlider = 1,
			IntegerTextBox,
			FloatSlider,
			FloatTextBox,
			String,
			Boolean,
			None
		}

		public class EntryParams {
			public	string displayName = "";
			public	float minValue = float.MinValue;
			public	float maxValue = float.MaxValue;
			public	int	minLength = 0;
			public	int	maxLength = 0;
			public	EntryDisplayType displayType;
		//	public	object	value = null;
		}




		void Start () {
			
		}



		public	ParametersViewEntry[]	GetEntries() {

			m_entries.RemoveAllDeadObjects ();

			return m_entries.ToArray ();

		}


		public	void	Clear() {

			foreach (var entry in m_entries.WhereAlive ()) {
				DestroyEntry (entry);
			}

			m_entries.Clear ();
		}


		private	ParametersViewEntry	AddOrUpdateEntry( ParametersViewEntry entry, EntryParams entryParams, object entryValue, 
			bool create, bool update ) {

			if (null == m_container)
				return null;


			if (create) {
				m_entries.RemoveAllDeadObjects ();
			}


			object editedValue = null;

			Transform childControl = null;
			GameObject labelGameObject = null;
			Text label = null;
			GameObject emptySpaceGameObject = null;

			if (create) {
				// create label
				labelGameObject = CreateChildInContainer (this.labelPrefab);
				label = labelGameObject.GetComponentInChildren<Text> ();
				label.text = entryParams.displayName;
			} else {
				// elements are already created => we can obtain them from entry
				label = entry.label;
				childControl = entry.transform;
				labelGameObject = entry.labelGameObject;
			}


			// create appropriate UI control for the specified display type

			var displayType = entryParams.displayType;

			if (displayType == EntryDisplayType.String || displayType == EntryDisplayType.FloatTextBox || displayType == EntryDisplayType.IntegerTextBox) {

				InputField inputField = null;

				if (create) {
					// create input field

					childControl = CreateChildInContainer (this.inputFieldPrefab).transform;
					entry = childControl.gameObject.AddComponentIfDoesntExist<ParametersViewEntry> ();

					inputField = childControl.GetComponentInChildren<InputField> ();
					if (entryParams.maxLength > 0) {
						inputField.characterLimit = entryParams.maxLength;
					}
					switch (displayType) {
					case EntryDisplayType.FloatTextBox:
						inputField.contentType = InputField.ContentType.DecimalNumber;
						break;
					case EntryDisplayType.IntegerTextBox:
						inputField.contentType = InputField.ContentType.IntegerNumber;
						break;
					}
				}

				inputField = childControl.GetComponentInChildren<InputField> ();

				if (update) {
					inputField.text = entryValue.ToString ();
				}

				entry.control = inputField;

				// get current value
				switch (displayType) {
				case EntryDisplayType.String:
					editedValue = inputField.text;
					break;
				case EntryDisplayType.FloatTextBox:
					float floatValue;
					if (float.TryParse (inputField.text, out floatValue))
						editedValue = floatValue;
					break;
				case EntryDisplayType.IntegerTextBox:
					int intValue;
					if (int.TryParse (inputField.text, out intValue))
						editedValue = intValue;
					break;
				}

			} else if (displayType == EntryDisplayType.FloatSlider || displayType == EntryDisplayType.IntegerSlider) {

				Slider slider = null;

				if (create) {
					// create slider

					childControl = CreateChildInContainer (this.sliderPrefab).transform;
					entry = childControl.gameObject.AddComponentIfDoesntExist<ParametersViewEntry> ();

					slider = childControl.GetComponentInChildren<Slider> ();
					slider.minValue = entryParams.minValue;
					slider.maxValue = entryParams.maxValue;
					slider.wholeNumbers = (displayType == EntryDisplayType.IntegerSlider);

					// add script for updating label
					var updater = slider.gameObject.AddComponentIfDoesntExist<ParametersViewSliderLabelUpdate> ();
					updater.entryName = entryParams.displayName;
					updater.label = label;
				}

				slider = childControl.GetComponentInChildren<Slider> ();

				if (update) {

					if (displayType == EntryDisplayType.FloatSlider)
						slider.value = (float)entryValue;
					else
						slider.value = (int)entryValue;

					// also add current value to label
					label.text = entryParams.displayName + " : " + entryValue.ToString ();
				}

				entry.control = slider;

				// get current value
				switch (displayType) {
				case EntryDisplayType.FloatSlider:
					editedValue = (float)slider.value;
					break;
				case EntryDisplayType.IntegerSlider:
					editedValue = (int)slider.value;
					break;
				}

			} else if (displayType == EntryDisplayType.Boolean) {
				// toggle

				if (create) {
					childControl = CreateChildInContainer (this.togglePrefab).transform;
					entry = childControl.gameObject.AddComponentIfDoesntExist<ParametersViewEntry> ();

					// reduce label height, and set text of toggle label
					label.text = "";
					label.rectTransform.SetNormalizedRectAndAdjustAnchors( Rect.zero );

					childControl.GetComponentInChildren<Text> ().text = " " + entryParams.displayName;
				}

				Toggle toggle = childControl.GetComponentInChildren<Toggle> ();

				if (update) {
					toggle.isOn = (bool)entryValue;
				}

				entry.control = toggle;

				// get current value
				editedValue = toggle.isOn ;

			}


			if (create) {
				// create empty space
				emptySpaceGameObject = CreateChildInContainer (this.emptySpacePrefab);
			}


			if (create) {
				// now that control is created, we can assign references in entry script

				entry.labelGameObject = labelGameObject;
				entry.emptySpaceGameObject = emptySpaceGameObject;
			}


			entry.editedValue = editedValue;

			if (create) {
				// TODO: make a copy
				entry.entryParams = entryParams;
			}

			return entry;

		}


		public	ParametersViewEntry	AddEntry( EntryParams entryParams ) {

			return this.AddOrUpdateEntry( null, entryParams, null, true, false );

		}

		public	void	UpdateEntry( ParametersViewEntry entry, object newValue ) {
			
			this.AddOrUpdateEntry (entry, entry.entryParams, newValue, false, true);

		}

		public	object	GetEntryValue(ParametersViewEntry entry) {
			
			this.AddOrUpdateEntry (entry, entry.entryParams, null, false, false);

			return entry.editedValue;
		}

		public	void	UpdateEntryPosition( ParametersViewEntry entry ) {

			int index = m_entries.IndexOf (entry);
			if (index >= 0)
				this.UpdateEntryPosition (m_entries [index]);

		}

		private	void	UpdateEntryPosition( ParametersViewEntry entry, int entryIndex ) {

			// TODO: apply alignment


			// the question is: do we want to resize container, or to fit controls inside container ?

			// don't change transform of container, only place controls inside it

			// controls will be placed from top to bottom


		//	Vector2 containerSize = m_container.GetRect ().size;
			float entryHeight = 40f;
			float labelHeight = 30f;
			float spaceBetween = 5f;

			float top = this.paddingTop + entryIndex * (entryHeight + labelHeight + 2 * spaceBetween);

			// update label position
			float labelTop = top;
			float labelBottom = labelTop + labelHeight;
			var rt = entry.labelGameObject.GetRectTransform ();
			float labelWidth = rt.GetRect ().size.x;
			if (entry.label)
				labelWidth = entry.label.preferredWidth * entry.label.preferredHeight / labelHeight;
			rt.SetRectAndAdjustAnchors(new Rect(-labelWidth / 2f, labelTop, labelWidth, labelHeight) );

			// update input control position
			rt = entry.control.transform.GetRectTransform ();
			Vector2 controlSize = rt.GetRect ().size;
			float controlTop = labelBottom + spaceBetween;
			rt.SetRectAndAdjustAnchors( new Rect(-controlSize.x / 2f, controlTop, controlSize.x, entryHeight) );


		}

		public	void	DeleteEntry(ParametersViewEntry entry) {

			DestroyEntry (entry);
			m_entries.Remove (entry);

		}

		private	static	void	DestroyEntry(ParametersViewEntry entry) {

			SafeDestroy (ref entry.labelGameObject);
			SafeDestroy (ref entry.emptySpaceGameObject);

			Destroy (entry.gameObject);

		}


		public	void	SetEntryInvalidState(ParametersViewEntry entry, bool isInvalid) {

			if (entry.label) {
				if (isInvalid)
					entry.label.color = Color.red;
				else
					entry.label.color = Color.white;
			}

		}

		public	void	ClearInvalidStateForAllEntries() {

			foreach (var entry in this.GetEntries()) {
				this.SetEntryInvalidState (entry, false);
			}

		}



		static	GameObject	CreateChild( GameObject prefab, Transform parent ) {

			var go = Instantiate (prefab);
			go.transform.SetParent (parent, false);
			return go;
		}

		private	GameObject	CreateChildInContainer( GameObject prefab ) {

			return CreateChild (prefab, m_container.transform);

		}






		private static void SetDirty(UnityEngine.Object obj) {



		}

		private static void SafeDestroy<T>(ref T obj) where T : UnityEngine.Object {

			if (obj) {
				Destroy (obj);
				obj = null;
			}

		}

	}

}
