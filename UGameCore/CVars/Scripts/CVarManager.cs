using System.Collections.Generic;
using UnityEngine;
using UGameCore.Utilities;
using System.Linq;
using static UGameCore.CommandManager;

namespace UGameCore
{
    // This script should execute before most of other scripts in order to register ConfigVars and assign their values from config,
    // before Awake() is called on other scripts.
    [DefaultExecutionOrder(-30000)]
    public class CVarManager : MonoBehaviour
	{
		IConfigProvider m_configProvider;
		CommandManager m_commandManager;

        readonly Dictionary<string, ConfigVar>	m_configVars = new(System.StringComparer.OrdinalIgnoreCase);
		public	IReadOnlyDictionary<string, ConfigVar>	ConfigVars => m_configVars;

        readonly Dictionary<string, string> m_configVarAliases = new(System.StringComparer.OrdinalIgnoreCase);

        public bool scanMyself = true;
		public List<GameObject> objectsToScan = new();


		void Awake ()
		{
			var provider = this.GetComponentOrThrow<System.IServiceProvider>();
            m_configProvider = provider.GetRequiredService<IConfigProvider>();
            m_commandManager = provider.GetRequiredService<CommandManager>();
            this.LoadConfigVars();
		}

		public ConfigVar GetConfigVarByNameOrAlias(string nameOrAlias)
		{
			if (m_configVars.TryGetValue(nameOrAlias, out ConfigVar configVar))
				return configVar;

            if (m_configVarAliases.TryGetValue(nameOrAlias, out string n))
                return m_configVars[n];

			throw new System.ArgumentException($"{nameof(ConfigVar)} not found by name or alias: {nameOrAlias}");
        }

        public void ChangeCVars(ConfigVar[] cvarsToChange, ConfigVarValue[] newValues)
		{
			if (cvarsToChange.Length != newValues.Length)
				throw new System.ArgumentException();

			for (int i = 0; i < cvarsToChange.Length; i++)
			{
				F.RunExceptionSafe(() => SetConfigVarValueWithConfigUpdate(cvarsToChange[i], newValues[i]));
			}
		}

		private void ResetAllCVarsToDefaultValues()
		{
			foreach (var pair in m_configVars)
			{
                ConfigVar cvar = pair.Value;
				F.RunExceptionSafe(() => SetConfigVarValueWithConfigUpdate(cvar, cvar.DefaultValue));
            }
		}

		void SetConfigVarValueWithConfigUpdate(ConfigVar cvar, ConfigVarValue configVarValue)
        {
            cvar.SetValue(configVarValue);

            if (cvar.Persist != ConfigVar.PersistType.None)
            {
                m_configProvider.SetProperty(cvar.FinalSerializationName, cvar.SaveValueToString(cvar.GetValue()));
            }
        }

        private void LoadConfigVars()
        {
			var registrators = new List<IConfigVarRegistrator>();
            var tempRegistrators = new List<IConfigVarRegistrator>();

            foreach (GameObject go in this.scanMyself ? new[] { this.gameObject }.Concat(this.objectsToScan) : this.objectsToScan)
			{
				go.GetComponentsInChildren(tempRegistrators);
				registrators.AddRange(tempRegistrators);
            }

			var context = new IConfigVarRegistrator.Context();
            foreach (IConfigVarRegistrator registrator in registrators)
            {
				F.RunExceptionSafe(() => registrator.Register(context));
            }

            m_configVars.Clear();

			foreach (ConfigVar configVar in context.ConfigVars)
			{
				F.RunExceptionSafe(() => this.RegisterConfigVar(configVar));
            }

			// assign config var values from config

			foreach (var pair in m_configVars)
			{
                ConfigVar configVar = pair.Value;

				if (configVar.Persist == ConfigVar.PersistType.None)
					continue;

				string valueStr = m_configProvider.GetProperty(pair.Key);
				if (valueStr == null && !configVar.ApplyDefaultValueWhenNotPresentInConfig)
					continue;

                F.RunExceptionSafe(() => configVar.SetValue(valueStr == null ? configVar.DefaultValue : configVar.LoadValueFromString(valueStr)));
            }
		}

		public void RegisterConfigVar(ConfigVar configVar)
		{
			string serializationName = configVar.FinalSerializationName;

            this.ValidateConfigVarName(serializationName);

			m_commandManager.RegisterCommand(new CommandManager.CommandInfo
			{
				command = serializationName,
				description = $"{configVar.Description}\r\nDefault value: {configVar.DescribeValue(configVar.DefaultValue)}\r\n{configVar.GetAdditionalDescription()}",
				maxNumArguments = 1,
                commandHandler = this.ProcessCommand,
				autoCompletionHandler = this.ProcessCommandAutoCompletion,
            });

            m_configVars.Add(serializationName, configVar);

            foreach (string alias in configVar.Aliases)
            {
                m_commandManager.RegisterCommandAlias(serializationName, alias);
				m_configVarAliases.Add(alias, serializationName);
            }
        }

        void ValidateConfigVarName(string serializationName)
		{
			if (string.IsNullOrWhiteSpace(serializationName))
				throw new System.ArgumentException("Config var name can not be empty");

			if (serializationName.Any(c => !char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.'))
				throw new System.ArgumentException($"Config var names can only have letters or digits: {serializationName}");
        }

        public bool IsCVarValueValid(ConfigVar cvar, ConfigVarValue value)
		{
			try
			{
                cvar.Validate(value);
				return true;
            }
			catch
			{
				return false;
			}
		}

		public void SaveConfigVars()
		{
			m_configProvider.Save();
		}

        ProcessCommandResult ProcessCommand(ProcessCommandContext context)
        {
			ConfigVar configVar = this.GetConfigVarByNameOrAlias(context.commandOnly);
            CommandInfo commandInfo = m_commandManager.RegisteredCommandsDict[context.commandOnly];

			bool hasValue = context.HasNextArgument();
			string valueStr = context.ReadStringOrDefault(null);

			if (!hasValue)
				return ProcessCommandResult.SuccessResponse(configVar.DescribeValue(configVar.GetValue()) + "\r\n\r\n" + commandInfo.description);
			
			this.SetConfigVarValueWithConfigUpdate(configVar, configVar.LoadValueFromString(valueStr));

            return ProcessCommandResult.Success;
        }

        ProcessCommandResult ProcessCommandAutoCompletion(ProcessCommandContext context)
		{
			// only auto-complete if configvar's value is not given
			if (context.NumArguments > 1)
				return ProcessCommandResult.AutoCompletion(null, null);

            ConfigVar configVar = this.GetConfigVarByNameOrAlias(context.commandOnly);

            string valueStr = configVar.SaveValueToString(configVar.GetValue());

            return ProcessCommandResult.AutoCompletion(
				m_commandManager.CombineArguments(context.commandOnly, valueStr),
				null);
        }
    }
}
