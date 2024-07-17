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

        public Component owner;

        public Func<Spectator.Context, PositionAndRotation> GetPositionAndRotation { get; set; }
        public Func<Spectator.Context, bool> RequiresCrosshair { get; set; } = (ctx) => false;
        public Func<Spectator.Context, float?> GetFieldOfView { get; set; } = (ctx) => null;
        public Func<Spectator.Context, bool> RequiresScopeImage { get; set; } = (ctx) => false;
        public Action<Spectator.SpectatedObjectChangedEvent> OnStartedSpectating { get; set; }
        public Action<Spectator.SpectatedObjectChangedEvent> OnStoppedSpectating { get; set; }
        public Action<Spectator.Context> OnSpectatingModeChanged { get; set; }


        Spectatable()
        {
            this.GetPositionAndRotation = this.GetPositionAndRotationDefault;
        }

        public PositionAndRotation GetPositionAndRotationDefault(Spectator.Context context)
        {
            this.transform.GetPositionAndRotation(out Vector3 pos, out Quaternion rot);

            if (this.rotationEulerOffset != Vector3.zero)
                rot = Quaternion.Euler(this.rotationEulerOffset) * rot;

            if (this.positionOffsetLocalSpace != Vector3.zero)
                pos += rot.TransformDirection(this.positionOffsetLocalSpace);

            return new PositionAndRotation(pos, rot);
        }
    }
}
