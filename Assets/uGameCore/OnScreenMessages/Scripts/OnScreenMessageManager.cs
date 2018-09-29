using UnityEngine;
using System.Collections.Generic;

namespace uGameCore.OnScreenMessages {

	public class OnScreenMessage {
		
		public	string	text = "";
		public	Color	color = Color.black ;
		public	Color	backgroundColor = new Color (0, 0, 0, 1);	// transparent
		/// in percentage of screen dimensions
		public	Vector2	screenPos = Vector2.one / 2f;
		/// in percentage of screen dimensions
		public	Vector2	screenSize = new Vector2 (80 / 1280f, 30 / 720f);
		/// in percentage of screen dimensions
		public	Vector2	velocity = Vector2.zero ;
		/// how much time it is displayed
		public	float	timeLeft = 2 ;

	}

	public class OnScreenMessageManager : MonoBehaviour
	{

		private	static	List<OnScreenMessage>	m_onScreenMessages = new List<OnScreenMessage> ();

		public	static	System.Action<OnScreenMessage>	onMessageAdded = delegate {};

		[SerializeField]	private	bool	m_drawMessages = true ;
		public	static	bool	DrawMessages { get { return singleton.m_drawMessages; } set { singleton.m_drawMessages = value; } }

		public	static	OnScreenMessageManager	singleton { get ; private set ; }



		void Awake ()
		{
			singleton = this;
		}

		void Update ()
		{
		
			foreach (var msg in m_onScreenMessages) {
				msg.timeLeft -= Time.deltaTime;
				msg.screenPos += msg.velocity * Time.deltaTime;
			}

			m_onScreenMessages.RemoveAll( msg => msg.timeLeft <= 0 );


		}

		void OnGUI ()
		{
			// draw messages

			if (!m_drawMessages)
				return;
			
			if (!GameManager.CanGameObjectsDrawGui ())
				return;

			var originalColor = GUI.color;
			var originalBackgroundColor = GUI.backgroundColor;

			Vector2 screenSize = new Vector2 (Screen.width, Screen.height);

			foreach (var msg in m_onScreenMessages) {
				GUI.color = msg.color;
				GUI.backgroundColor = msg.backgroundColor;

				Vector2 size = Utilities.Utilities.CalcScreenSizeForContent (new GUIContent (msg.text), GUI.skin.label);

				GUI.Label (new Rect (Vector2.Scale( msg.screenPos, screenSize ), size), msg.text );
			}

			GUI.color = originalColor;
			GUI.backgroundColor = originalBackgroundColor;

		}


		public	static	void	AddMessage( OnScreenMessage msg ) {

			m_onScreenMessages.Add (msg);

			onMessageAdded (msg);
		}

		public	static	void	RemoveMessage( OnScreenMessage msg ) {

			m_onScreenMessages.Remove (msg);
		}

		public	static	OnScreenMessage[]	GetMessages() {

			return m_onScreenMessages.ToArray ();

		}

	}

}
