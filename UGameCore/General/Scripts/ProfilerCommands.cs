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
        List<RecorderInfo> m_nonSortedProfilerRecorders = new List<RecorderInfo>();

        class RecorderInfo
        {
            public ProfilerRecorder recorder;
            public string name;
            public int numFramesLeft;
        }


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        void Update()
        {
            UpdateList(m_profilerRecorders, true);
            UpdateList(m_nonSortedProfilerRecorders, false);
        }

        void UpdateList(List<RecorderInfo> recorderList, bool bSort)
        {
            recorderList.RemoveAll(_ => _.numFramesLeft <= 0);

            List<(double, ProfilerMarkerDataUnit, string)> valuesToLog = null;

            foreach (RecorderInfo info in recorderList)
            {
                info.numFramesLeft--;

                if (info.numFramesLeft > 0)
                    continue;

                valuesToLog ??= new List<(double, ProfilerMarkerDataUnit, string)>();

                valuesToLog.Add((info.recorder.CurrentValueAsDouble, info.recorder.UnitType, info.name));

                info.recorder.Stop();
                info.recorder.Dispose();
            }

            if (valuesToLog == null)
                return;

            if (bSort)
                valuesToLog.SortBy(_ => -_.Item1);

            var sb = new StringBuilder();

            foreach (var valueInfo in valuesToLog)
            {
                double value = valueInfo.Item1;
                ProfilerMarkerDataUnit unitType = valueInfo.Item2;

                if (unitType == ProfilerMarkerDataUnit.Bytes)
                    value /= (1024 * 1024);
                else if (unitType == ProfilerMarkerDataUnit.TimeNanoseconds)
                    value /= (1000 * 1000);

                sb.Append(valueInfo.Item3);
                sb.Append(" :  ");
                sb.Append(value);
                sb.Append("  [");
                if (unitType == ProfilerMarkerDataUnit.Bytes)
                    sb.Append("MB");
                else if (unitType == ProfilerMarkerDataUnit.TimeNanoseconds)
                    sb.Append("ms");
                else
                    sb.Append(unitType.ToString());
                sb.Append("]\n");
            }

            Debug.Log(sb.ToString());
        }

        [CommandMethod("profiler_list", "List all available profiler categories", maxNumArguments = 0)]
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

        [CommandMethod("profiler_category", "List all available profiler stats in given category", exactNumArguments = 1)]
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

        [CommandMethod("profiler_category_capture", "Capture all profiler stats in given category", exactNumArguments = 1)]
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

                m_profilerRecorders.Add(new RecorderInfo { recorder = recorder, name = desc.Name, numFramesLeft = 2 });
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

        [CommandMethod("profiler_summary", "Capture summary of profiler stats", maxNumArguments = 0)]
        ProcessCommandResult ProfilerSummarizeCmd(ProcessCommandContext context)
        {
            var list = new List<(ProfilerCategory, string)>()
            {
                (ProfilerCategory.Internal, "Semaphore.WaitForSignal"),
                (ProfilerCategory.Internal, "Main Thread"),
                (ProfilerCategory.Internal, "Idle"),

                (ProfilerCategory.Render, "CPU Total Frame Time"),
                (ProfilerCategory.Render, "CPU Main Thread Frame Time"),
                (ProfilerCategory.Render, "CPU Render Thread Frame Time"),
                (ProfilerCategory.Render, "Render Textures Bytes"),
                (ProfilerCategory.Render, "Render Textures Count"),
                (ProfilerCategory.Render, "Visible Skinned Meshes Count"),

                (ProfilerCategory.Physics, "Physics.Processing"),

                (ProfilerCategory.Memory, "Total Reserved Memory"),
                (ProfilerCategory.Memory, "Total Used Memory"),
                (ProfilerCategory.Memory, "Texture Memory"),
                (ProfilerCategory.Memory, "Texture Count"),
                (ProfilerCategory.Memory, "Gfx Reserved Memory"),
                (ProfilerCategory.Memory, "GC Reserved Memory"),
                (ProfilerCategory.Memory, "GC Used Memory"),
                (ProfilerCategory.Memory, "Mesh Memory"),
                (ProfilerCategory.Memory, "Mesh Count"),
                (ProfilerCategory.Memory, "Physics Used Memory"),
                (ProfilerCategory.Memory, "Game Object Count"),
            };

            foreach (var item in list)
            {
                var recorder = ProfilerRecorder.StartNew(item.Item1, item.Item2);
                m_nonSortedProfilerRecorders.Add(new RecorderInfo { recorder = recorder, name = item.Item2, numFramesLeft = 2 });
            }

            return ProcessCommandResult.Success;
        }
    }
}
