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

        /// <summary>
        /// If specified, spectated object will be redirected to this <see cref="Spectatable"/>.
        /// </summary>
        public Spectatable redirectedSpectatable;



        protected PositionAndRotation GetPositionAndRotationDefault(Spectator.Context context)
        {
            this.transform.GetPositionAndRotation(out Vector3 pos, out Quaternion rot);

            if (this.rotationEulerOffset != Vector3.zero)
                rot = Quaternion.Euler(this.rotationEulerOffset) * rot;

            if (this.positionOffsetLocalSpace != Vector3.zero)
                pos += rot.TransformDirection(this.positionOffsetLocalSpace);

            return new PositionAndRotation(pos, rot);
        }

        protected internal virtual void OnStoppedSpectating(Spectator.SpectatedObjectChangedEvent ev)
        {
        }

        protected internal virtual void OnStartedSpectating(Spectator.SpectatedObjectChangedEvent ev)
        {
        }

        protected internal virtual void OnSpectatingModeChanged(Spectator.Context context)
        {
        }

        public virtual PositionAndRotation GetPositionAndRotation(Spectator.Context context)
            => GetPositionAndRotationDefault(context);

        public virtual bool RequiresCrosshair(Spectator.Context context) => false;

        public virtual bool RequiresScopeImage(Spectator.Context context) => false;

        public virtual float? GetFieldOfView(Spectator.Context context) => null;
    }
}
