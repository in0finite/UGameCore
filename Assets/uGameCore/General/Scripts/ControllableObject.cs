using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


namespace uGameCore {


	public class ControllableObject : NetworkBehaviour {

	//	protected	GameManager	mainScript = null ;
	//	protected	NetworkManager	networkManager = null;

		[System.NonSerialized]	[SyncVar(hook="PlayerOwnerChanged")]	internal	GameObject	playerOwnerGameObject = null;

		public	Player	playerOwner { get ; internal set ; }

		public	static	event System.Action<GameObject>	onStartedOnLocalPlayer = delegate {};


		protected	void Awake () {
		
		//	this.mainScript = GameManager.singleton;
		//	this.networkManager = NetworkManager.singleton;


		}

		protected	void Start () {


		}

		protected	void	Update () {
		

		}


	/*	// finds net control object of the player who owns this object
		public	void	FindNetControlObject( bool logWarningIfNotFound ) {

			bool found = false;
			foreach (GameObject go in this.mainScript.networkManager.playersNetControlObjects) {
				if (go.GetComponent<NetworkIdentity> ().netId.Value == this.netControlObjectId) {
					found = true;
					this.netControlObjectScript = go.GetComponent<Player> ();
					this.netControlObjectScript.playerGameObject = this.gameObject;
					break;
				}
			}

			if (!found)
				if (logWarningIfNotFound)
					this.mainScript.LogWarning ("Failed to find net control object (id " + this.netControlObjectId + ").");


		}
	*/


		private	void	PlayerOwnerChanged( GameObject newPlayerOwnerGameObject ) {

			if (this.isServer)	// this is host
				return;

			this.playerOwnerGameObject = newPlayerOwnerGameObject;

			if (newPlayerOwnerGameObject != null)
				this.playerOwner = newPlayerOwnerGameObject.GetComponent<Player> ();
			else
				this.playerOwner = null;

		}


		public	override	void	OnStartClient() {

			base.OnStartClient ();

			if (!NetworkStatus.IsServer ()) {
				if (this.playerOwnerGameObject)
					this.playerOwner = this.playerOwnerGameObject.GetComponent<Player> ();
				else
					this.playerOwner = null;
			}

		}

		public	override	void	OnStartLocalPlayer() {

			base.OnStartLocalPlayer ();

			onStartedOnLocalPlayer (this.gameObject);

		}


		/*
		public	void	Respawn( Vector3 pos, Quaternion q ) {

			if (this.isServer) {
				this.RpcRespawn (pos, q);
			}

			this.gameObject.transform.position = pos;
			this.gameObject.transform.rotation = q;

			this.OnRespawned ();

		}

		[ClientRpc]
		public	void	RpcRespawn( Vector3 pos, Quaternion q ) {

			if (!this.isServer) {
				this.Respawn (pos, q);
			}

		}

		// Should be called after the object is respawned.
		public	virtual	void	OnRespawned() {


		}
		*/


	}


}

