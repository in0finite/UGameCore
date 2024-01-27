using System.Linq;
using UnityEngine;
using UGameCore.Utilities;
using static UGameCore.CommandManager;
using System.Collections.Generic;
using System;
using System.Text;

namespace UGameCore
{
    public class StatsCommands : MonoBehaviour
    {
        public CommandManager commandManager;
        public GameObject[] objectsToCollectStatsFrom;


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        List<IStatsCollectable> GetStatsCollectables()
        {
            var collectables = new List<IStatsCollectable>();
            var tempList = new List<IStatsCollectable>();
            foreach (var go in objectsToCollectStatsFrom)
            {
                tempList.Clear();
                go.GetComponentsInChildren(tempList);
                collectables.AddRange(tempList);
            }
            return collectables;
        }

        [CommandMethod("stats", "Displays statistics", syntax = "(string category)")]
        ProcessCommandResult StatsCmd(ProcessCommandContext context)
        {
            string category = context.ReadStringOrDefault(null);

            var collectables = this.GetStatsCollectables();

            var statsContext = new IStatsCollectable.Context()
            {
                categoryToProcess = category,
            };

            foreach (IStatsCollectable collectable in collectables)
            {
                F.RunExceptionSafe(() => collectable.DumpStats(statsContext));
            }

            var sb = new StringBuilder(1024);
            foreach (var pair in statsContext.StringBuildersPerCategory)
            {
                StringBuilder sbForCategory = pair.Value;
                if (sbForCategory == null)
                    continue;

                sb.Append("Category: ");
                sb.AppendLine(pair.Key.ToUpperInvariant());
                sb.AppendLine();
                sb.Append(sbForCategory);
                sb.AppendLine();
                sb.AppendLine();
            }

            return ProcessCommandResult.SuccessResponse(sb.ToString());
        }

        [CommandAutoCompletionMethod("stats")]
        ProcessCommandResult StatsCmdAutoComplete(ProcessCommandContext context)
        {
            var collectables = this.GetStatsCollectables();

            var categories = new List<string>();
            foreach (IStatsCollectable collectable in collectables)
                collectable.GetCategories(categories);
            
            return this.commandManager.ProcessCommandAutoCompletion(
                context, categories.Distinct(StringComparer.OrdinalIgnoreCase));
        }
    }
}
