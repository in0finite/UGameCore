using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;


namespace uGameCore {


	public class Spectator : MonoBehaviour {

		private	static	List<Player>	m_spectatableObjects = new List<Player> ();
		private	static	Player	m_currentlySpectatedObject = null;
	//	public	static	Player	CurrentlySpectatedObject { get { return this.m_currentlySpectatedObject; } }



		// Use this for initialization
		void Start () {


		}
		
		// Update is called once per frame
		void Update () {
		
			if (!NetworkStatus.IsServerStarted() && !NetworkStatus.IsClientConnected()) {
				m_currentlySpectatedObject = null;
				return;
			}


			if (NetworkStatus.IsClient ()) {
				if (Player.local != null) {
					if (Player.local.GetControllingGameObject () != null) {
						// controlling object is alive
						// set watched object to null
						m_currentlySpectatedObject = null;
					} else {
						// controlling object is not alive
						if (null == m_currentlySpectatedObject || null == m_currentlySpectatedObject.GetControllingGameObject ()) {
							// we are not spectating anyone
							// find object for spectating
							FindObjectForSpectating (0);
						} else {
							// we are spectating someone

						}
					}
				} else {
					// we are on client, and there is no local player
					m_currentlySpectatedObject = null;
				}
			} else if (NetworkStatus.IsServer ()) {
				// we are on dedicated server

				if (null == m_currentlySpectatedObject || null == m_currentlySpectatedObject.GetControllingGameObject()) {
					// we are not spectating anyone
					// find object for spectating
					FindObjectForSpectating (0);
				}

			}


			// just in case
			if (m_currentlySpectatedObject) {
				if (!m_currentlySpectatedObject.GetControllingGameObject ()) {
					// controlling game object of spectated player is dead
					m_currentlySpectatedObject = null;
				}
			}

		}


		void	OnGUI() {

//			if (!IsSpectating ())
//				return;
//
//			// draw spectator gui
//
//			int height = 50;
//			GUILayout.BeginArea (new Rect (0, Screen.height - height, Screen.width, height));
//
//			/*		GUILayout.BeginVertical ();
//			//	GUILayout.Label (" ");
//				GUILayout.FlexibleSpace ();
//			//	GUILayout.Label (" ");
//				GUILayout.EndVertical ();
//		*/
//
//			GUILayout.BeginHorizontal ();
//			GUILayout.Label (" ");
//			GUILayout.FlexibleSpace ();
//
//			if (GUILayout.Button ("< Prev", GUILayout.Width (80))) {
//				FindObjectForSpectating (-1);
//			}
//
//			string s = m_currentlySpectatedObject.playerName + " ";
//			if( m_currentlySpectatedObject.Team != "")
//				s += "(" + m_currentlySpectatedObject.Team + ") ";
//			s += "<color=orange>[" + m_currentlySpectatedObject.health + "]</color>" ;
//			GUILayout.Button ( s, GUILayout.Width(200) );
//
//			if (GUILayout.Button ("Next >", GUILayout.Width (80))) {
//				FindObjectForSpectating (1);
//			}
//
//			GUILayout.FlexibleSpace ();
//			GUILayout.Label (" ");
//			GUILayout.EndHorizontal ();
//
//			GUILayout.EndArea ();


		}


		/// <summary>
		/// direction: 0 - random, 1 - find next object in list, -1 - find previous object in list
		/// </summary>
		public	static	void	FindObjectForSpectating( int direction ) {
			
			// find spectatable objects
			m_spectatableObjects = new List<Player> (PlayerManager.players.Where (p => p.IsAlive()));

			if (0 == m_spectatableObjects.Count)
				return;

			direction = Math.Sign (direction);

			// find index of currently spectated object
			int index = -1 ;
			if (m_currentlySpectatedObject != null) {
				index = m_spectatableObjects.IndexOf (m_currentlySpectatedObject);
			}

			m_currentlySpectatedObject = null;

			if (index != -1) {
				// object found in list

				if (0 == direction) {
					FindRandomObjectForSpectating ();
				} else if (-1 == direction) {
					int newIndex = index - 1;
					if (newIndex < 0)
						newIndex = m_spectatableObjects.Count - 1;
					m_currentlySpectatedObject = m_spectatableObjects [newIndex];
				} else if (1 == direction) {
					int newIndex = index + 1;
					if (newIndex >= m_spectatableObjects.Count)
						newIndex = 0;
					m_currentlySpectatedObject = m_spectatableObjects [newIndex];
				}

			} else {
				// object not found in list
				// find random object

				FindRandomObjectForSpectating ();
			}


		}

		private	static	void	FindRandomObjectForSpectating() {

			if (0 == m_spectatableObjects.Count)
				return;

			foreach( var index in GetRandomIndicesInCollection(m_spectatableObjects.Count)) {

				var obj = m_spectatableObjects [index];
				if (obj.GetControllingGameObject() != null) {
					m_currentlySpectatedObject = obj;
					break;
				}

			}


		}

		private	static	IEnumerable<int>	GetRandomIndicesInCollection( int collectionLength ) {

			if (0 == collectionLength)
				yield break;

			int startIndex = UnityEngine.Random.Range (0, collectionLength);

			for (int i = startIndex, count = 0, adder = 0; count < collectionLength;
				count++, adder++, i += (count % 2 == 0 ? adder : -adder)) {

				int index = i;
				if (index < 0)
					index = collectionLength - (-index);
				if (index >= collectionLength)
					index -= collectionLength;

				yield return index;
			}

		}

		public	static	bool	IsSpectating() {

			if (null == m_currentlySpectatedObject)
				return false;
			
			return m_currentlySpectatedObject.GetControllingGameObject() != null;

		}

		public	static	GameObject	GetSpectatingGameObject() {

			if (null == m_currentlySpectatedObject)
				return null;

			return m_currentlySpectatedObject.GetControllingGameObject();
		}


	}


}

