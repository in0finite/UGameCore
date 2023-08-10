using System;
using System.Collections;
using System.Linq;
using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore.UI
{
    public class SelectFolderDialogHandlerUI : MonoBehaviour, ISelectFolderDialogHandler
    {
        public GameObject selectFolderDialogPrefab;
        public Transform dialogParent;
        public SerializablePair<string, string>[] additionalFoldersInHeader = Array.Empty<SerializablePair<string, string>>();


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();
        }

        public IEnumerator SelectAsync(Ref<string> resultRef, string title, string folder, string defaultName)
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                resultRef.value = UnityEditor.EditorUtility.OpenFolderPanel(title, folder, defaultName);
#endif
                yield break;
            }

            GameObject go = this.selectFolderDialogPrefab.InstantiateAsUIElement(this.dialogParent);

            var folderDialog = go.GetComponentOrThrow<SelectFolderDialog>();
            folderDialog.initialFolder = folder;
            folderDialog.titleText.text = title;
            folderDialog.additionalFoldersInHeader = folderDialog.additionalFoldersInHeader.Concat(this.additionalFoldersInHeader).ToArray();
            
            string selectedFolder = null;
            folderDialog.onSelect.AddListener((str) => selectedFolder = str);

            while (folderDialog != null)
                yield return null;

            resultRef.value = selectedFolder;
        }

        public string Select(string title, string folder, string defaultName)
        {
            throw new NotSupportedException("Can not select folder synchronously");
        }
    }
}
