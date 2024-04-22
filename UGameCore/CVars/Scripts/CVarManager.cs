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

        private readonly Dictionary<string, ConfigVar>	m_configVars = new(System.StringComparer.OrdinalIgnoreCase);
		public	IReadOnlyDictionary<string, ConfigVar>	ConfigVars => m_configVars;

		public bool scanMyself = true;
		public List<GameObject> objectsToScan = new();


		void Awake ()
		{
			var provider = this.GetComponentOrThrow<System.IServiceProvider>();
            m_configProvider = provider.GetRequiredService<IConfigProvider>();
            m_commandManager = provider.GetRequiredService<CommandManager>();
            this.LoadConfigVars();
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
            this.ValidateConfigVarName(configVar.FinalSerializationName);

			m_commandManager.RegisterCommand(new CommandManager.CommandInfo
			{
				command = configVar.FinalSerializationName,
				description = configVar.Description,
				maxNumArguments = 1,
                commandHandler = this.ProcessCommand,
				autoCompletionHandler = this.ProcessCommandAutoCompletion,
            });

            m_configVars.Add(configVar.FinalSerializationName, configVar);
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
			ConfigVar configVar = m_configVars[context.commandOnly];

			string valueStr = context.ReadStringOrDefault(null);
			if (string.IsNullOrWhiteSpace(valueStr))
                return ProcessCommandResult.SuccessResponse(configVar.SaveValueToString(configVar.GetValue()));

			this.SetConfigVarValueWithConfigUpdate(configVar, configVar.LoadValueFromString(valueStr));

            return ProcessCommandResult.Success;
        }

        ProcessCommandResult ProcessCommandAutoCompletion(ProcessCommandContext context)
		{
			// only auto-complete if configvar's value is not given
			if (context.NumArguments > 1)
				return ProcessCommandResult.AutoCompletion(null, null);

            ConfigVar configVar = m_configVars[context.commandOnly];

            string valueStr = configVar.SaveValueToString(configVar.GetValue());

            return ProcessCommandResult.AutoCompletion(
				m_commandManager.CombineArguments(context.commandOnly, valueStr),
				null);
        }
    }
}
