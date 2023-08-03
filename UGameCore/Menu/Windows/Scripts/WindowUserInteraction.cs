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

        public IEnumerator ConfirmAsync(Ref<bool> bResultRef, string title, string message, string ok, string cancel)
        {
            throw new System.NotSupportedException("Confirm not supported");
        }

        public void ShowMessage(string title, string message)
        {
            WindowManager.OpenMessageBox(title, message);
        }

        public IEnumerator ShowMessageAsync(string title, string message)
        {
            var window = WindowManager.OpenMessageBox(title, message);
            while (window != null || !window.isClosed)
                yield return null;
        }
    }
}
