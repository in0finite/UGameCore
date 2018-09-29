

namespace uGameCore.Utilities {
		
	[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
	public class ClientSyncVarAttribute : System.Attribute
	{
		public	object	lastValue = null;


	}

}
