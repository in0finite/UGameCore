using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;


namespace uGameCore.Utilities {
	
	public class NetworkTransformServerAuthority : NetworkBehaviour {

		private class FrameInfo
		{
			public	Vector3 position = Vector3.zero;
			public	Vector3 eulers = Vector3.zero;
			public	Vector3 velocity = Vector3.zero;
			public	Vector3 angularVelocity = Vector3.zero;

			public	float	timeWhenArrived = 0 ;
		}

		private	List<FrameInfo>	frames = new List<FrameInfo>();

		[Range(0,100)]
		public	float	sendRate = 10 ;
		public	int		channel = Channels.DefaultUnreliable ;
		public	float	lerpMovementFactor = 2 ;
		public	float	lerpRotationFactor = 2 ;
		public	bool	syncRigidBodyVelocity = true;
		public	bool	syncRigidBodyAngularVelocity = true;

		private	Rigidbody	rigidBody = null;

		// position we are lerping towards
		private	Vector3	syncPosition = Vector3.zero ;
		// rotation we are lerping towards
		private	Vector3	syncRotation = Vector3.zero ;
		// position we are lerping from
		private	Vector3	syncSourcePosition = Vector3.zero ;
		// rotation we are lerping from
		private	Vector3	syncSourceRotation = Vector3.zero ;

		private	float	timeWhenLastFrameArrived = 0 ;
		private	float	updateLatency = 0;	// measured latency

		private	float	timeSinceSent = 0 ;
		private	float	timeSinceReceived = 0 ;



		void	Awake() {

			this.rigidBody = GetComponent<Rigidbody> ();

		}

		// Use this for initialization
		void Start () {
		
			syncSourcePosition = this.transform.position;
			syncPosition = this.transform.position;
			syncSourceRotation = this.transform.rotation.eulerAngles;
			syncRotation = this.transform.rotation.eulerAngles;

		}
		
		// Update is called once per frame
		void Update () {


			this.timeSinceSent += Time.deltaTime;
			this.timeSinceReceived += Time.deltaTime;


			if (this.isServer) {
				if (this.timeSinceSent >= 1.0f / this.sendRate) {
					// we should send update

					// order of data: position, rotation, velocity, angular velocity
					byte bitField = 3;

					NetworkWriter w = new NetworkWriter ();
					w.Write (transform.position);
					w.Write (transform.rotation.eulerAngles);
					if (this.syncRigidBodyVelocity && this.rigidBody != null) {
						w.Write (this.rigidBody.velocity);
						bitField |= 4;
					}
					if (this.syncRigidBodyAngularVelocity && this.rigidBody != null) {
						w.Write (this.rigidBody.angularVelocity);
						bitField |= 8;
					}

					if (0 == this.channel)
						this.RpcSendingUpdateChannel0 (bitField, w.AsArray());
					else if (1 == this.channel)
						this.RpcSendingUpdateChannel1 (bitField, w.AsArray());
					else if( 2 == this.channel )
						this.RpcSendingUpdateChannel2 (bitField, w.AsArray());

					this.timeSinceSent = 0;
				}
			}
			else if (this.isClient) {
				// lerp positon and rotation

			//	float lerp = 1.0f / Mathf.Pow (this.lerpMovementFactor, Time.deltaTime);
			//	transform.position = Vector3.Lerp( transform.position, syncPosition, lerp);

			//	transform.position += this.rigidBody.velocity * Time.deltaTime;

			//	Vector3 difference = syncPosition - transform.position;
			//	Vector3 difference = syncPosition + this.rigidBody.velocity * 1.0f / this.sendRate - transform.position ;
			//	float timeLeftToNextUpdate = Mathf.Clamp (1.0f / this.sendRate - this.timeSinceReceived, 0.0001f, Mathf.Infinity);
			//	transform.position += difference * Mathf.Clamp (Time.deltaTime / timeLeftToNextUpdate, 0, 1);

				float timePassed = Time.unscaledTime - this.timeWhenLastFrameArrived;
				float lerp = 1; //timePassed / (1.0f / this.sendRate);
			//	transform.position = Vector3.Lerp( syncSourcePosition, syncPosition, lerp );
			//	transform.rotation = Quaternion.Lerp( Quaternion.Euler( syncSourceRotation ), Quaternion.Euler( syncRotation ), lerp );

			/*	Vector3 difference = Vector3.zero;
				if (frames.Count > 0) {
					
					difference = frames [0].position - transform.position;
					if (difference.sqrMagnitude >= 0.0001f) {
						// we haven't arrived at destination

					/*	// how much time has passed since the first frame
						float timeLate = Time.unscaledTime - frames [0].timeWhenArrived;
						// how much time we have left to move to new position
						float timeLeft = Mathf.Clamp (1.0f / sendRate - timeLate, 0, Mathf.Infinity) * 3;
						int numFramesLeft = Mathf.RoundToInt (timeLeft / Time.smoothDeltaTime);
						float lerp = 1;
						if (numFramesLeft != 0)
							lerp = 1.0f / (numFramesLeft + 1);
						//	float lerp = Time.smoothDeltaTime / (1.0f / sendRate) ;
						transform.position = Vector3.Lerp (transform.position, frames [0].position, lerp);
					*	// end of comment here

					//	float move = frames [0].velocity.magnitude * Time.deltaTime * this.lerpMovementFactor;
					//	if (move * move > difference.sqrMagnitude)
					//		move = difference.magnitude;
					//	transform.position += difference.normalized * move;

					//	transform.position = frames [0].position;

						difference = frames [0].position - transform.position;
					}
				

					//	lerp = 1.0f / Mathf.Pow (this.lerpRotationFactor, Time.deltaTime);
					//	transform.rotation = Quaternion.Lerp( transform.rotation, Quaternion.Euler(syncRotation), lerp);

					//	transform.rotation = Quaternion.Euler (syncRotation);
				
					transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (frames [0].eulers), this.lerpRotationFactor);
				

					if (difference.sqrMagnitude < 0.0001f) {
						// we arrived at destination position

						// remove this frame from list
						frames.RemoveAt (0);
					}
				}
			*/

			//	transform.rotation = Quaternion.Euler( syncRotation );

			}


		}


		void	OnGUI() {

			if (!isLocalPlayer)
				return;

			GUILayout.BeginArea( new Rect( Screen.width - 300, 0, 300, Screen.height ) );

			GUILayout.Label ("num frames " + frames.Count);
			GUILayout.Label ("update latency " + (updateLatency * 1000));

			GUILayout.EndArea ();

		}


		[ClientRpc(channel=0)]
		void	RpcSendingUpdateChannel0( byte bitField, byte[] data) {

			this.ProcessUpdate (bitField, data);

		}

		[ClientRpc(channel=1)]
		void	RpcSendingUpdateChannel1( byte bitField, byte[] data) {

			this.ProcessUpdate (bitField, data);

		}

		[ClientRpc(channel=2)]
		void	RpcSendingUpdateChannel2( byte bitField, byte[] data) {

			this.ProcessUpdate (bitField, data);

		}

		void	ProcessUpdate( byte bitField, byte[] data) {

			if (this.isServer) {
				// this is host
				return ;
			}

			this.timeSinceReceived = 0;

			// order of data: position, rotation, velocity, angular velocity

			NetworkReader r = new NetworkReader (data);

			FrameInfo frame = new FrameInfo ();
			frame.timeWhenArrived = Time.unscaledTime;

			this.updateLatency = frame.timeWhenArrived - this.timeWhenLastFrameArrived;
			this.timeWhenLastFrameArrived = frame.timeWhenArrived;

			int i = 0;

			if ((bitField & (1<<i)) != 0) {
				Vector3 pos = r.ReadVector3 ();
				syncPosition = pos;
				frame.position = pos;
				syncSourcePosition = this.transform.position;
			//	transform.position = syncPosition;
			}
			i++;

			if ((bitField & (1<<i)) != 0) {
				Vector3 eulers = r.ReadVector3 ();
				syncRotation = eulers;
				frame.eulers = eulers;
				syncSourceRotation = this.transform.rotation.eulerAngles;
			}
			i++;

			if ((bitField & (1 << i)) != 0) {
				Vector3 velocity = r.ReadVector3 ();
				if (this.rigidBody != null) {
					this.rigidBody.velocity = velocity;
				}
				frame.velocity = velocity;
				syncPosition = frame.position + velocity * 1.0f / this.sendRate;
			} else {
				if (this.rigidBody != null)
					this.rigidBody.velocity = Vector3.zero;
			}
			i++;

			if ((bitField & (1 << i)) != 0) {
				Vector3 angularVelocity = r.ReadVector3 ();
				if (this.rigidBody != null)
					this.rigidBody.angularVelocity = angularVelocity;
				frame.angularVelocity = angularVelocity;
				syncRotation = frame.eulers + angularVelocity * 1.0f / this.sendRate;
			} else {
				if (this.rigidBody != null)
					this.rigidBody.angularVelocity = Vector3.zero;
			}
			i++;


			if (null == this.rigidBody) {
				this.transform.Translate (frame.position - this.transform.position, Space.World);
				this.transform.rotation = Quaternion.Euler (frame.eulers);
			} else {
				this.rigidBody.MovePosition (frame.position);
				this.rigidBody.MoveRotation (Quaternion.Euler (frame.eulers));
			}


			if (frames.Count < 100) {
				frames.Add (frame);
			}

		}


	}

}
