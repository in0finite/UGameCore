using System.Collections.Generic;
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

            if (context.NumArguments <= 1)
            {
                var commands = this.commandManager.RegisteredCommandsDict.ToArray();
                commands.SortBy(_ => _.Key);

                response = "List of available commands (use `help cmd_name` for details): \n" +
                              string.Join(", ", commands.Select(_ => _.Key));

                response += "\n\n" + string.Join("\n", commands.Select(_ => _.Key + (_.Value.description != null ? "  -  " + _.Value.description : string.Empty)));

                return ProcessCommandResult.SuccessResponse(response);
            }

            string cmd = context.ReadString();

            if (!this.commandManager.RegisteredCommandsDict.TryGetValue(cmd, out CommandInfo commandInfo))
                return ProcessCommandResult.UnknownCommand(cmd);

            response = $"{cmd}";
            if (commandInfo.description != null)
                response += "  -  " + commandInfo.description;
            response += "\n";
            response += $"requires server perms: {!commandInfo.allowToRunWithoutServerPermissions}   ";
            response += $"only on server: {commandInfo.runOnlyOnServer}   ";
            response += $"limit interval: {commandInfo.limitInterval}   ";
            response += $"auto-complete: {commandInfo.autoCompletionHandler != null}   ";

            return ProcessCommandResult.SuccessResponse(response);
        }

        [CommandAutoCompletionMethod("help")]
        ProcessCommandResult HelpCmdAutoComplete(ProcessCommandContext context)
        {
            if (context.NumArguments <= 1)
                return ProcessCommandResult.AutoCompletion(null, null);

            var possibleCompletions = new List<string>();

            //return this.commandManager.AutoCompleteCommand(, out string outExactCompletion, possibleCompletions);

            string cmd = context.ReadString();

            CommandManager.DoAutoCompletion(
                cmd, this.commandManager.RegisteredCommands, out string outExactCompletion, possibleCompletions);

            if (outExactCompletion != null)
                outExactCompletion = "help " + outExactCompletion;

            return ProcessCommandResult.AutoCompletion(outExactCompletion, possibleCompletions);
        }

        [CommandMethod("command_remove", "Removes a command")]
        ProcessCommandResult RemoveCmd(ProcessCommandContext context)
        {
            string cmd = context.ReadString();
            if (!this.commandManager.HasCommand(cmd))
                return ProcessCommandResult.UnknownCommand(cmd);
            this.commandManager.RemoveCommand(cmd);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("command_forbid", "Forbids a command. Forbidden commands can not be registered or executed.")]
        ProcessCommandResult ForbidCmd(ProcessCommandContext context)
        {
            string cmd = context.ReadString();
            if (!this.commandManager.ForbiddenCommands.Contains(cmd))
                return ProcessCommandResult.Error("Command already forbidden");
            this.commandManager.ForbiddenCommands.Add(cmd);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("alias", "Creates alias for a command")]
        ProcessCommandResult AliasCmd(ProcessCommandContext context)
        {
            string newCmd = context.ReadString();
            string existingCmd = context.ReadString();

            if (!this.commandManager.HasCommand(existingCmd))
                return ProcessCommandResult.UnknownCommand(existingCmd);

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
