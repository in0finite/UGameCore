using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace uGameCore.Utilities {
	
	public class DisableInputFieldsWhenCanvasIsDisabled : MonoBehaviour
	{
		
		private	InputField[]	m_inputFields = null;
	//	private	Canvas	m_canvas = null;
	//	private	bool	m_wasEnabledLastTime = true ;


		void Start ()
		{
			m_inputFields = GetComponentsInChildren<InputField> ();
		//	m_canvas = GetComponentsInChildren<Canvas> (true).FirstOrDefault (c => c.gameObject == this.gameObject);
		//	m_wasEnabledLastTime = m_canvas.enabled;
		}

//		void Update ()
//		{
//			if (null == m_canvas)
//				return;
//
//			bool isCanvasEnabled = m_canvas.enabled;
//
//			if (m_wasEnabledLastTime != isCanvasEnabled) {
//				// canvas state changed
//				CanvasStateChanged( isCanvasEnabled );
//			}
//
//			m_wasEnabledLastTime = isCanvasEnabled;
//		}

		void OnPreCanvasStateChanged( bool newCanvasState ) {

			CanvasStateChanged (newCanvasState);

		}

		void CanvasStateChanged( bool newCanvasState ) {
			
			foreach (var inputField in m_inputFields) {
				inputField.interactable = newCanvasState;

			//	if (newCanvasState)
			//		inputField.textComponent.Rebuild ();

//				if (newCanvasState)
//					inputField.ActivateInputField ();
//				else
//					inputField.DeactivateInputField ();
//				inputField.interactable = newCanvasState;
			}

		}

	}

}
