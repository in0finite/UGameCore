using System.Linq;
using System.Text;
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

        [CommandMethod("sys_info", "Displays various system information")]
        ProcessCommandResult SystemInfoCmd(ProcessCommandContext context)
        {
            var props = typeof(SystemInfo).GetProperties(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            string response = string.Join("\n", props.Select(p => p.Name + ": " + p.GetValue(null)));

            return ProcessCommandResult.SuccessResponse(response);
        }

        [CommandMethod("version", "Displays version of application")]
        ProcessCommandResult VersionCmd(ProcessCommandContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"App version: {Application.version}");
            sb.AppendLine($"Unity version: {Application.unityVersion}");
            sb.AppendLine($"Company name: {Application.companyName}");
            sb.AppendLine($"Product name: {Application.productName}");
            
            return ProcessCommandResult.SuccessResponse(sb.ToString());
        }
    }
}
