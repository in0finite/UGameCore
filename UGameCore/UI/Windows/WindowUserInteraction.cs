using System;
using System.Collections;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore.Menu.Windows
{
    /// <summary>
    /// Implements user interaction through windows (message boxes).
    /// </summary>
    public class WindowUserInteraction : MonoBehaviour, IUserInteraction
    {
        public bool SupportsConfirm => false;


        void Awake()
        {
            var provider = this.gameObject.GetSingleComponentOrThrow<IServiceProvider>();
            provider.GetRequiredService<WindowManager>(); // make sure it exists
        }

        public IEnumerator ConfirmAsync(Ref<bool> bResultRef, string title, string message, string ok, string cancel)
        {
            MessageBoxConfirmation msgBox = WindowManager.OpenMessageBoxConfirm(title, message);

            msgBox.OKButtonText = ok;
            msgBox.messageBox.CloseButtonText = cancel;

            bool confirmResult = false;
            msgBox.okButton.onClick.AddListener(() => confirmResult = true);

            while (msgBox != null)
                yield return null;

            bResultRef.value = confirmResult;
        }

        public void ShowMessage(string title, string message)
        {
            WindowManager.OpenMessageBox(title, message);
        }

        public IEnumerator ShowMessageAsync(string title, string message)
        {
            var window = WindowManager.OpenMessageBox(title, message);
            while (window != null)
                yield return null;
        }
    }
}
