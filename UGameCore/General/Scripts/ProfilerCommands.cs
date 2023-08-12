using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UGameCore.Utilities;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using static UGameCore.CommandManager;

namespace UGameCore
{
    public class ProfilerCommands : MonoBehaviour
    {
        public CommandManager commandManager;

        List<(ProfilerRecorder recorder, ProfilerRecorderDescription desc)> m_profilerRecorders = 
            new List<(ProfilerRecorder, ProfilerRecorderDescription)>();


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        void Update()
        {
            StringBuilder sb = null;
            if (m_profilerRecorders.Count > 0)
                sb = new StringBuilder();

            foreach (var recorder in m_profilerRecorders)
            {
                double value = recorder.recorder.CurrentValueAsDouble;
                if (recorder.recorder.UnitType == ProfilerMarkerDataUnit.Bytes)
                    value /= (1024 * 1024);
                else if (recorder.recorder.UnitType == ProfilerMarkerDataUnit.TimeNanoseconds)
                    value /= (1000 * 1000);

                sb.Append(recorder.desc.Name);
                sb.Append(" :  ");
                sb.Append(value);
                sb.Append("  [");
                if (recorder.recorder.UnitType == ProfilerMarkerDataUnit.Bytes)
                    sb.Append("MB");
                else if (recorder.recorder.UnitType == ProfilerMarkerDataUnit.TimeNanoseconds)
                    sb.Append("ms");
                else
                    sb.Append(recorder.recorder.UnitType.ToString());
                sb.Append("]\n");

                recorder.recorder.Stop();
                recorder.recorder.Dispose();
            }

            m_profilerRecorders.Clear();

            if (sb != null)
                Debug.Log(sb.ToString());
        }

        [CommandMethod("profiler_list", "List all available profiler categories")]
        ProcessCommandResult ProfilerListCmd(ProcessCommandContext context)
        {
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

        [CommandMethod("profiler_category_capture", "Capture all profiler stats in given category")]
        ProcessCommandResult ProfilerCategoryCaptureCmd(ProcessCommandContext context)
        {
            string categoryInput = context.ReadString();

            var availableStatHandles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(availableStatHandles);

            var descs = availableStatHandles
                .Select(_ => ProfilerRecorderHandle.GetDescription(_))
                .Where(_ => _.Category.Name.Equals(categoryInput, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (descs.Length == 0)
                return ProcessCommandResult.Error($"Category {categoryInput} not found");

            foreach (ProfilerRecorderDescription desc in descs)
            {
                var recorder = new ProfilerRecorder(
                    new ProfilerMarker(desc.Category, desc.Name),
                    1,
                    ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.Default);

                m_profilerRecorders.Add((recorder, desc));
            }

            return ProcessCommandResult.Success;
        }

        [CommandAutoCompletionMethod("profiler_category")]
        [CommandAutoCompletionMethod("profiler_category_capture")]
        ProcessCommandResult ProfilerCategoryAutoCompleteCmd(ProcessCommandContext context)
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
                outExactCompletion = this.commandManager.CombineArguments(context.commandOnly, outExactCompletion);

            return ProcessCommandResult.AutoCompletion(outExactCompletion, possibleCompletions);
        }
    }
}
