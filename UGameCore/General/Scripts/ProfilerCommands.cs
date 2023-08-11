using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UGameCore.Utilities;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore
{
    public class ProfilerCommands : MonoBehaviour
    {
        public CommandManager commandManager;


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        [CommandMethod("profiler_list", "List all available profiler categories")]
        ProcessCommandResult ProfilerListCmd(ProcessCommandContext context)
        {
            //string categoryInput = context.ReadStringOrDefault(null);

            var availableStatHandles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(availableStatHandles);

            var categories = availableStatHandles.Select(_ => ProfilerRecorderHandle.GetDescription(_).Category.Name).Distinct().ToArray();

            var sb = new StringBuilder();
            sb.Append($"Available stats: {availableStatHandles.Count}\n");
            sb.Append($"Available categories [{categories.Length}]: {string.Join(", ", categories)}\n");
            
            return ProcessCommandResult.SuccessResponse(sb.ToString());
        }

        [CommandMethod("profiler_category", "List all available profiler stats in given category")]
        ProcessCommandResult ProfilerCategoryCmd(ProcessCommandContext context)
        {
            string categoryInput = context.ReadString();

            var availableStatHandles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(availableStatHandles);

            var descs = availableStatHandles
                .Select(_ => ProfilerRecorderHandle.GetDescription(_))
                .Where(_ => _.Category.Name.Equals(categoryInput, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var sb = new StringBuilder();
            sb.Append($"Available stats in {categoryInput} category [{descs.Length}]: \n");
            sb.Append($"{string.Join("\n", descs.Select(_ => _.Name + " - " + _.UnitType))}");

            return ProcessCommandResult.SuccessResponse(sb.ToString());
        }

        [CommandAutoCompletionMethod("profiler_category")]
        ProcessCommandResult ProfilerListAutoCompleteCmd(ProcessCommandContext context)
        {
            var availableStatHandles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(availableStatHandles);

            var categories = availableStatHandles
                .Select(_ => ProfilerRecorderHandle.GetDescription(_).Category.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (context.NumArguments <= 1)
                return ProcessCommandResult.AutoCompletion(null, categories);

            string input = context.ReadString();

            var possibleCompletions = new List<string>();

            CommandManager.DoAutoCompletion(
                input, categories, out string outExactCompletion, possibleCompletions);

            if (outExactCompletion != null)
                outExactCompletion = "profiler_category " + outExactCompletion;

            return ProcessCommandResult.AutoCompletion(outExactCompletion, possibleCompletions);
        }
    }
}
