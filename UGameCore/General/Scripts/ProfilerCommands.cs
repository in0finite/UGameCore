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

        List<RecorderInfo> m_profilerRecorders = new List<RecorderInfo>();

        class RecorderInfo
        {
            public ProfilerRecorder recorder;
            public ProfilerRecorderDescription desc;
            public int numFramesLeft;
        }


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

            m_profilerRecorders.RemoveAll(_ => _.numFramesLeft <= 0);

            foreach (RecorderInfo info in m_profilerRecorders)
            {
                info.numFramesLeft--;

                if (info.numFramesLeft > 0)
                    continue;

                double value = info.recorder.CurrentValueAsDouble;
                if (info.recorder.UnitType == ProfilerMarkerDataUnit.Bytes)
                    value /= (1024 * 1024);
                else if (info.recorder.UnitType == ProfilerMarkerDataUnit.TimeNanoseconds)
                    value /= (1000 * 1000);

                sb.Append(info.desc.Name);
                sb.Append(" :  ");
                sb.Append(value);
                sb.Append("  [");
                if (info.recorder.UnitType == ProfilerMarkerDataUnit.Bytes)
                    sb.Append("MB");
                else if (info.recorder.UnitType == ProfilerMarkerDataUnit.TimeNanoseconds)
                    sb.Append("ms");
                else
                    sb.Append(info.recorder.UnitType.ToString());
                sb.Append("]\n");

                info.recorder.Stop();
                info.recorder.Dispose();
            }

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

                m_profilerRecorders.Add(new RecorderInfo { recorder = recorder, desc = desc, numFramesLeft = 2 });
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
