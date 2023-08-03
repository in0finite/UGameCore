using UnityEngine;

namespace UGameCore
{
    public class NetworkIdentity : MonoBehaviour
    {
        public bool isLocalPlayer => false;
        public bool isServer => true;
        public bool isClient => false;

        public uint NetworkId => 0;
    }
}
