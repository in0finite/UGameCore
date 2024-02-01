using System;
using UnityEngine;
using UGameCore.Utilities;
using static UGameCore.CommandManager;
using System.Text;

namespace UGameCore
{
    public class ConfigCommands : MonoBehaviour
    {
        public CommandManager commandManager;
        IConfigProvider m_configProvider;


        void Start()
        {
            var provider = this.GetSingleComponentOrThrow<IServiceProvider>();
            m_configProvider = provider.GetRequiredService<IConfigProvider>();

            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        [CommandMethod("config_dump", "List all config properties")]
        ProcessCommandResult Dump(ProcessCommandContext context)
        {
            var sb = new StringBuilder();
            foreach (string key in m_configProvider.GetKeys())
            {
                sb.Append(key);
                sb.Append(" = ");
                sb.Append(m_configProvider.GetProperty(key));
                sb.AppendLine();
            }

            return ProcessCommandResult.SuccessResponse(sb.ToString());
        }

        [CommandMethod("config_get", "Get config property by key", syntax = "(string key)")]
        ProcessCommandResult Get(ProcessCommandContext context)
        {
            string key = context.ReadString();
            string value = m_configProvider.GetProperty(key);
            if (value == null)
                return ProcessCommandResult.Error("Property not found");
            return ProcessCommandResult.SuccessResponse(value);
        }

        [CommandMethod("config_set", "Set config property's value", syntax = "(string key, string value)")]
        ProcessCommandResult Set(ProcessCommandContext context)
        {
            string key = context.ReadString();
            string value = context.ReadString();
            m_configProvider.SetProperty(key, value);
            return ProcessCommandResult.Success;
        }

        [CommandMethod("config_remove", "Remove config property by key", syntax = "(string key)")]
        ProcessCommandResult Remove(ProcessCommandContext context)
        {
            string key = context.ReadString();
            if (!m_configProvider.RemoveProperty(key))
                return ProcessCommandResult.Error("Property not found");
            return ProcessCommandResult.Success;
        }

        [CommandMethod("config_save", "Save config to permanent storage")]
        ProcessCommandResult Save(ProcessCommandContext context)
        {
            m_configProvider.Save();
            return ProcessCommandResult.Success;
        }

        [CommandMethod("config_clear", "Remove all properties from config")]
        ProcessCommandResult Clear(ProcessCommandContext context)
        {
            m_configProvider.Clear();
            return ProcessCommandResult.Success;
        }
    }
}
