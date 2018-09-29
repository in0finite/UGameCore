using UnityEngine;
using System.Collections.Generic;

namespace uGameCore {
	
	public class DisableWhenInputOrDrawingIsForbidden : MonoBehaviour
	{
		[System.Serializable]
		public class DisableData {
			public	Behaviour	component = null;
			public	bool	input = true ;
			public	bool	drawing = true ;
		}

		public	List<DisableData>	m_componentsToDisable = new List<DisableData>();


		void Update ()
		{
			
			foreach (var dis in m_componentsToDisable) {
				if (null == dis.component)
					continue;

				bool disabled = dis.input && ! GameManager.CanGameObjectsReadUserInput ();
				if (!disabled)
					disabled = dis.drawing && ! GameManager.CanGameObjectsDrawGui ();

				dis.component.enabled = ! disabled ;

//				if (dis.input) {
//
//				} else if (dis.drawing) {
//
//				}

			}

		}

	}

}
