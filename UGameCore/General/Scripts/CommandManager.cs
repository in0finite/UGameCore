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

        // note: we have to use InvariantCultureIgnoreCase instead of OrdinalIgnoreCase comparer,
        // because on a lot of places we use ToLowerInvariant(), so we need to be consistent

        readonly Dictionary<string, CommandInfo> m_registeredCommands =
            new Dictionary<string, CommandInfo>(System.StringComparer.InvariantCultureIgnoreCase);

        public IReadOnlyCollection<string> RegisteredCommands => m_registeredCommands.Keys;
        public IReadOnlyDictionary<string, CommandInfo> RegisteredCommandsDict => m_registeredCommands;

        public int maxNumCharactersInCommand = 300;

        public static string invalidSyntaxText => "Invalid syntax";

        [Tooltip("Forbidden commands can not be registered or executed")]
        [SerializeField]
        List<string> m_forbiddenCommandsList = new List<string>();

        /// <summary>
        /// Forbidden commands can not be registered or executed.
        /// </summary>
        public readonly HashSet<string> ForbiddenCommands = new HashSet<string>(System.StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Annotate a method with this attribute to register it as a command.
        /// </summary>
        [System.AttributeUsageAttribute(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
        public class CommandMethodAttribute : System.Attribute
        {
            public string command;
            public string description;
            public string syntax;
            public sbyte maxNumArguments = -1;
            public sbyte minNumArguments = -1;
            public sbyte exactNumArguments = -1;
            public bool allowToRunWithoutServerPermissions;
            public bool runOnlyOnServer;
            public float limitInterval;

            public CommandMethodAttribute(string command)
            {
                this.command = command;
            }

            public CommandMethodAttribute(string command, string description)
            {
                this.command = command;
                this.description = description;
            }
        }

        /// <summary>
        /// Annotate a method with this attribute to provide auto-completion for a command.
        /// </summary>
        [System.AttributeUsageAttribute(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
        public class CommandAutoCompletionMethodAttribute : System.Attribute
        {
            public string command;

            public CommandAutoCompletionMethodAttribute(string command)
            {
                this.command = command;
            }
        }

        public struct CommandInfo
        {
            public string command;
            public string description;
            public string syntax;
            public sbyte? maxNumArguments;
            public sbyte? minNumArguments;
            public sbyte? exactNumArguments;
            public System.Func<ProcessCommandContext, ProcessCommandResult> commandHandler;
            public bool allowToRunWithoutServerPermissions;
            public bool runOnlyOnServer;
            public float limitInterval;
            public System.Func<ProcessCommandContext, ProcessCommandResult> autoCompletionHandler;

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

        /// <summary>
        /// Result of a command processing.
        /// </summary>
        public class ProcessCommandResult
        {
            public int exitCode;
            public string response;
            public List<string> autoCompletions;

            public bool IsSuccess => this.exitCode == 0;

            public static ProcessCommandResult UnknownCommand(string cmd) => Error($"Unknown command: {cmd}");
            public static ProcessCommandResult InvalidCommand => Error("Invalid command");
            public static ProcessCommandResult ForbiddenCommand => Error("Forbidden command");
            public static ProcessCommandResult NoPermissions => Error("You don't have permissions to run this command");
            public static ProcessCommandResult CanOnlyRunOnServer => Error("This command can only run on server");
            public static ProcessCommandResult LimitInterval(float interval) => Error($"This command can only be used on an interval of {interval} seconds");
            public static ProcessCommandResult Error(string errorMessage)
                => new ProcessCommandResult { exitCode = 1, response = errorMessage };
            public static ProcessCommandResult Success => SuccessResponse(null);
            public static ProcessCommandResult SuccessResponse(string response)
                => new ProcessCommandResult() { exitCode = 0, response = response };
            public static ProcessCommandResult AutoCompletion(string exactMatch, IEnumerable<string> autoCompletions)
                => new ProcessCommandResult() { exitCode = 0, response = exactMatch, autoCompletions = autoCompletions != null ? new List<string>(autoCompletions) : null };
        }

        /// <summary>
        /// Context in which the command is processed.
        /// </summary>
        public class ProcessCommandContext
        {
            /// <summary>
            /// Command that should be processed. This variable contains the entire command, including arguments.
            /// </summary>
            public string command;

            /// <summary>
            /// Command that should be processed, without arguments.
            /// </summary>
            public string commandOnly { get; internal set; }

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

            /// <summary>
            /// All arguments, including command itself.
            /// </summary>
            public string[] arguments { get; internal set; }

            /// <summary>
            /// Number of arguments, including command itself.
            /// </summary>
            public int NumArguments => this.arguments.Length;

            public int currentArgumentIndex = 1;

            public bool HasNextArgument()
            {
                return this.currentArgumentIndex < this.NumArguments;
            }

            /// <summary>
            /// Read next command argument as string.
            /// </summary>
            public string ReadString()
            {
                if (this.currentArgumentIndex >= this.NumArguments)
                    throw new System.ArgumentException($"Trying to read command argument out of bounds (index {this.currentArgumentIndex}, num arguments {this.NumArguments})");

                string arg = this.arguments[this.currentArgumentIndex];
                this.currentArgumentIndex++;
                return arg;
            }

            /// <summary>
            /// Read next command argument as string, or if it's not available, return specified default value.
            /// </summary>
            public string ReadStringOrDefault(string defaultValue)
            {
                if (this.currentArgumentIndex >= this.NumArguments)
                    return defaultValue;

                return this.ReadString();
            }

            /// <summary>
            /// Read next command argument as int.
            /// </summary>
            public int ReadInt()
            {
                string str = this.ReadString();
                return int.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Read next command argument as float.
            /// </summary>
            public float ReadFloat()
            {
                string str = this.ReadString();
                return float.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Read next command argument as Vector3.
            /// </summary>
            public Vector3 ReadVector3()
            {
                return new Vector3(this.ReadFloat(), this.ReadFloat(), this.ReadFloat());
            }

            /// <summary>
            /// Read next command argument as Enum.
            /// </summary>
            public T ReadEnum<T>()
                where T : struct, System.Enum
            {
                string str = this.ReadString();
                return System.Enum.Parse<T>(str, true);
            }
        }



        void Awake()
        {
            if (null == Singleton)
                Singleton = this;

            this.ForbiddenCommands.UnionWith(m_forbiddenCommandsList);
        }

        public bool HasCommand(string command)
        {
            return m_registeredCommands.ContainsKey(command);
        }

        public bool RemoveCommand(string command)
        {
            return m_registeredCommands.Remove(command);
        }

        void CheckIfHasInvalidChars(string cmd)
        {
            for (int i = 0; i < cmd.Length; i++)
            {
                char c = cmd[i];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                    throw new System.ArgumentException($"Command characters can only be letters or digits, found {c}");
            }
        }

        public void RegisterCommand(CommandInfo commandInfo)
        {
            if (null == commandInfo.commandHandler)
                throw new System.ArgumentException("Command handler must be provided");

            if (string.IsNullOrWhiteSpace(commandInfo.command))
                throw new System.ArgumentException("Command can not be empty");

            commandInfo.command = commandInfo.command.ToLowerInvariant().Trim();

            this.CheckIfHasInvalidChars(commandInfo.command);

            if (this.ForbiddenCommands.Contains(commandInfo.command))
                throw new System.InvalidOperationException($"Command '{commandInfo.command}' is forbidden");

            if (m_registeredCommands.ContainsKey(commandInfo.command))
                throw new System.ArgumentException($"Command '{commandInfo.command}' was already registered");

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

            var autoCompleteMethods = new List<(MethodInfo, CommandAutoCompletionMethodAttribute)>();

            foreach (var method in methods)
            {
                bool isCommandExecutor = method.IsDefined(typeof(CommandMethodAttribute), true);
                if (!isCommandExecutor)
                    continue;

                if (!F.RunExceptionSafe(() => CheckIfCommandMethodIsCorrect(method)))
                    continue;

                var attrs = method.GetCustomAttributes<CommandMethodAttribute>(true);
                foreach (CommandMethodAttribute attr in attrs)
                {
                    var commandInfo = new CommandInfo
                    {
                        command = attr.command,
                        description = attr.description,
                        syntax = attr.syntax,
                        minNumArguments = attr.minNumArguments >= 0 ? attr.minNumArguments : null,
                        maxNumArguments = attr.maxNumArguments >= 0 ? attr.maxNumArguments : null,
                        exactNumArguments = attr.exactNumArguments >= 0 ? attr.exactNumArguments : null,
                        allowToRunWithoutServerPermissions = attr.allowToRunWithoutServerPermissions,
                        runOnlyOnServer = attr.runOnlyOnServer,
                        limitInterval = attr.limitInterval,
                        commandHandler = CreateCommandHandler(method, instanceObject),
                    };

                    F.RunExceptionSafe(() => this.RegisterCommand(commandInfo));
                }
            }

            // 2nd pass to register auto-completion methods

            foreach (MethodInfo method in methods)
            {
                bool isCommandAutoCompletion = method.IsDefined(typeof(CommandAutoCompletionMethodAttribute), true);
                if (!isCommandAutoCompletion)
                    continue;

                var autoCompleteAttrs = method.GetCustomAttributes<CommandAutoCompletionMethodAttribute>(true);
                foreach (CommandAutoCompletionMethodAttribute attr in autoCompleteAttrs)
                    F.RunExceptionSafe(() => this.RegisterAutoCompletionMethod(method, attr.command, instanceObject));
            }
        }

        void RegisterAutoCompletionMethod(MethodInfo method, string cmd, object instanceObject)
        {
            CheckIfCommandMethodIsCorrect(method);

            if (!m_registeredCommands.TryGetValue(cmd, out CommandInfo commandInfo))
                throw new System.ArgumentException($"Failed to register auto-complete handler for command '{cmd}': command does not exist");
            
            if (commandInfo.autoCompletionHandler != null)
                throw new System.ArgumentException($"Auto-complete handler for command '{cmd}' already exists");
            
            commandInfo.autoCompletionHandler = CreateCommandHandler(method, instanceObject);

            m_registeredCommands[cmd] = commandInfo;
        }

        System.Func<ProcessCommandContext, ProcessCommandResult> CreateCommandHandler(
            MethodInfo method, object instanceObject)
        {
            return (ProcessCommandContext context) => 
                (ProcessCommandResult)method.Invoke(method.IsStatic ? null : instanceObject, new object[] { context });
        }

        void CheckIfCommandMethodIsCorrect(MethodInfo method)
        {
            System.Type type = method.DeclaringType;

            if (method.ReturnType != typeof(ProcessCommandResult))
                throw new System.ArgumentException($"Return type must be {nameof(ProcessCommandResult)}, method: {type.Name}.{method.Name}()");

            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                throw new System.ArgumentException($"Method must have exactly 1 parameter ({nameof(ProcessCommandContext)}), method: {type.Name}.{method.Name}()");

            if (parameters[0].ParameterType != typeof(ProcessCommandContext))
                throw new System.ArgumentException($"Type of parameter must be ({nameof(ProcessCommandContext)}), method: {type.Name}.{method.Name}()");
        }

        public static string[] SplitCommandIntoArguments(string command)
        {
            // examples:
            // abcd"abc"abc
            // abcd" abcd" abcd
            // abcd "abcd"abcd
            // abcd "a  |
            // abcd ""
            // abcd " "

            var arguments = new List<string>();

            // trim is fine here, because arguments can not have whitespaces at start/end without using quotes
            command = command.Trim();
            
            int argumentStartIndex = -1;
            char startingQuoteChar = (char)0;
            bool lastCharWasEscape = false;

            for (int i = 0; i < command.Length; i++)
            {
                char ch = command[i];

                bool thisCharIsEscaped = lastCharWasEscape;
                lastCharWasEscape = false;

                if (!thisCharIsEscaped && ch == '\\')
                {
                    lastCharWasEscape = true;
                    continue;
                }

                if (char.IsWhiteSpace(ch))
                {
                    if (startingQuoteChar == 0) // not inside quotes
                    {
                        // cut argument here
                        string argument = command.Substring(argumentStartIndex + 1, i - argumentStartIndex - 1);
                        argument = argument.Trim(); // not sure if Trim() is needed here, but keep it just in case
                        if (argument.Length > 0) // only add it if not empty, because a whitespace outside of quotes should not be an argument
                            arguments.Add(argument);
                        argumentStartIndex = i;
                        continue;
                    }

                    // inside quotes
                    // skip this character, he will be part of current argument

                    continue;
                }

                if ((ch == '\'' || ch == '\"') && !thisCharIsEscaped)
                {
                    if (ch == startingQuoteChar) // inside quotes
                    {
                        if (i == command.Length - 1 || char.IsWhiteSpace(command[i + 1]))
                        {
                            // whitespace is after this char, end current argument

                            string argument = command.Substring(argumentStartIndex + 1, i - argumentStartIndex - 1);
                            arguments.Add(argument); // do not Trim() here, arguments are allowed to have whitespaces at start/end
                            argumentStartIndex = i;
                            startingQuoteChar = (char)0;

                            continue;
                        }

                        // no whitespace after this char, treat it as regular char - he will be part of current argument

                        continue;
                    }

                    if (startingQuoteChar == 0) // not inside quotes
                    {
                        if (i == 0 || char.IsWhiteSpace(command[i - 1]))
                        {
                            // whitespace is before this char, open new argument
                            startingQuoteChar = ch;
                            argumentStartIndex = i;

                            continue;
                        }

                        // no whitespace before this char, treat it as regular char - he will be part of current argument

                        continue;
                    }

                    // different quote character, skip this character, he will be part of current argument

                    continue;
                }

                // normal character, skip it, he will be part of current argument

                continue;
            }

            // add the remaining argument
            string remainingArgument = command.Substring(argumentStartIndex + 1, command.Length - argumentStartIndex - 1);
            if (remainingArgument.Length > 0)
            {
                // here we only need to trim from end, because argument may have quotes
                // but because the command was trimmed at beginning, we don't need to do it
                arguments.Add(remainingArgument);
            }

            arguments.ReplaceEach(arg => UnescapeArgument(arg));

            // do not remove empty strings, they are valid arguments
            //arguments.RemoveAll(string.IsNullOrWhiteSpace);

            return arguments.ToArray();
        }

        static string UnescapeArgument(string argument)
        {
            var list = new List<char>(argument);

            bool lastCharWasUnescapeChar = false;

            for (int i = 0; i < list.Count; i++)
            {
                char ch = list[i];

                bool thisCharIsUnescaped = lastCharWasUnescapeChar;
                lastCharWasUnescapeChar = false;

                if (!thisCharIsUnescaped && ch == '\\')
                {
                    lastCharWasUnescapeChar = true;
                    list[i] = (char)0; // do not include this char in final string
                    continue;
                }

                if (thisCharIsUnescaped)
                {
                    if (ch == 't')
                        list[i] = '\t';
                    else if (ch == 'n')
                        list[i] = '\n';
                    else if (ch == 'r')
                        list[i] = '\r';
                    else if (ch == '0')
                        list[i] = '\0';
                }
            }

            list.RemoveAll(ch => ch == 0); // remove all unescape chars

            return new string(list.ToArray());
        }

        public string CombineArguments(params string[] arguments)
        {
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < arguments.Length; i++)
            {
                string arg = arguments[i]; // don't trim, arguments are allowed to have whitespaces at start/end

                bool isEmpty = arg.Length == 0;
                bool hasWhitespace = arg.Any(char.IsWhiteSpace);
                bool needsQuotes = isEmpty || hasWhitespace;

                if (needsQuotes)
                    sb.Append('\"');

                sb.Append(needsQuotes ? arg.Replace("\"", "\\\"") : arg);

                if (needsQuotes)
                    sb.Append('\"');

                if (i != arguments.Length - 1)
                    sb.Append(' ');
            }

            return sb.ToString();
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
                return ProcessCommandResult.UnknownCommand(null);

            if (context.command.Length > this.maxNumCharactersInCommand)
                return ProcessCommandResult.Error("Command too long");

            string[] arguments = SplitCommandIntoArguments(context.command);
            if (0 == arguments.Length)
                return ProcessCommandResult.InvalidCommand;

            if (!m_registeredCommands.TryGetValue(arguments[0], out CommandInfo commandInfo))
                return ProcessCommandResult.UnknownCommand(arguments[0]);

            if (this.ForbiddenCommands.Contains(commandInfo.command))
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

            if (commandInfo.exactNumArguments.HasValue && arguments.Length - 1 != commandInfo.exactNumArguments.Value)
                return ProcessCommandResult.Error($"Command requires exactly {commandInfo.exactNumArguments.Value} arguments");

            if (commandInfo.minNumArguments.HasValue && arguments.Length - 1 < commandInfo.minNumArguments.Value)
                return ProcessCommandResult.Error($"Command requires at least {commandInfo.minNumArguments.Value} arguments");

            if (commandInfo.maxNumArguments.HasValue && arguments.Length - 1 > commandInfo.maxNumArguments.Value)
            {
                if (commandInfo.maxNumArguments.Value == 0)
                    return ProcessCommandResult.Error("Command does not accept any arguments");
                return ProcessCommandResult.Error($"Command can not have more than {commandInfo.maxNumArguments.Value} arguments");
            }

            context.commandOnly = commandInfo.command;
            context.arguments = arguments;

            return commandInfo.commandHandler(context);
        }

        public ProcessCommandResult ProcessCommandAsServer(string command)
        {
            return ProcessCommand(new ProcessCommandContext {command = command, hasServerPermissions = true});
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
                // there are multiple arguments - input can only be auto-completed using command handlers
                this.AutoCompleteUsingCommandHandler(context, out outExactCompletion, outPossibleCompletions);
                return;
            }

            // there is only 1 argument, the command itself, auto-complete it

            DoAutoCompletion(
                arguments[0],
                m_registeredCommands.Select(pair => pair.Key),
                out outExactCompletion,
                outPossibleCompletions);

            if (outExactCompletion == null && outPossibleCompletions.Count == 0)
            {
                // input could be equal to one of commands
                // ask the command handler to do auto-completion
                this.AutoCompleteUsingCommandHandler(context, out outExactCompletion, outPossibleCompletions);
            }
        }

        void AutoCompleteUsingCommandHandler(
            ProcessCommandContext context, out string outExactCompletion, List<string> outPossibleCompletions)
        {
            outExactCompletion = null;

            var arguments = SplitCommandIntoArguments(context.command);

            if (!m_registeredCommands.TryGetValue(arguments[0], out CommandInfo commandInfo))
                return;

            if (null == commandInfo.autoCompletionHandler)
                return;

            context.commandOnly = commandInfo.command;
            context.arguments = arguments;

            var result = commandInfo.autoCompletionHandler(context);

            outExactCompletion = result.response;
            if (result.autoCompletions != null)
                outPossibleCompletions.AddRange(result.autoCompletions);
        }

        static void DoAutoCompletion(
            string input,
            IEnumerable<string> availableOptions,
            out string outExactCompletion,
            List<string> outPossibleCompletions)
        {
            outExactCompletion = null;

            var stringComparison = System.StringComparison.InvariantCultureIgnoreCase;

            var optionsStartingWith = new List<string>();

            foreach (string option in availableOptions)
            {
                if (option.StartsWith(input, stringComparison))
                    optionsStartingWith.Add(option);
            }

            if (optionsStartingWith.Count == 0)
                return;

            // find common prefix

            string optionToTest = optionsStartingWith[0];

            string commonPrefix = input;
            int startIndex = commonPrefix.Length;

            for (int i = startIndex; i < optionToTest.Length; i++)
            {
                char ch = optionToTest[i];

                // if all other options have this char at this index, it is part of common prefix

                bool allOptionsHaveThisChar = true;

                for (int j = 1; j < optionsStartingWith.Count; j++)
                {
                    string option = optionsStartingWith[j];
                    
                    if (i >= option.Length || char.ToLowerInvariant(option[i]) != char.ToLowerInvariant(ch))
                    {
                        allOptionsHaveThisChar = false;
                        break;
                    }
                }

                if (!allOptionsHaveThisChar)
                    break;

                commonPrefix += ch;
            }

            if (commonPrefix.Equals(input, stringComparison))
            {
                // common prefix is equal to input
                // no need to auto-complete, only return all possible completions

                if (optionsStartingWith.Count > 1) // don't return 1 option only (which would be equal to input)
                {
                    outPossibleCompletions.AddRange(optionsStartingWith);
                }
                else
                {
                    // only 1 option shares common prefix with input - it means input is equal to that option
                    // no need to do anything here
                }

                return;
            }

            // common prefix is not equal to input (it's shorter) - expand it
            // auto-complete the input into common prefix
            outExactCompletion = commonPrefix;
        }

        public ProcessCommandResult ProcessCommandAutoCompletion(
            ProcessCommandContext context, IEnumerable<string> availableOptions)
        {
            if (context.NumArguments <= 1)
            {
                // no option specified
                // list all available options
                return ProcessCommandResult.AutoCompletion(null, availableOptions);
            }

            // already has an option specified
            string category = context.ReadString();
            var possibleCompletions = new List<string>();
            CommandManager.DoAutoCompletion(
                category, availableOptions, out string exactCompletion, possibleCompletions);

            if (exactCompletion != null)
                exactCompletion = this.CombineArguments(context.commandOnly, exactCompletion);

            return ProcessCommandResult.AutoCompletion(exactCompletion, possibleCompletions);
        }
    }
}
