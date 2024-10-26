using System;
using System.Collections;
using System.Collections.Generic;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore.UI.Windows
{
    /// <summary>
    /// Implements user interaction through windows (message boxes).
    /// </summary>
    public class WindowUserInteraction : MonoBehaviour, IUserInteraction
    {
        readonly EditorDialogUserInteraction m_editorDialogUserInteraction = new();

        readonly Dictionary<string, Window> OpenedWindowsPerTitle = new(StringComparer.Ordinal);
        readonly Dictionary<string, MessageBoxConfirmation> OpenedConfirmationWindowsPerTitle = new(StringComparer.Ordinal);

        public bool SupportsConfirm => true;


        void Awake()
        {
            var provider = this.gameObject.GetSingleComponentOrThrow<IServiceProvider>();
            provider.GetRequiredService<WindowManager>(); // make sure it exists
        }

        public IEnumerator ConfirmAsync(Ref<bool> bResultRef, string title, string message, string ok, string cancel)
        {
            if (!Application.isPlaying)
            {
                yield return m_editorDialogUserInteraction.ConfirmAsync(bResultRef, title, message, ok, cancel);
                yield break;
            }

            MessageBoxConfirmation msgBox = GetExistingOrCreateNewConfirmationWindow(title, message, ok, cancel);

            bool confirmResult = false;
            msgBox.okButton.onClick.AddListener(() => confirmResult = true);

            while (msgBox != null)
                yield return null;

            RemoveIfDead(title);

            bResultRef.value = confirmResult;
        }

        public IEnumerator ShowMessageAsync(string title, string message)
        {
            if (!Application.isPlaying)
            {
                yield return m_editorDialogUserInteraction.ShowMessageAsync(title, message);
                yield break;
            }

            Window window = GetExistingOrCreateNewWindow(title, message);

            while (window != null)
                yield return null;

            RemoveIfDead(title);
        }

        public void ShowMessage(string title, string message)
        {
            if (!Application.isPlaying)
            {
                m_editorDialogUserInteraction.ShowMessage(title, message);
                return;
            }

            GetExistingOrCreateNewWindow(title, message);
        }

        Window GetExistingOrCreateNewWindow(string title, string message)
        {
            RemoveIfDead(title);

            if (OpenedWindowsPerTitle.TryGetValue(title, out Window window))
            {
                window.BringToTop();
                window.SetContentText(message);
                return window;
            }

            window = WindowManager.OpenMessageBox(title, message);
            OpenedWindowsPerTitle.Add(title, window);
            return window;
        }

        MessageBoxConfirmation GetExistingOrCreateNewConfirmationWindow(
            string title, string message, string ok, string cancel)
        {
            RemoveIfDead(title);

            if (!OpenedConfirmationWindowsPerTitle.TryGetValue(title, out MessageBoxConfirmation msgBox))
            {
                msgBox = WindowManager.OpenMessageBoxConfirm(title, message);
                OpenedConfirmationWindowsPerTitle.Add(title, msgBox);
            }

            msgBox.messageBox.window.BringToTop();

            msgBox.OKButtonText = ok;
            msgBox.messageBox.CloseButtonText = cancel;
            msgBox.messageBox.window.SetContentText(message);

            return msgBox;
        }

        static void RemoveIfDead<T>(Dictionary<string, T> dict, string title)
            where T : Component
        {
            if (dict.TryGetValue(title, out T component) && component == null)
                dict.Remove(title);
        }

        void RemoveIfDead(string title)
        {
            RemoveIfDead(OpenedWindowsPerTitle, title);
            RemoveIfDead(OpenedConfirmationWindowsPerTitle, title);
        }
    }
}
