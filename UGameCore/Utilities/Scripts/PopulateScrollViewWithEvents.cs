using UnityEngine;

namespace uGameCore.Utilities {


	public class PopulateScrollViewWithEvents : MonoBehaviour {

		private	float	m_timeSinceRemovedEvent = 0 ;
		public	float	timeToRemoveEvent = 8 ;
		public	int		maxNumEvents = 5 ;

		[SerializeField]	private	Transform	m_content = null ;
		[SerializeField]	private	GameObject	m_eventPrefab = null ;


		void Start () {
			

		}

		void Update() {

			if (this.GetNumEventsInUI() > 0) {

				m_timeSinceRemovedEvent += Time.deltaTime;

				if( m_timeSinceRemovedEvent >= this.timeToRemoveEvent ) {
					// remove event
					this.RemoveTopEventFromUI();
					m_timeSinceRemovedEvent = 0 ;
				}
			}


		}

		public void EventHappened( string eventText ) {

			this.AddEventToUI (eventText);

			if (this.GetNumEventsInUI() > this.maxNumEvents) {
				// too many events
				// remove 1 event
				this.RemoveTopEventFromUI();
			}

			m_timeSinceRemovedEvent = 0;

		}

		void	AddEventToUI( string eventText ) {

			if (null == m_eventPrefab)
				return;

			var go = Instantiate( m_eventPrefab );
			go.transform.SetParent (m_content, false);
			var text = go.GetComponentInChildren<UnityEngine.UI.Text> ();
			text.text = eventText ;

		}

		public	void	RemoveTopEventFromUI() {

			if (null == m_content)
				return;
			if (0 == GetNumEventsInUI ())
				return;

			var child = m_content.GetChild (0);
			Destroy (child.gameObject);

		}

		public	void	RemoveAllEventsFromUI() {

			if (null == m_content)
				return;

			for (int i = 0; i < m_content.childCount; i++) {
				var child = m_content.GetChild (i);
				Destroy (child.gameObject);
			}

		}

		public	int		GetNumEventsInUI() {

			if (null == m_content)
				return 0;

			return m_content.childCount;
		}


	}

}
