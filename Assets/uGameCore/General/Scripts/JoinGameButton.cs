using UnityEngine;
using UnityEngine.UI;

namespace uGameCore {
	
	public class JoinGameButton : MonoBehaviour
	{
		private	Button	m_button = null;
		private	Text	m_text = null;
		private	System.Text.StringBuilder	m_stringBuilder = new System.Text.StringBuilder( 15 );

		public	bool	changeButtonTextWhileConnecting = true;
		public	string	prefixText = "Join" ;
		public	bool	disableButtonWhileClientIsActive = true;

		private	string	m_originalButtonText = "";



		void Awake ()
		{
			m_button = this.GetComponent<Button> ();
			m_text = this.GetComponentInChildren<Text> ();
		}

		void Start ()
		{
			if (m_text)
				m_originalButtonText = m_text.text;
		}

		void Update ()
		{

			if (this.changeButtonTextWhileConnecting) {
				if (m_text) {
					string newText = "";

					if (NetworkStatus.IsClientConnecting ()) {
						// if client is connecting, change text

						m_stringBuilder.Length = 0;
						m_stringBuilder.Append (this.prefixText);
						int numDots = ((int)Time.realtimeSinceStartup) % 4;
						for (int i = 0; i < numDots; i++) {
							m_stringBuilder.Append (".");
						}

						newText = m_stringBuilder.ToString ();

					} else {
						// restore original text
						newText = m_originalButtonText ;
					}

					if (newText != m_text.text)
						m_text.text = newText;
				}
			}

			if (this.disableButtonWhileClientIsActive) {
				// if client is active, disable button
				if (m_button) {
					m_button.interactable = ! NetworkStatus.IsClientActive ();
				}
			}

		}

	}

}