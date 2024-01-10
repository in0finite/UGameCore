using System;
using System.Collections;
using System.Linq;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore.UI
{
    public class SelectFolderDialogHandlerUI : MonoBehaviour, ISelectFolderDialog, ISelectFileDialog
    {
        public GameObject selectFolderDialogPrefab;
        public Transform dialogParent;
        public SerializablePair<string, string>[] additionalFoldersInHeader = Array.Empty<SerializablePair<string, string>>();


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();
        }

        IEnumerator ISelectFolderDialog.SelectAsync(
            Ref<string> resultRef, string title, string folder, string defaultName)
        {
            return this.ShowDialogAsync(resultRef, title, folder, defaultName, null, false);
        }

        IEnumerator ISelectFileDialog.SelectAsync(
            Ref<string> resultRef, string title, string directory, string extension)
        {
            return this.ShowDialogAsync(resultRef, title, directory, null, extension, true);
        }

        IEnumerator ShowDialogAsync(
            Ref<string> resultRef, string title, string folder, string defaultName, string extension, bool isFile)
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                if (isFile)
                    resultRef.value = UnityEditor.EditorUtility.OpenFilePanel(title, folder, extension);
                else
                    resultRef.value = UnityEditor.EditorUtility.OpenFolderPanel(title, folder, defaultName);
#endif
                yield break;
            }

            GameObject go = this.selectFolderDialogPrefab.InstantiateAsUIElement(this.dialogParent);

            var dialog = go.GetComponentOrThrow<SelectFolderDialog>();
            dialog.initialFolder = folder;
            dialog.titleText.text = title;
            dialog.additionalFoldersInHeader = dialog.additionalFoldersInHeader.Concat(this.additionalFoldersInHeader).ToArray();
            dialog.allowFiles = isFile;
            dialog.allowFolders = !isFile;
            if (!extension.IsNullOrWhiteSpace())
                dialog.fileSearchPattern = "*." + extension;

            string selectedItem = null;
            if (isFile)
                dialog.onFileSelect.AddListener((str) => selectedItem = str);
            else
                dialog.onFolderSelect.AddListener((str) => selectedItem = str);

            while (dialog != null)
                yield return null;

            resultRef.value = selectedItem;
        }
    }
}
