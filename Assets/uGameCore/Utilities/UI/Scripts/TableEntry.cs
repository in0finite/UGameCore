using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Utilities.UI {

	/// <summary>
	/// Entry in a table.
	/// </summary>
	public class TableEntry : MonoBehaviour
	{
		
		[HideInInspector]	[SerializeField]	internal	TableRow	tableRow = null;
		public TableRow TableRow { get { return this.tableRow; } }

	//	public	object	cachedValue = null;

		private	Text	m_textComponent = null;
		public Text textComponent {
			get {
				if (m_textComponent)
					return m_textComponent;
				m_textComponent = this.GetComponentInChildren<Text> ();
				return m_textComponent;
			}
		}

		public	string	entryText {
			get {
				if (this.textComponent)
					return this.textComponent.text;
				else
					return "";
			}
			set {
				if (this.textComponent) {
					this.textComponent.text = value;
					Table.MySetDirty (this.textComponent);
				}
			}
		}



		void Awake ()
		{
			m_textComponent = this.GetComponentInChildren<Text> ();
		}


	}

}
