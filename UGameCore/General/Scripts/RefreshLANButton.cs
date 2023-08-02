using UnityEngine;
using UnityEngine.UI;
using uGameCore.Utilities.UI;

namespace uGameCore {

	/// <summary>
	/// Starts LAN scan when attached button is clicked, and clears table.
	/// </summary>
	public class RefreshLANButton : MonoBehaviour
	{

		Button	m_button;
		Text	m_buttonTextComponent;
		string	m_originalButtonText = "";

		public	Table	table;



		void Awake ()
		{
			m_button = this.GetComponent<Button> ();
			if (m_button) {
				m_button.onClick.AddListener (() => this.OnClicked ());
			}

			m_buttonTextComponent = this.GetComponentInChildren<Text> ();
			if (m_buttonTextComponent)
				m_originalButtonText = m_buttonTextComponent.text;

		}

		void Start ()
		{
			
		}

		void OnEnable()
		{

			if (null == LANScan2UI.singleton) {
				// singleton was not set up yet
				return;
			}

			if (this.table && this.table.gameObject.activeSelf && this.table.gameObject.activeInHierarchy) {
				// table was just activated ?
				// but what if it is still not activated ? => this script could be activated before table

				/*
				// insert all discovered games into table
				foreach(var data in NetBroadcast.allReceivedBroadcastData) {
					LANScan2UI.HandleBroadcastData( this.table, data );
				}
				// remove all discovered games from list
				NetBroadcast.allReceivedBroadcastData.Clear();

				// start scan if it is not started
				if (!NetBroadcast.IsListening ()) {
					NetBroadcast.StartListening ();
					LANScan2UI.StopListeningLater ();
				}
				*/
			}

		}

		void Update ()
		{
			if (null == m_button)
				return;

			// update button text and interactable state

			if (NetBroadcast.IsListening ()) {

				string buttonText = m_originalButtonText;
				int numDots = (int)Time.time % 3 + 1;
				for (int i = 0; i < numDots; i++) {
					buttonText += ".";
				}

				if (m_buttonTextComponent)
					m_buttonTextComponent.text = buttonText;

				m_button.interactable = false;

			} else {
				if (m_buttonTextComponent)
					m_buttonTextComponent.text = m_originalButtonText;

				m_button.interactable = true;
			}

		}

		void OnClicked() {

			// clear the table, and start listening if not already started

			NetBroadcast.allReceivedBroadcastData.Clear ();

			if (this.table) {
				this.table.Clear ();
				this.table.UpdateTable ();
			}

			if (!NetBroadcast.IsListening ()) {
				NetBroadcast.StartListening ();
				LANScan2UI.StopListeningLater ();
			}
			
		}


	}

}
