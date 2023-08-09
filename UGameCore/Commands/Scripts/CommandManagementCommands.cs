using System.Linq;
using UGameCore.Utilities;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore
{
    public class CommandManagementCommands : MonoBehaviour
    {
        public CommandManager commandManager;


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        [CommandMethod("help", allowToRunWithoutServerPermissions = true)]
        ProcessCommandResult ForbidCmd(ProcessCommandContext context)
        {
            string response = "List of available commands: " +
                              string.Join(", ", this.commandManager.RegisteredCommandsDict
                                  .Select(pair => pair.Key));

            return ProcessCommandResult.SuccessResponse(response);
        }

        [CommandMethod("command_remove")]
        ProcessCommandResult RemoveCmd(ProcessCommandContext context)
        {
            string cmd = context.ReadString();
            if (!this.commandManager.HasCommand(cmd))
                return ProcessCommandResult.Error("Command not found");
            this.commandManager.RemoveCommand(cmd);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("command_forbid")]
        ProcessCommandResult ForbidCmd(ProcessCommandContext context)
        {
            string cmd = context.ReadString();
            if (!this.commandManager.forbiddenCommands.Contains(cmd))
                return ProcessCommandResult.Error("Command already forbidden");
            this.commandManager.forbiddenCommands.Add(cmd);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("alias")]
        ProcessCommandResult AliasCmd(ProcessCommandContext context)
        {
            string newCmd = context.ReadString();
            string existingCmd = context.ReadString();

            if (!this.commandManager.HasCommand(existingCmd))
                return ProcessCommandResult.Error($"Command '{existingCmd}' not found");

            this.commandManager.RegisterCommand(newCmd, context =>
            {
                context.arguments[0] = existingCmd;
                context.command = this.commandManager.CombineArguments(context.arguments);
                return this.commandManager.ProcessCommand(context);
            });

            return ProcessCommandResult.Success;
        }
    }
}
