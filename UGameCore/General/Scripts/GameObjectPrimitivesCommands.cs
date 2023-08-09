using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore
{
    public class GameObjectPrimitivesCommands : MonoBehaviour
    {
        public CommandManager commandManager;


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        void CreatePrimitive(ProcessCommandContext context, PrimitiveType primitiveType)
        {
            Vector3 scale = Vector3.one;
            if (context.NumArguments == 2)
                scale = context.ReadFloat() * Vector3.one;
            else if (context.NumArguments == 4)
                scale = context.ReadVector3();

            var go = GameObject.CreatePrimitive(primitiveType);
            go.transform.localScale = scale;

            var cam = Camera.main;
            if (null == cam)
                return;

            Bounds bounds = go.GetRenderersBounds();
            go.transform.position = cam.transform.position + cam.transform.forward * bounds.size.magnitude;
        }

        [CommandMethod("cube")]
        ProcessCommandResult CubeCmd(ProcessCommandContext context)
        {
            CreatePrimitive(context, PrimitiveType.Cube);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("sphere")]
        ProcessCommandResult SphereCmd(ProcessCommandContext context)
        {
            CreatePrimitive(context, PrimitiveType.Sphere);
            return ProcessCommandResult.Success;
        }
    }
}
