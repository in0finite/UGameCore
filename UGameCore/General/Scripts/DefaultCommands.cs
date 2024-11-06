using System.Diagnostics;
using System;
using UnityEngine;
using static UGameCore.CommandManager;
using UGameCore.Utilities;
using System.IO;

namespace UGameCore
{
    public class DefaultCommands : MonoBehaviour
    {
		public CommandManager commandManager;


        void Start()
        {
            this.commandManager.RegisterCommandsFromTypeMethods(this);
        }

        [CommandMethod("exit", "Exits the application", commandAliases = new string[] { "quit" })]
        ProcessCommandResult ExitCmd(ProcessCommandContext context)
        {
            Application.Quit();
            return ProcessCommandResult.Success;
        }

        [CommandMethod("echo", "Prints text to Console")]
        ProcessCommandResult EchoCmd(ProcessCommandContext context)
        {
            UnityEngine.Debug.Log(context.GetRestOfTheCommand());
            return ProcessCommandResult.Success;
        }

        [CommandMethod("uptime", "Returns uptime of application")]
        ProcessCommandResult UptimeCmd(ProcessCommandContext context)
        {
            return ProcessCommandResult.SuccessResponse(TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble).ToString());
        }

        [CommandMethod("date", "Returns current system date")]
        ProcessCommandResult DateCmd(ProcessCommandContext context)
        {
            return ProcessCommandResult.SuccessResponse(
                DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        [CommandMethod("date_utc", "Returns current UTC date")]
        ProcessCommandResult DateUtcCmd(ProcessCommandContext context)
        {
            return ProcessCommandResult.SuccessResponse(
                DateTime.UtcNow.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        [CommandMethod("gc_collect", "Collect garbage")]
        ProcessCommandResult GCCollectCmd(ProcessCommandContext context)
        {
            var sw = Stopwatch.StartNew();
            double memBefore = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            GC.Collect();
            double memAfter = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            return ProcessCommandResult.SuccessResponse($"Before: {memBefore:F} MB, after: {memAfter:F} MB, elapsed: {sw.Elapsed.TotalMilliseconds:F} ms");
        }

        [CommandMethod("unload_unused_assets", "Unload unused assets")]
        ProcessCommandResult UnloadUnusedAssetsCmd(ProcessCommandContext context)
        {
            var operation = Resources.UnloadUnusedAssets();
            operation.completed += (op) => UnityEngine.Debug.Log("Assets unload complete");
            return ProcessCommandResult.Success;
        }

        [CommandMethod("log_file_open", "Open log file")]
        ProcessCommandResult LogFileOpenCmd(ProcessCommandContext context)
        {
            string filePath = F.GetLogFilePath();
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            Application.OpenURL(filePath);

            return ProcessCommandResult.Success;
        }

        [CommandMethod("camera_main_enable", "Enable main camera")]
        ProcessCommandResult CameraEnableCmd(ProcessCommandContext context)
        {
            return CameraEnableDisableCmd(context, true);
        }

        [CommandMethod("camera_main_disable", "Disable main camera")]
        ProcessCommandResult CameraDisableCmd(ProcessCommandContext context)
        {
            return CameraEnableDisableCmd(context, false);
        }

        ProcessCommandResult CameraEnableDisableCmd(ProcessCommandContext context, bool bEnable)
        {
            Camera cam = F.FindMainCameraEvenIfDisabled();
            if (cam == null)
                throw new InvalidOperationException("Failed to find main camera");

            cam.enabled = bEnable;

            return ProcessCommandResult.Success;
        }
    }
}
