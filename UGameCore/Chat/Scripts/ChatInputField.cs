using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace uGameCore.Chat
{
	public class ChatInputField : MonoBehaviour
	{
		private	InputField	m_inputField = null;


		void Start() {

			m_inputField = this.GetComponent<InputField> ();

			m_inputField.onEndEdit.AddListener (this.OnEndEdit);

		}

		void OnEndEdit (string value)
		{
			if (Input.GetKeyDown (KeyCode.KeypadEnter) || Input.GetKeyDown (KeyCode.Return)) {
				
				// send chat message
				ChatManager.SendChatMessageToAllPlayersAsLocalPlayer (m_inputField.text);

				// clear input field
				m_inputField.text = "";

				// set focus to input field
				m_inputField.Select();
				m_inputField.ActivateInputField ();
			}
		}

	}
}

