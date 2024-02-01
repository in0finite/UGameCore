using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore
{
    public class RenderingCommands : MonoBehaviour
    {
        public CommandManager commandManager;
        

        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        [CommandMethod("r_static_batch", description = "Performs static batching for given GameObject")]
        ProcessCommandResult StaticBatchCmd(ProcessCommandContext context)
        {
            GameObject go = F.FindObjectByInstanceId<GameObject>(context.ReadInt());

            StaticBatchingUtility.Combine(go);

            return ProcessCommandResult.Success;
        }
    }
}
