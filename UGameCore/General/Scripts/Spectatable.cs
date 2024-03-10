using System;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore
{
    [DisallowMultipleComponent]
    public class Spectatable : MonoBehaviour
    {
        public Vector3 positionOffsetLocalSpace;
        public Vector3 rotationEulerOffset;

        public Func<(Vector3 pos, Quaternion rot)> PositionAndRotationFunc { get; set; }


        Spectatable()
        {
            this.PositionAndRotationFunc = this.GetPositionAndRotationDefault;
        }

        public (Vector3 pos, Quaternion rot) GetPositionAndRotationDefault()
        {
            this.transform.GetPositionAndRotation(out Vector3 pos, out Quaternion rot);

            if (this.rotationEulerOffset != Vector3.zero)
                rot = Quaternion.Euler(this.rotationEulerOffset) * rot;

            if (this.positionOffsetLocalSpace != Vector3.zero)
                pos += rot.TransformDirection(this.positionOffsetLocalSpace);

            return (pos, rot);
        }
    }
}
