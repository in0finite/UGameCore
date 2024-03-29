﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UGameCore.Utilities;

namespace UGameCore
{

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



		void Update () {

			m_players.RemoveDeadObjects();

		}


		/// <summary>
		/// Called when new player is created.
		/// </summary>
		internal	static	void	AddNewPlayer( Player player ) {

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

		public	static	bool	IsValidPlayerName( string name )
		{
			try
			{
				ValidatePlayerName(name);
				return true;
            }
			catch
			{
				return false;
			}
		}

		public	static	void	ValidatePlayerName( string name )
		{
            if (name.Length < GetMinimumNickLength() || name.Length > GetMaxmimumNickLength())
				throw new System.ArgumentException($"Player name must be between {GetMinimumNickLength()} and {GetMaxmimumNickLength()} characters long");
            
			string unallowedChars = "<>";
            if (name.IndexOfAny(unallowedChars.ToCharArray()) >= 0)
                throw new System.ArgumentException($"Player name can not contain '{unallowedChars}' characters");
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
