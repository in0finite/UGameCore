using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

namespace uGameCore {
	
	public class PlayerManager : MonoBehaviour {

		private	static	List<Player>	m_players = new List<Player> ();


		/// <summary> Returns all logged in players. </summary>
		public	static	IEnumerable<Player>	players { get {

				foreach (var player in m_players) {
					if (null == player)
						continue;
					if (!player.IsLoggedIn ())
						continue;

					yield return player;
				}

			}
		}

		/// <summary> Number of logged in players. </summary>
		public	static	int	numPlayers { get { return PlayerManager.players.Count (); } }



		// Use this for initialization
		void Start () {
			
		}

		// Update is called once per frame
		void Update () {

			m_players.RemoveAll (delegate(Player p) {

				if(null == p)
					return true ;

				return false ;
			});

		}


		/// <summary>
		/// Called when new player is created.
		/// </summary>
		public	static	void	AddNewPlayer( Player player ) {

			if (!m_players.Contains (player)) {
				m_players.Add (player);
			}

		}


		public	static	Player	GetPlayerByConnection( NetworkConnection conn ) {

			return players.FirstOrDefault (p => p.conn == conn);
		}

		public	static	Player	GetPlayerByName( string name ) {

			return players.FirstOrDefault (p => p.playerName == name);

		}

		public	static	Player	GetPlayerByGameObject( GameObject go ) {

			return players.FirstOrDefault (p => p.controllingObject == go);

		}

		public	static	IEnumerable<Player>	GetLoggedInNonBotPlayers() {

			return players.Where (p => !p.IsBot ());

		}

		public	static	bool	IsValidPlayerName( string name ) {

			if ( name.Length < GetMinimumNickLength() || name.Length > GetMaxmimumNickLength() ) {
				return false;
			}

			if (name.Contains ("<"))
				return false;
			if (name.Contains (">"))
				return false;


			return true;
		}

		/// This function does not check if string is valid. You should do it before calling it.
		public	static	string	CheckPlayerNameAndChangeItIfItExists( string name ) {
			
			bool exists = (GetPlayerByName (name) != null);

			if (!exists) {
				return name;
			}

			string newName = "";
			for (int i = 1; i < 500000; i++) {
				newName = name + " (" + i + ")";
				if (null == GetPlayerByName (newName)) {
					return newName;
				}
			}

			return null;
		}


		public	static	int		GetMaxmimumNickLength() {
			return 25;
		}

		public	static	int		GetMinimumNickLength() {
			return 2;
		}


	}

}
