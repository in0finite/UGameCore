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

        readonly Dictionary<string, CommandInfo> m_registeredCommands = new(System.StringComparer.OrdinalIgnoreCase);

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
        public readonly HashSet<string> ForbiddenCommands = new(System.StringComparer.OrdinalIgnoreCase);

        static readonly Dictionary<char, char> UnescapedCharsMapping = new()
        {
            { 't', '\t' },
            { 'r', '\r' },
            { 'n', '\n' },
            { '0', '\0' },
            { '\\', '\\' },
            { '\"', '\"' },
            { '\'', '\'' },
        };

        /// <summary>
        /// Annotate a method with this attribute to register it as a command.
        /// </summary>
        [System.AttributeUsageAttribute(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
        public class CommandMethodAttribute : System.Attribute
        {
            public string command;
            public string[] commandAliases = System.Array.Empty<string>();
            public string commandAlias;
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
        public struct ProcessCommandResult
        {
            public int exitCode;
            public string response;
            public System.Exception exception;
            public List<string> autoCompletions;

            public readonly bool IsSuccess => this.exitCode == 0;

            public static ProcessCommandResult UnknownCommand(string cmd) => Error($"Unknown command: {cmd}");
            public static ProcessCommandResult InvalidCommand => Error("Invalid command");
            public static ProcessCommandResult ForbiddenCommand => Error("Forbidden command");
            public static ProcessCommandResult NoPermissions => Error("You don't have permissions to run this command");
            public static ProcessCommandResult CanOnlyRunOnServer => Error("This command can only run on server");
            public static ProcessCommandResult LimitInterval(float interval) => Error($"This command can only be used on an interval of {interval} seconds");
            public static ProcessCommandResult Error(string errorMessage)
                => new ProcessCommandResult { exitCode = 1, response = errorMessage };
            public static ProcessCommandResult Exception(System.Exception ex)
                => new ProcessCommandResult { exitCode = 1, exception = ex };
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

            int currentArgumentIndex = 1;


            public ProcessCommandContext Clone()
            {
                return (ProcessCommandContext)this.MemberwiseClone();
            }

            public bool HasNextArgument()
            {
                return this.currentArgumentIndex < this.NumArguments;
            }

            public void SkipNextArgument()
            {
                this.ReadString();
            }

            public string GetRestOfTheCommand(int argumentIndex = 1, string separator = " ")
            {
                return string.Join(separator, this.arguments, argumentIndex, this.arguments.Length - argumentIndex);
            }

            /// <summary>
            /// Peek next command argument as string.
            /// </summary>
            public string PeekString()
            {
                if (this.currentArgumentIndex >= this.NumArguments)
                    throw new System.ArgumentException($"Trying to read command argument out of bounds (index {this.currentArgumentIndex}, num arguments {this.NumArguments})");

                string arg = this.arguments[this.currentArgumentIndex];
                return arg;
            }

            /// <summary>
            /// Read next command argument as string.
            /// </summary>
            public string ReadString()
            {
                string str = this.PeekString();
                this.currentArgumentIndex++;
                return str;
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

            T? ReadNullableUsingFunction<T>(System.Func<ProcessCommandContext, T> func)
                where T : struct
            {
                string str = this.PeekString();
                if (string.IsNullOrEmpty(str))
                {
                    this.SkipNextArgument();
                    return default;
                }

                return func(this);
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
            /// Read next command argument as ulong.
            /// </summary>
            public ulong ReadUlong()
            {
                string str = this.ReadString();
                return ulong.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }

            static float ParseFloat(string str)
                => float.Parse(str, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

            /// <summary>
            /// Read next command argument as float.
            /// </summary>
            public float ReadFloat()
            {
                string str = this.ReadString();
                return ParseFloat(str);
            }

            /// <summary>
            /// Read next command argument as Vector2, by reading 2 floats.
            /// </summary>
            public Vector2 ReadVector2As2Floats()
            {
                return new Vector2(this.ReadFloat(), this.ReadFloat());
            }

            /// <summary>
            /// Read next command argument as Vector3, by reading 3 floats.
            /// </summary>
            public Vector3 ReadVector3As3Floats()
            {
                return new Vector3(this.ReadFloat(), this.ReadFloat(), this.ReadFloat());
            }

            public Vector3 ReadVector3()
            {
                string str = this.ReadString();

                string[] parts = str.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 3)
                    throw new System.ArgumentException($"Expected 3 floats for Vector3 argument, found {parts.Length}");

                Vector3 v = default;
                for (int i = 0; i < 3; i++)
                    v[i] = ParseFloat(parts[i]);

                return v;
            }

            public Vector3? ReadNullableVector3() => this.ReadNullableUsingFunction(static c => c.ReadVector3());

            public Vector3? ReadNullableVector3OrDefault(Vector3? defaultValue = default)
            {
                return this.HasNextArgument() ? this.ReadNullableVector3() : defaultValue;
            }

            /// <summary>
            /// Read next command argument as bool.
            /// </summary>
            public bool ReadBool()
            {
                string str = this.ReadString();

                if (bool.TryParse(str, out bool boolValue))
                    return boolValue;

                if (int.TryParse(str, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int intValue))
                {
                    if (intValue == 0)
                        return false;
                    if (intValue == 1)
                        return true;
                }

                throw new System.ArgumentException($"Invalid boolean value: {str}. Use 1|0 or true|false.");
            }

            public bool? ReadNullableBool() => ReadNullableUsingFunction(static c => c.ReadBool());

            /// <summary>
            /// Read next command argument as Enum.
            /// </summary>
            public T ReadEnum<T>()
                where T : struct, System.Enum
            {
                string str = this.ReadString();
                T value = System.Enum.Parse<T>(str, true);
                if (!System.Enum.IsDefined(typeof(T), value))
                    throw new System.ArgumentException($"Specified {typeof(T).Name} value is not valid: {value}");
                return value;
            }

            public T? ReadNullableEnum<T>()
                where T : struct, System.Enum
                => this.ReadNullableUsingFunction(static c => c.ReadEnum<T>());

            public T ReadGeneric<T>()
            {
                string str = this.ReadString();
                return (T)System.Convert.ChangeType(str, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }

            public T ReadGenericOrDefault<T>(T defaultValue = default)
            {
                return this.HasNextArgument() ? this.ReadGeneric<T>() : defaultValue;
            }

            public T? ReadNullableGeneric<T>()
                where T : struct
                => this.ReadNullableUsingFunction(static c => c.ReadGeneric<T>());

            public T? ReadNullableGenericOrDefault<T>(T? defaultValue = default)
                where T : struct
            {
                return this.HasNextArgument() ? this.ReadNullableGeneric<T>() : defaultValue;
            }
        }



        void Awake()
        {
            if (null == Singleton)
                Singleton = this;

            this.ForbiddenCommands.UnionWith(m_forbiddenCommandsList);
        }

        public CommandInfo GetCommandOrThrow(string command)
        {
            return m_registeredCommands.TryGetValue(command, out CommandInfo commandInfo)
                ? commandInfo
                : throw new System.ArgumentException($"Command not found: {command}");
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

        public void RegisterCommandAlias(string existingCommand, string alias)
        {
            CommandInfo commandInfo = this.GetCommandOrThrow(existingCommand);

            commandInfo.command = alias;
            commandInfo.description = $"(Alias for '{existingCommand}') {commandInfo.description}";

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

                    F.RunExceptionSafeArg2(this, commandInfo, static (arg1, arg2) => arg1.RegisterCommand(arg2));

                    // register aliases
                    string[] commandAliases = attr.commandAlias.IsNullOrEmpty()
                        ? attr.commandAliases
                        : new string[] { attr.commandAlias }.AppendToArray(attr.commandAliases);
                    foreach (string alias in commandAliases)
                    {
                        F.RunExceptionSafeArg3(this, attr, alias, static (arg1, arg2, arg3) => arg1.RegisterCommandAlias(arg2.command, arg3));
                    }
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

        public static List<string[]> SplitMultipleCommandsIntoArguments(string command)
        {
            var commands = new List<string[]>();
            var arguments = new List<string>();

            // trim is fine here, because arguments can not have whitespaces at start/end without using quotes
            command = command.Trim();
            
            int argumentStartIndex = -1;
            char startingQuoteChar = (char)0;
            bool lastCharWasEscape = false;


            void EndCurrentArgument(int i, bool bAllowEmptyArg)
            {
                string argument = command.Substring(argumentStartIndex + 1, i - argumentStartIndex - 1);
                if (argument.Length > 0 || bAllowEmptyArg)
                    arguments.Add(argument);
                argumentStartIndex = i;
            }

            void EndCurrentCommand()
            {
                if (arguments.Count > 0)
                    commands.Add(arguments.ToArray());
                arguments.Clear();
            }


            for (int i = 0; i < command.Length; i++)
            {
                char ch = command[i];

                bool thisCharIsEscaped = lastCharWasEscape;
                lastCharWasEscape = false;
                bool isInsideQuotes = startingQuoteChar != 0;

                if (!thisCharIsEscaped && ch == '\\')
                {
                    lastCharWasEscape = true;
                    continue;
                }

                // split logic based on whether in quotes or not

                if (isInsideQuotes)
                {
                    // check if we need to close quotes
                    if (IsQuote(ch) && !thisCharIsEscaped && ch == startingQuoteChar)
                    {
                        if (i == command.Length - 1 || IsCommandOrArgumentSeparator(command[i + 1]))
                        {
                            // separator is after this char, end current argument

                            EndCurrentArgument(i, true); // do not Trim() here, arguments are allowed to have whitespaces at start/end

                            startingQuoteChar = (char)0;

                            continue;
                        }
                    }

                    continue;
                }

                // --------- not inside quotes ---------

                // check for command separator
                if (IsCommandSeparator(ch))
                {
                    // end current command here

                    EndCurrentArgument(i, false);

                    EndCurrentCommand();

                    continue;
                }

                if (IsArgumentSeparator(ch))
                {
                    // end current argument here
                    EndCurrentArgument(i, false); // only add it if not empty, because a whitespace outside of quotes should not be an argument
                    continue;
                }

                if (IsQuote(ch) && !thisCharIsEscaped)
                {
                    if (i == 0 || IsCommandOrArgumentSeparator(command[i - 1]))
                    {
                        // separator is before this char, open new argument
                        startingQuoteChar = ch;
                        argumentStartIndex = i;

                        continue;
                    }

                    continue;
                }
            }

            // add the remaining argument

            // here we only need to trim from end, because argument may have quotes,
            // but because the command was trimmed at beginning, we don't need to do it

            EndCurrentArgument(command.Length, false);

            EndCurrentCommand();

            foreach (string[] args in commands)
            {
                args.ReplaceEach(static arg => UnescapeArgument(arg));
            }

            return commands;
        }

        public static string[] SplitSingleCommandIntoArguments(string command)
        {
            List<string[]> commands = SplitMultipleCommandsIntoArguments(command);
            if (commands.Count > 1)
                throw new System.ArgumentException($"Found multiple commands ({commands.Count}) while trying to split arguments of single command");
            if (commands.Count == 0)
                throw new System.ArgumentException($"No commands found");

            return commands[0];
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
                    // special char indicating that next char should be unescaped
                    lastCharWasUnescapeChar = true;
                    list[i] = (char)0; // do not include this special char in final string
                    continue;
                }

                if (thisCharIsUnescaped)
                {
                    if (UnescapedCharsMapping.TryGetValue(ch, out char unescapedChar))
                        list[i] = unescapedChar;
                    else
                        list[i] = (char)0;
                }
            }

            list.RemoveAll(ch => ch == 0); // remove all special chars

            return new string(list.ListAsSpan());
        }

        static bool IsCommandSeparator(char c) => c == ';' || c == '\n';

        static bool IsArgumentSeparator(char c) => char.IsWhiteSpace(c);

        static bool IsCommandOrArgumentSeparator(char c) => IsCommandSeparator(c) || IsArgumentSeparator(c);

        static bool IsQuote(char c) => c == '\'' || c == '\"';

        public string CombineArguments(params string[] arguments)
        {
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < arguments.Length; i++)
            {
                string arg = arguments[i]; // don't trim, arguments are allowed to have whitespaces at start/end

                bool isEmpty = arg.Length == 0;
                bool hasSeparator = arg.Any(static c => IsCommandOrArgumentSeparator(c));
                bool needsQuotes = isEmpty || hasSeparator;

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

        public string CombineCommands(List<string[]> commandsWithArguments)
        {
            return CombineCommands(commandsWithArguments.Select(_ => CombineArguments(_)).ToArray());
        }

        public string CombineCommands(string[] commands)
        {
            return string.Join("; ", commands);
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
            string[] arguments = SplitSingleCommandIntoArguments(context.command);
            return ProcessCommandInternal(context, arguments);
        }

        ProcessCommandResult ProcessCommandInternal(ProcessCommandContext context, string[] arguments)
        {
            if (string.IsNullOrEmpty(context.command))
                return ProcessCommandResult.InvalidCommand;

            if (context.command.Length > this.maxNumCharactersInCommand)
                return ProcessCommandResult.Error("Command too long");

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
                return ProcessCommandResult.Error($"Command requires {commandInfo.exactNumArguments.Value} arguments");

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

            List<string[]> commandsWithArguments = SplitMultipleCommandsIntoArguments(context.command);
            if (commandsWithArguments.Count == 0)
                return;

            string[] arguments = commandsWithArguments.Last();
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
                return;
            }

            InsertPreviousCommandsIntoAutoCompletionResult(commandsWithArguments, ref outExactCompletion, outPossibleCompletions);
        }

        void AutoCompleteUsingCommandHandler(
            ProcessCommandContext context, out string outExactCompletion, List<string> outPossibleCompletions)
        {
            outExactCompletion = null;

            List<string[]> commandsWithArguments = SplitMultipleCommandsIntoArguments(context.command);
            if (commandsWithArguments.Count == 0)
                return;

            string[] arguments = commandsWithArguments.Last();
            if (0 == arguments.Length)
                return;

            if (!m_registeredCommands.TryGetValue(arguments[0], out CommandInfo commandInfo))
                return;

            if (null == commandInfo.autoCompletionHandler)
                return;

            context.commandOnly = commandInfo.command;
            context.arguments = arguments;

            ProcessCommandResult result = commandInfo.autoCompletionHandler(context);

            outExactCompletion = result.response;
            if (result.autoCompletions != null)
                outPossibleCompletions.AddRange(result.autoCompletions);

            InsertPreviousCommandsIntoAutoCompletionResult(commandsWithArguments, ref outExactCompletion, outPossibleCompletions);
        }

        void InsertPreviousCommandsIntoAutoCompletionResult(
            List<string[]> commandsWithArguments,
            ref string outExactCompletion,
            List<string> outPossibleCompletions)
        {
            List<string[]> previousCommands = commandsWithArguments.Take(commandsWithArguments.Count - 1).ToList();
            string previousCommand = CombineCommands(previousCommands);

            if (string.IsNullOrEmpty(previousCommand))
                return;

            if (outExactCompletion != null)
            {
                outExactCompletion = CombineCommands(new string[] { previousCommand, outExactCompletion });
            }

            // possible completions should not have previous commands inserted into them
        }

        static void DoAutoCompletion(
            string input,
            IEnumerable<string> availableOptions,
            out string outExactCompletion,
            List<string> outPossibleCompletions)
        {
            outExactCompletion = null;

            var stringComparison = System.StringComparison.OrdinalIgnoreCase;

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

        public string GetCommandsFilePath(string relativeFileName)
        {
            // make sure that other files can not be read - restrict to ".cfg" file extension
            if (!relativeFileName.EndsWith(".cfg", System.StringComparison.OrdinalIgnoreCase))
                throw new System.ArgumentException($"File name must end with '.cfg'");

            // allow any path, no need to prevent different folders or drives

            //if (relativeFileName.ContainsAnyChar("/\\", System.StringComparison.OrdinalIgnoreCase))
            //    throw new System.ArgumentException($"Invalid file name: {relativeFileName}");

            string dir = F.IsOnDesktopPlatform ? Application.dataPath : Application.persistentDataPath;

            string fullPath = System.IO.Path.Join(dir, relativeFileName);

            return fullPath;
        }

        public ProcessCommandResult[] ProcessCommandsFromFile(ProcessCommandContext context, string relativeFileName)
        {
            string commandsText = System.IO.File.ReadAllText(GetCommandsFilePath(relativeFileName));

            context = context.Clone();
            context.command = commandsText;
            return this.ProcessMultipleCommands(context);
        }

        public ProcessCommandResult[] ProcessMultipleCommands(ProcessCommandContext context)
        {
            List<string[]> commands = SplitMultipleCommandsIntoArguments(context.command);
            ProcessCommandResult[] results = new ProcessCommandResult[commands.Count];

            context = context.Clone();

            for (int i = 0; i < commands.Count; i++)
            {
                string[] arguments = commands[i];
                context.command = CombineArguments(arguments);

                ProcessCommandResult result;
                try
                {
                    result = ProcessCommandInternal(context, arguments);
                }
                catch (System.Exception ex)
                {
                    result = ProcessCommandResult.Exception(ex);
                }

                results[i] = result;
            }

            return results;
        }

        public void LogCommandResult(ProcessCommandResult result, Object contextObject)
        {
            if (result.exception != null)
            {
                Debug.LogException(result.exception, contextObject);
                return;
            }

            if (!result.IsSuccess)
            {
                Debug.LogError(result.response, contextObject);
                return;
            }

            if (result.response != null) // don't log empty successful response
            {
                Debug.Log(result.response, contextObject);
            }
        }
    }
}
