using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uGameCore {
	
	public class Console2Commands : MonoBehaviour {

		// Use this for initialization
		void Start () {

			uGameCore.Menu.Console.onTextSubmitted += TextSubmitted;

		}

		void TextSubmitted( string text ) {

			// Process command
			string response = "" ;
			Commands.CommandManager.ProcessCommand( text, ref response );

			if( response != "" )
				Debug.Log ( response );

		}
		

	}

}
