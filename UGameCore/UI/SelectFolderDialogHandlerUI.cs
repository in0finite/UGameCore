using System;
using System.Collections;
using System.Linq;
using UGameCore.Menu.Windows;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore.UI
{
    public class SelectFolderDialogHandlerUI : MonoBehaviour, ISelectFolderDialogHandler
    {
        public GameObject selectFolderDialogPrefab;
        WindowManager m_windowManager;
        public SerializablePair<string, string>[] additionalFoldersInHeader = Array.Empty<SerializablePair<string, string>>();


        void Awake()
        {
            this.EnsureSerializableReferencesAssigned();
            var provider = this.GetSingleComponentOrThrow<IServiceProvider>();
            m_windowManager = provider.GetRequiredService<WindowManager>();
        }

        public string Select(string title, string folder, string defaultName)
        {
            var resultRef = new Ref<string>();
            SelectAsync(resultRef, title, folder, defaultName).EnumerateToEnd();
            return resultRef.value;
        }

        public IEnumerator SelectAsync(Ref<string> resultRef, string title, string folder, string defaultName)
        {
            GameObject go = this.selectFolderDialogPrefab.InstantiateAsUIElement(m_windowManager.windowsCanvas.transform);

            var folderDialog = go.GetComponentOrThrow<SelectFolderDialog>();
            folderDialog.initialFolder = folder;
            folderDialog.titleText.text = title;
            folderDialog.additionalFoldersInHeader = folderDialog.additionalFoldersInHeader.Concat(this.additionalFoldersInHeader).ToArray();
            
            string selectedFolder = null;
            folderDialog.onSelect.AddListener((str) => selectedFolder = str);

            var window = go.GetOrAddComponent<Window>(); // add Window functionality to folder picker

            while (window != null)
                yield return null;

            resultRef.value = selectedFolder;
        }
    }
}
