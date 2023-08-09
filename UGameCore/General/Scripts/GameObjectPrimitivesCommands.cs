using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore
{
    public class GameObjectPrimitivesCommands : MonoBehaviour
    {
        public CommandManager commandManager;

        public float initialVelocity = 5f;
        public float initialAngularVelocity = 45f;
        public float lifeTime = 0f;
        public float lifeTimeDynamic = 30f;


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        void CreatePrimitive(ProcessCommandContext context, PrimitiveType primitiveType, bool bDynamic)
        {
            Vector3 scale = Vector3.one;
            if (context.NumArguments == 2)
                scale = context.ReadFloat() * Vector3.one;
            else if (context.NumArguments == 4)
                scale = context.ReadVector3();

            var go = GameObject.CreatePrimitive(primitiveType);
            go.transform.localScale = scale;

            float destroyTime = bDynamic ? this.lifeTimeDynamic : this.lifeTime;
            if (destroyTime > 0f)
                Destroy(go, destroyTime);

            Rigidbody rigidbody = null;
            if (bDynamic)
            {
                rigidbody = go.GetOrAddComponent<Rigidbody>();
            }

            var cam = Camera.main;
            if (null == cam)
                return;

            Bounds bounds = go.GetRenderersBounds();
            go.transform.position = cam.transform.position + cam.transform.forward * bounds.size.magnitude;

            if (bDynamic)
            {
                rigidbody.velocity = this.initialVelocity * cam.transform.forward;
                rigidbody.angularVelocity = Mathf.Deg2Rad * this.initialAngularVelocity * Random.onUnitSphere;
            }
        }

        [CommandMethod("cube")]
        ProcessCommandResult CubeCmd(ProcessCommandContext context)
        {
            CreatePrimitive(context, PrimitiveType.Cube, false);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("sphere")]
        ProcessCommandResult SphereCmd(ProcessCommandContext context)
        {
            CreatePrimitive(context, PrimitiveType.Sphere, false);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("cube_dynamic")]
        ProcessCommandResult CubeDynamicCmd(ProcessCommandContext context)
        {
            CreatePrimitive(context, PrimitiveType.Cube, true);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("sphere_dynamic")]
        ProcessCommandResult SphereDynamicCmd(ProcessCommandContext context)
        {
            CreatePrimitive(context, PrimitiveType.Sphere, true);
            return ProcessCommandResult.Success;
        }
    }
}
