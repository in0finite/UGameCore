using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.UI {

	/// <summary>
	/// Entry in a parameters view.
	/// </summary>
	public class ParametersViewEntry : MonoBehaviour {

		internal	ICanvasElement	control = null;
		internal	GameObject	labelGameObject = null;
		internal	GameObject	emptySpaceGameObject = null;
	//	internal	CVar	cvar = null;
		internal	object	editedValue = null;
		internal	ParametersView.EntryParams	entryParams = null;


		public	Text	label { get { if (this.labelGameObject)
					return this.labelGameObject.GetComponentInChildren<Text> ();
				else
					return null; } }


	}

}
