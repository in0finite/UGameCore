using UnityEngine;
using System.Collections;


namespace uGameCore.Utilities {

	public	class IntInputInfo {
		
		public	string	editString = "" ;
		public	int		outValue = 0 ;

	}


	public	static	class IntInput {
		
		// Returns true if text in edit box changed from last time and if successfully converted to float.
		public static bool Display( ref IntInputInfo inputInfo )
		{
			string newString = GUILayout.TextField( inputInfo.editString );
			if (newString != inputInfo.editString) {
				// String changed.
				inputInfo.editString = newString ;
				if (int.TryParse (inputInfo.editString, out inputInfo.outValue)) {
					// Converted to float.
					return true ;
				}
			}
			
			inputInfo.editString = newString;
			
			return false;
		}
		
	}

}
