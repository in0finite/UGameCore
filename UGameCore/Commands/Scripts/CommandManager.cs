﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore
{
    public class CommandManager : MonoBehaviour
    {
        public static CommandManager Singleton { get; private set; }

        readonly Dictionary<string, CommandInfo> m_registeredCommands =
            new Dictionary<string, CommandInfo>(System.StringComparer.InvariantCulture);

        public IReadOnlyCollection<string> RegisteredCommands => m_registeredCommands.Keys;

        public static string invalidSyntaxText => "Invalid syntax";

        [Tooltip("Forbidden commands can not be registered or executed")]
        public List<string> forbiddenCommands = new List<string>();

        [SerializeField] private bool m_registerHelpCommand = true;

        /// <summary>
        /// Annotate a method with this attribute to register it as a command.
        /// </summary>
        [System.AttributeUsageAttribute(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
        public class CommandMethodAttribute : System.Attribute
        {
            public string command;
            public string description;
            public bool allowToRunWithoutServerPermissions;
            public bool runOnlyOnServer;
            public float limitInterval;

            public CommandMethodAttribute(string command)
            {
                this.command = command;
            }
        }

        public struct CommandInfo
        {
            public string command;
            public string description;
            public System.Func<ProcessCommandContext, ProcessCommandResult> commandHandler;
            public bool allowToRunWithoutServerPermissions;
            public bool runOnlyOnServer;
            public float limitInterval;

            public CommandInfo(string command, bool allowToRunWithoutServerPermissions)
                : this()
            {
                this.command = command;
                this.allowToRunWithoutServerPermissions = allowToRunWithoutServerPermissions;
            }

            public CommandInfo(string command, string description, bool allowToRunWithoutServerPermissions)
                : this()
            {
                this.command = command;
                this.description = description;
                this.allowToRunWithoutServerPermissions = allowToRunWithoutServerPermissions;
            }

            public CommandInfo(string command, string description, bool allowToRunWithoutServerPermissions, bool runOnlyOnServer, float limitInterval)
                : this()
            {
                this.command = command;
                this.description = description;
                this.allowToRunWithoutServerPermissions = allowToRunWithoutServerPermissions;
                this.runOnlyOnServer = runOnlyOnServer;
                this.limitInterval = limitInterval;
            }
        }

        public class ProcessCommandResult
        {
            public int exitCode;
            public string response;

            public bool IsSuccess => this.exitCode == 0;

            public static ProcessCommandResult UnknownCommand => Error("Unknown command");
            public static ProcessCommandResult InvalidCommand => Error("Invalid command");
            public static ProcessCommandResult ForbiddenCommand => Error("Forbidden command");
            public static ProcessCommandResult NoPermissions => Error("You don't have permissions to run this command");
            public static ProcessCommandResult CanOnlyRunOnServer => Error("This command can only run on server");
            public static ProcessCommandResult LimitInterval(float interval) => Error($"This command can only be used on an interval of {interval} seconds");
            public static ProcessCommandResult Error(string errorMessage) => new ProcessCommandResult { exitCode = 1, response = errorMessage };
            public static ProcessCommandResult Success => SuccessResponse(null);
            public static ProcessCommandResult SuccessResponse(string response) => new ProcessCommandResult() { exitCode = 0, response = response };
        }

        public class ProcessCommandContext
        {
            /// <summary>
            /// Command that should be processed. This variable contains the entire command, including arguments.
            /// </summary>
            public string command;

            /// <summary>
            /// Does the executor have server permissions ?
            /// </summary>
            public bool hasServerPermissions;
            
            /// <summary>
            /// The one who is executing the command.
            /// </summary>
            public object executor;

            /// <summary>
            /// Last time when executor executed a command. If specified, the value will be used for rate-limiting.
            /// </summary>
            public double? lastTimeExecutedCommand;
        }



        void Awake()
        {
            if (null == Singleton)
                Singleton = this;

            if (m_registerHelpCommand)
                RegisterCommand(new CommandInfo { command = "help", commandHandler = ProcessHelpCommand, allowToRunWithoutServerPermissions = true });
        }

        public void RegisterCommand(CommandInfo commandInfo)
        {
            if (null == commandInfo.commandHandler)
                throw new System.ArgumentException("Command handler must be provided");

            if (string.IsNullOrWhiteSpace(commandInfo.command))
                throw new System.ArgumentException("Command can not be empty");

            commandInfo.command = commandInfo.command.Trim();

            if (this.forbiddenCommands.Contains(commandInfo.command))
                throw new System.InvalidOperationException("Command is forbidden");

            if (m_registeredCommands.ContainsKey(commandInfo.command))
                throw new System.ArgumentException("Command was already registered");

            m_registeredCommands.Add(commandInfo.command, commandInfo);
        }

        public void RegisterCommand(string command, System.Func<ProcessCommandContext, ProcessCommandResult> function)
        {
            var commandInfo = new CommandInfo(command, true) { commandHandler = function };
            this.RegisterCommand(commandInfo);
        }

        public void RegisterCommandsFromTypeMethods(object instanceObject)
        {
            this.RegisterCommandsFromTypeMethods(instanceObject, instanceObject.GetType());
        }

        void RegisterCommandsFromTypeMethods(object instanceObject, System.Type type)
        {
            var methods = type.GetMethods(
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.Instance);

            foreach (var method in methods)
            {
                if (!method.IsDefined(typeof(CommandMethodAttribute), true))
                    continue;

                if (!F.RunExceptionSafe(() => CheckIfCommandMethodIsCorrect(type, method)))
                    continue;

                var attrs = method.GetCustomAttributes<CommandMethodAttribute>(true);
                foreach (CommandMethodAttribute attr in attrs)
                {
                    var commandInfo = new CommandInfo
                    {
                        command = attr.command,
                        description = attr.description,
                        allowToRunWithoutServerPermissions = attr.allowToRunWithoutServerPermissions,
                        runOnlyOnServer = attr.runOnlyOnServer,
                        limitInterval = attr.limitInterval,
                        commandHandler = (ProcessCommandContext context) => (ProcessCommandResult)method.Invoke(method.IsStatic ? null : instanceObject, new object[] { context }),
                    };

                    F.RunExceptionSafe(() => this.RegisterCommand(commandInfo));
                }
            }
        }

        void CheckIfCommandMethodIsCorrect(System.Type type, MethodInfo method)
        {
            if (method.ReturnType != typeof(ProcessCommandResult))
                throw new System.ArgumentException($"Return type must be {nameof(ProcessCommandResult)}, method: {type.Name}.{method.Name}()");

            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                throw new System.ArgumentException($"Method must have exactly 1 parameter ({nameof(ProcessCommandContext)}), method: {type.Name}.{method.Name}()");

            if (parameters[0].ParameterType != typeof(ProcessCommandContext))
                throw new System.ArgumentException($"Type of parameter must be ({nameof(ProcessCommandContext)}), method: {type.Name}.{method.Name}()");
        }

        public bool RemoveCommand(string command)
        {
            return m_registeredCommands.Remove(command);
        }

        public static string[] SplitCommandIntoArguments(string command)
        {
            // TODO: add support for arguments that have spaces, i.e. those enclosed with quotes

            return command.Split(new string[] {" ", "\t"}, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public static string GetRestOfTheCommand(string command, int argumentIndex)
        {
            if (argumentIndex < 0)
                return "";

            string[] args = SplitCommandIntoArguments(command);

            if (argumentIndex > args.Length - 2)
                return "";

            return string.Join(" ", args, argumentIndex + 1, args.Length - argumentIndex - 1);
        }

        public static Vector3 ParseVector3(string[] arguments, int startIndex)
        {
            if (startIndex + 2 >= arguments.Length)
                throw new System.ArgumentException("Failed to parse Vector3: not enough arguments");

            Vector3 v = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                if (!float.TryParse(arguments[startIndex + i], out float f))
                    throw new System.ArgumentException("Failed to parse Vector3: invalid number");
                v[i] = f;
            }

            return v;
        }

        public static Quaternion ParseQuaternion(string[] arguments, int startIndex)
        {
            if (startIndex + 3 >= arguments.Length)
                throw new System.ArgumentException("Failed to parse Quaternion: not enough arguments");

            Quaternion quaternion = Quaternion.identity;
            for (int i = 0; i < 4; i++)
            {
                if (!float.TryParse(arguments[startIndex + i], out float f))
                    throw new System.ArgumentException("Failed to parse Quaternion: invalid number");
                quaternion[i] = f;
            }

            return quaternion;
        }

        public static Color ParseColor(string[] arguments, int startIndex)
        {
            if (startIndex >= arguments.Length)
                throw new System.ArgumentException("Failed to parse color: not enough arguments");

            if (!ColorUtility.TryParseHtmlString(arguments[startIndex], out Color color))
                throw new System.ArgumentException("Failed to parse color");

            return color;
        }

        public ProcessCommandResult ProcessCommand(ProcessCommandContext context)
        {
            if (string.IsNullOrWhiteSpace(context.command))
                return ProcessCommandResult.UnknownCommand;

            string[] arguments = SplitCommandIntoArguments(context.command);
            if (0 == arguments.Length)
                return ProcessCommandResult.InvalidCommand;

            if (!m_registeredCommands.TryGetValue(arguments[0], out CommandInfo commandInfo))
                return ProcessCommandResult.UnknownCommand;

            if (this.forbiddenCommands.Contains(commandInfo.command))
                return ProcessCommandResult.ForbiddenCommand;

            if (commandInfo.runOnlyOnServer && !NetworkStatus.IsServer)
                return ProcessCommandResult.CanOnlyRunOnServer;

            if (!context.hasServerPermissions && !commandInfo.allowToRunWithoutServerPermissions)
                return ProcessCommandResult.NoPermissions;

            if (context.lastTimeExecutedCommand.HasValue)
            {
                if (commandInfo.limitInterval > 0 && Time.timeAsDouble - context.lastTimeExecutedCommand.Value < commandInfo.limitInterval)
                    return ProcessCommandResult.LimitInterval(commandInfo.limitInterval);
            }

            return commandInfo.commandHandler(context);
        }

        public ProcessCommandResult ProcessCommandAsServer(string command)
        {
            return ProcessCommand(new ProcessCommandContext {command = command, hasServerPermissions = true});
        }

        ProcessCommandResult ProcessHelpCommand(ProcessCommandContext context)
        {
            string response = "List of available commands: " +
                              string.Join(", ", m_registeredCommands
                                  .Where(pair => context.hasServerPermissions || pair.Value.allowToRunWithoutServerPermissions)
                                  .Select(pair => pair.Key));

            return new ProcessCommandResult {response = response};
        }

        public void AutoCompleteCommand(ProcessCommandContext context, out string outExactCompletion, List<string> outPossibleCompletions)
        {
            outExactCompletion = null;

            if (string.IsNullOrWhiteSpace(context.command))
                return;

            string[] arguments = SplitCommandIntoArguments(context.command);
            if (0 == arguments.Length)
                return;

            if (arguments.Length > 1)
            {
                // TODO: ask the command handler to do auto-completion

                return;
            }

            // only 1 argument, the command itself, auto-complete it


            // example (commands: net_start, net_stop, net_exit, net_socket, neptun):
            // 
            // input: n
            // output: ne (common prefix for ALL commands that start with it)
            //
            // input: ne
            // output: ne (no expansion)
            //
            // input: net
            // output: net_ (common prefix for ALL commands that start with it)
            //
            // input: net_st
            // output: net_start, net_stop (common prefix is equal to input)
            //
            // input: net_s
            // output: net_start, net_stop, net_socket (common prefix is equal to input)

            var commandsStartingWith = new List<string>();

            foreach (var pair in m_registeredCommands)
            {
                if (pair.Key.StartsWith(arguments[0], System.StringComparison.Ordinal))
                    commandsStartingWith.Add(pair.Key);
            }

            if (commandsStartingWith.Count == 0)
                return;

            // find common prefix

            string commandToTest = commandsStartingWith[0];

            string commonPrefix = arguments[0];
            int startIndex = commonPrefix.Length;

            for (int i = startIndex; i < commandToTest.Length; i++)
            {
                char ch = commandToTest[i];

                // if all other commands have this char at this index, it is part of common prefix

                bool allCommandsHaveThisChar = true;

                for (int j = 1; j < commandsStartingWith.Count; j++)
                {
                    string cmd = commandsStartingWith[j];
                    if (i >= cmd.Length || cmd[i] != ch)
                    {
                        allCommandsHaveThisChar = false;
                        break;
                    }
                }

                if (!allCommandsHaveThisChar)
                    break;

                commonPrefix += ch;
            }

            if (commonPrefix.Equals(arguments[0], System.StringComparison.Ordinal))
            {
                // longest common prefix is equal to input
                // no need to auto-complete, only return all possible completions

                if (commandsStartingWith.Count > 1) // don't return 1 command only (which would be equal to input)
                    outPossibleCompletions.AddRange(commandsStartingWith);
                return;
            }

            // auto-complete the command into common prefix
            outExactCompletion = commonPrefix;
        }

        public bool HasCommand(string command)
        {
            return m_registeredCommands.ContainsKey(command);
        }
    }
}
