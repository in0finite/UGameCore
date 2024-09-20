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
        ProcessCommandResult HelpCmd(ProcessCommandContext context)
        {
            string response;

            if (context.NumArguments <= 1) // display help for all commands
            {
                var commands = this.commandManager.RegisteredCommandsDict.ToArray();
                commands.SortBy(_ => _.Key);

                response = "List of available commands (use `help cmd_name` for details): \n" +
                              string.Join(", ", commands.Select(_ => _.Key));

                response += "\n\n" + string.Join("\n", commands.Select(_ => _.Key + (_.Value.description != null ? "  -  " + _.Value.description : string.Empty)));

                return ProcessCommandResult.SuccessResponse(response);
            }

            // display help for specified command

            string cmd = context.ReadString();

            if (!this.commandManager.RegisteredCommandsDict.TryGetValue(cmd, out CommandInfo commandInfo))
                return ProcessCommandResult.UnknownCommand(cmd);

            response = $"{cmd}";
            if (commandInfo.description != null)
                response += "  -  " + commandInfo.description;
            response += "\n";
            if (commandInfo.syntax != null)
                response += "syntax:  " + commandInfo.syntax + "   ";
            response += $"requires server perms: {!commandInfo.allowToRunWithoutServerPermissions}   ";
            response += $"only on server: {commandInfo.runOnlyOnServer}   ";
            response += $"limit interval: {commandInfo.limitInterval}   ";
            response += $"auto-complete: {commandInfo.autoCompletionHandler != null}   ";

            return ProcessCommandResult.SuccessResponse(response);
        }

        [CommandAutoCompletionMethod("help")]
        ProcessCommandResult HelpCmdAutoComplete(ProcessCommandContext context)
        {
            // don't auto-complete with all commands
            if (context.NumArguments <= 1)
                return ProcessCommandResult.AutoCompletion(null, null);

            return this.commandManager.ProcessCommandAutoCompletion(context, this.commandManager.RegisteredCommands);
        }

        [CommandMethod("command_remove", "Removes a command", syntax = "(string commandName)")]
        ProcessCommandResult RemoveCmd(ProcessCommandContext context)
        {
            string cmd = context.ReadString();
            if (!this.commandManager.HasCommand(cmd))
                return ProcessCommandResult.UnknownCommand(cmd);
            this.commandManager.RemoveCommand(cmd);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("command_forbid", "Forbids a command. Forbidden commands can not be registered or executed.", syntax = "(string commandName)")]
        ProcessCommandResult ForbidCmd(ProcessCommandContext context)
        {
            string cmd = context.ReadString();
            if (!this.commandManager.ForbiddenCommands.Contains(cmd))
                return ProcessCommandResult.Error("Command already forbidden");
            this.commandManager.ForbiddenCommands.Add(cmd);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("alias", "Creates alias for a command", syntax = "(string newCommand, string existingCommand)", exactNumArguments = 2)]
        ProcessCommandResult AliasCmd(ProcessCommandContext context)
        {
            string newCmd = context.ReadString();
            string existingCmd = context.ReadString();

            this.commandManager.RegisterCommandAlias(existingCmd, newCmd);

            return ProcessCommandResult.Success;
        }

        [CommandMethod("args_print", "Prints arguments 1 line each")]
        ProcessCommandResult ArgsPrintCmd(ProcessCommandContext context)
        {
            string response = string.Empty;
            while (context.HasNextArgument())
                response += context.ReadString() + "\n";
            return ProcessCommandResult.SuccessResponse(response);
        }
    }
}
