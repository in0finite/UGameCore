using System;
using System.Collections;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore.UI.Windows
{
    /// <summary>
    /// Implements user interaction through windows (message boxes).
    /// </summary>
    public class WindowUserInteraction : MonoBehaviour, IUserInteraction
    {
        EditorDialogUserInteraction m_editorDialogUserInteraction = new EditorDialogUserInteraction();

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

            MessageBoxConfirmation msgBox = WindowManager.OpenMessageBoxConfirm(title, message);

            msgBox.OKButtonText = ok;
            msgBox.messageBox.CloseButtonText = cancel;

            bool confirmResult = false;
            msgBox.okButton.onClick.AddListener(() => confirmResult = true);

            while (msgBox != null)
                yield return null;

            bResultRef.value = confirmResult;
        }

        public IEnumerator ShowMessageAsync(string title, string message)
        {
            if (!Application.isPlaying)
            {
                yield return m_editorDialogUserInteraction.ShowMessageAsync(title, message);
                yield break;
            }

            var window = WindowManager.OpenMessageBox(title, message);
            while (window != null)
                yield return null;
        }

        public void ShowMessage(string title, string message)
        {
            if (!Application.isPlaying)
            {
                m_editorDialogUserInteraction.ShowMessage(title, message);
                return;
            }

            WindowManager.OpenMessageBox(title, message);
        }
    }
}
