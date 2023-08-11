using System;
using System.Linq;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using static UGameCore.CommandManager;

namespace UGameCore
{
    public class ShaderCommands : MonoBehaviour
    {
        public CommandManager commandManager;


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        [CommandMethod("shader_global_keywords", "Displays all global shader keywords")]
        ProcessCommandResult GlobalKeywordsCmd(ProcessCommandContext context)
        {
            var keywords = Shader.enabledGlobalKeywords;

            string response = $"Enabled global shader keywords [{keywords.Length}]:\n";

            foreach (GlobalKeyword keyword in keywords)
            {
                response += keyword.name + "\n";
            }

            return ProcessCommandResult.SuccessResponse(response);
        }

        [CommandMethod("shader_global_float", "Read global shader float")]
        ProcessCommandResult GlobalFloatCmd(ProcessCommandContext context)
        {
            return ProcessCommandResult.SuccessResponse(Shader.GetGlobalFloat(context.ReadString()).ToString());
        }

        [CommandMethod("shader_global_vector", "Read global shader vector")]
        ProcessCommandResult GlobalVectorCmd(ProcessCommandContext context)
        {
            return ProcessCommandResult.SuccessResponse(Shader.GetGlobalVector(context.ReadString()).ToString());
        }

        [CommandMethod("shader_global_int", "Read global shader int")]
        ProcessCommandResult GlobalIntCmd(ProcessCommandContext context)
        {
            return ProcessCommandResult.SuccessResponse(Shader.GetGlobalFloat(context.ReadString()).ToString());
        }

        [CommandMethod("shader_global_float_array", "Read global shader float array")]
        ProcessCommandResult GlobalFloatArrayCmd(ProcessCommandContext context)
        {
            float[] floats = Shader.GetGlobalFloatArray(context.ReadString()) ?? Array.Empty<float>();
            string response = $"[{floats.Length}]: {string.Join(", ", floats.Take(100))}";
            return ProcessCommandResult.SuccessResponse(response);
        }

        [CommandMethod("shader_global_vector_array", "Read global shader vector array")]
        ProcessCommandResult GlobalVectorArrayCmd(ProcessCommandContext context)
        {
            Vector4[] vectors = Shader.GetGlobalVectorArray(context.ReadString()) ?? Array.Empty<Vector4>();
            string response = $"[{vectors.Length}]: {string.Join(", ", vectors.Take(50))}";
            return ProcessCommandResult.SuccessResponse(response);
        }
    }
}
