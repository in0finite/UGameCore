using UGameCore.Menu;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore
{

    public class Console2Commands : MonoBehaviour {

		public Console console;


		void Start () {

			this.EnsureSerializableReferencesAssigned();
			this.console.onTextSubmitted += TextSubmitted;

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
