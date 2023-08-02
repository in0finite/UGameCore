using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Networking;


namespace uGameCore.Utilities {


	public class ClientSyncVarUpdater : NetworkBehaviour {


		public	MonoBehaviour	syncedScript = null;
		public	float	updateRate = 2 ;

		private	List<MemberInfo>	clientSyncVarMembers = new List<MemberInfo>() ;
		private	float	timeSinceUpdated = 0 ;



		// Use this for initialization
		void Start () {
		
			if (null != this.syncedScript) {

				Type myType = this.syncedScript.GetType ();

				MemberInfo[] allMembers = myType.GetMembers (BindingFlags.NonPublic | BindingFlags.Instance
					| BindingFlags.Public);
			
				foreach (MemberInfo memberInfo in allMembers) {
					if (memberInfo.MemberType != MemberTypes.Field && memberInfo.MemberType != MemberTypes.Property)
						continue;

					if( memberInfo.GetCustomAttributes ( typeof(ClientSyncVarAttribute), true).Length > 0 ) {
						this.clientSyncVarMembers.Add (memberInfo);
					}
				}

			//	Debug.Log ("Found " + this.clientSyncVarMembers.Count + " client sync var members for " + this.gameObject.name);
			}


		}
		
		// Update is called once per frame
		void Update () {
		
			if (!this.IsInputting())
				return;

			if (null == this.syncedScript)
				return;

			if (this.updateRate <= 0)
				return;

			this.timeSinceUpdated += Time.deltaTime;

			if (this.timeSinceUpdated < 1.0f / this.updateRate) {
				// not enough time passed
				return ;
			}


			// send bitfield of changed values
			// send changed values

		//	byte[] bitField = new byte[Mathf.CeilToInt (this.clientSyncVarFields.Count / 8.0f)];
		//	for (int i = 0; i < bitField.Length; i++)
		//		bitField [i] = 0;
			Int32 bitField = 0 ;
			
			NetworkWriter w = new NetworkWriter ();

			int j = -1 ;
			foreach (MemberInfo member in this.clientSyncVarMembers) {
				j++ ;

				object value = null;
				Type type = null;
				this.GetMemberValueAndType (member, ref value, ref type);

				// check if value changed
				ClientSyncVarAttribute v = (ClientSyncVarAttribute) member.GetCustomAttributes (typeof(ClientSyncVarAttribute), true) [0] ;
				if (v.lastValue == null) {
					// var has not been updated so far

				} else {
					if (value != null && value.ToString () == v.lastValue.ToString ()) {
						continue;
					}
				}

				v.lastValue = value;

				bitField |= (1 << j);

				if (type == typeof(int)) {
					w.Write ( (int) value);
				} else if (type == typeof(float)) {
					w.Write ( (float) value);
				} else if (type == typeof(string)) {
					w.Write ( (string) value);
				} else if (type == typeof(Vector3)) {
					w.Write ( (Vector3) value);
				}

			}

			this.CmdSendingVars ( bitField, w.AsArray ());


			this.timeSinceUpdated = 0;


		}


		[Command]
		private	void	CmdSendingVars( Int32 dirtyBits, byte[] bytes ) {

			this.ProcessVars (dirtyBits, bytes);

			this.RpcSendingVars (dirtyBits, bytes);

		}

		[ClientRpc]
		private	void	RpcSendingVars( Int32 dirtyBits, byte[] bytes ) {

			if (this.IsInputting())
				return;
			
			this.ProcessVars (dirtyBits, bytes);
		}


		private	void	ProcessVars( Int32 dirtyBits, byte[] bytes ) {

			NetworkReader r = new NetworkReader (bytes);

		//	BitArray bitArray = new BitArray (32, false);
		//	for (int i = 0; i < bitArray.Length; i++) {
		//		bitArray[i] = ;
		//	}

			int i = -1;
			foreach (MemberInfo member in this.clientSyncVarMembers) {
				i++;

				if ( (dirtyBits & (1 << i)) == 0 )
					continue;
				
				object value = null;
				Type type = null;
				this.GetMemberValueAndType (member, ref value, ref type);

				if (type == typeof(int)) {
					value = r.ReadInt32 ();
				} else if (type == typeof(float)) {
					value = r.ReadSingle ();
				} else if (type == typeof(string)) {
					value = r.ReadString ();
				} else if (type == typeof(Vector3)) {
					value = r.ReadVector3 ();
				}

				if (value != null) {
				//	Debug.Log ("Changing client sync var " + field.Name + ", new value " + value.ToString() );
					this.SetMemberValue( member, value );
				}

			}

		}

		private	void	GetMemberValueAndType( MemberInfo member, ref object value, ref Type type ) {

			if( member.MemberType == MemberTypes.Field ) {
				value = ((FieldInfo) member).GetValue (this.syncedScript);
				type = ((FieldInfo) member).FieldType;
			} else if( member.MemberType == MemberTypes.Property ) {
				value = ((PropertyInfo) member).GetValue (this.syncedScript, null);
				type = ((PropertyInfo) member).PropertyType;
			}

		}

		private	void	SetMemberValue( MemberInfo member, object value ) {

			if( member.MemberType == MemberTypes.Field ) {
				((FieldInfo) member).SetValue (this.syncedScript, value);
			} else if( member.MemberType == MemberTypes.Property ) {
				((PropertyInfo) member).SetValue (this.syncedScript, value, null);
			}

		}


	}


}

