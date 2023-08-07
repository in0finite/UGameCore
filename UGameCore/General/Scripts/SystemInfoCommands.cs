using System.Linq;
using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore
{
    public class SystemInfoCommands : MonoBehaviour
    {
        public CommandManager commandManager;


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        [CommandMethod("sys_info")]
        ProcessCommandResult SystemInfoCmd(ProcessCommandContext context)
        {
            var props = typeof(SystemInfo).GetProperties(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            string response = string.Join("\n", props.Select(p => p.Name + ": " + p.GetValue(null)));

            return ProcessCommandResult.SuccessResponse(response);
        }
    }
}
