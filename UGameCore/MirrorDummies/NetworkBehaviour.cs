using UnityEngine;

namespace UGameCore
{
#if !MIRROR
    public class NetworkBehaviour : MonoBehaviour
    {
        public bool isLocalPlayer => false;
        public bool isServer => true;
        public bool isClient => false;

        public uint NetworkId => 0;

        public string clientAddress => string.Empty;

        public NetworkConnection connectionToClient => null;
        public NetworkConnection connectionToServer => null;


        public virtual void OnStartClient()
        {
        }

        public virtual void OnStartLocalPlayer()
        {
        }
    }
#endif
}
