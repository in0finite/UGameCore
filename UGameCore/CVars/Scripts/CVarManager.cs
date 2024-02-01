using System.Collections.Generic;
using UnityEngine;
using UGameCore.Utilities;
using System.Linq;

namespace UGameCore
{
    // This script should execute before most of other scripts in order to register ConfigVars and assign their values from config,
    // before Awake() is called on other scripts.
    [DefaultExecutionOrder(-30000)]
    public class CVarManager : MonoBehaviour
	{
		IConfigProvider m_configProvider;

        private readonly Dictionary<string, ConfigVar>	m_configVars = new(System.StringComparer.OrdinalIgnoreCase);
		public	IReadOnlyDictionary<string, ConfigVar>	ConfigVars => m_configVars;

		public bool scanMyself = true;
		public List<GameObject> objectsToScan = new();


		void Awake ()
		{
			var provider = this.GetComponentOrThrow<System.IServiceProvider>();
            m_configProvider = provider.GetRequiredService<IConfigProvider>();
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
				registrator.Register(context);
            }

			m_configVars.Clear();

			foreach (ConfigVar configVar in context.ConfigVars)
			{
				F.RunExceptionSafe(() =>
				{
                    this.ValidateConfigVarName(configVar.FinalSerializationName);
                    m_configVars.Add(configVar.FinalSerializationName, configVar);
                });
            }

			// assign config var values from config

			foreach (var pair in m_configVars)
			{
                ConfigVar configVar = pair.Value;

				if (configVar.Persist == ConfigVar.PersistType.None)
					continue;

				string valueStr = m_configProvider.GetProperty(pair.Key);
				if (valueStr == null)
					continue;

                F.RunExceptionSafe(() => configVar.SetValue(configVar.LoadValueFromString(valueStr)));
            }

            Debug.Log($"Loaded config vars [{m_configVars.Count}]");
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
    }
}
