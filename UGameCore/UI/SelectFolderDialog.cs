using System;
using System.IO;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UGameCore.UI
{
    public class SelectFolderDialog : MonoBehaviour
    {
        [Serializable]
        public class FolderSelectedEvent : UnityEvent<string> { }
        [Serializable]
        public class FileSelectedEvent : UnityEvent<string> { }

        public FolderSelectedEvent onFolderSelect = new();
        public FileSelectedEvent onFileSelect = new();

        public Text titleText;

        public InputField currentFolderInputField;
        public Button goUpButton;

        public Button selectButton;
        public Button cancelButton;

        public RectTransform headerContainer;
        public RectTransform folderListContainer;

        public GameObject headerItemPrefab;
        public GameObject folderListItemPrefab;

        public bool destroyOnSelect = true;

        public Color selectedColor;
        Color m_nonSelectedColor;

        public bool allowFiles = false;
        public bool allowFolders = true;

        public bool IsFileSelected { get; private set; } = false;
        public bool IsFolderSelected { get; private set; } = false;
        public Text SelectedTextComponent { get; private set; }

        public string CurrentFolder { get; private set; }

        public string initialFolder;

        public SerializablePair<string, string>[] additionalFoldersInHeader = Array.Empty<SerializablePair<string, string>>();

        public string folderSearchPattern = "*";
        public string fileSearchPattern = "*";


        private void Start()
        {
            this.EnsureSerializableReferencesAssigned();

            m_nonSelectedColor = this.folderListItemPrefab.GetComponentInChildrenOrThrow<Text>().color;

            this.goUpButton.onClick.AddListener(this.GoUp);
            this.selectButton.onClick.AddListener(this.OnSelectPressed);
            this.cancelButton.onClick.AddListener(this.OnCancelPressed);
            this.currentFolderInputField.onSubmit.AddListener(this.OnInputFieldSubmit);

            this.CurrentFolder = this.initialFolder;
            if (this.CurrentFolder.IsNullOrWhiteSpace())
                this.CurrentFolder = Directory.GetCurrentDirectory();

            this.currentFolderInputField.text = this.CurrentFolder;
            this.PopulateFolderList();

            this.PopulateHeader();
        }

        public void GoUp()
        {
            if (string.IsNullOrWhiteSpace(this.CurrentFolder))
                return;

            string newFolder = Path.GetDirectoryName(this.CurrentFolder);

            this.ChangeCurrentFolder(newFolder);
        }

        void OnSelectPressed()
        {
            if (this.IsFileSelected && this.allowFiles)
            {
                string fileName = this.SelectedTextComponent.text;

                if (this.destroyOnSelect)
                    F.DestroyEvenInEditMode(this.gameObject);

                this.onFileSelect.Invoke(Path.Combine(this.CurrentFolder, fileName));
            }
            else if (this.allowFolders)
            {
                if (this.destroyOnSelect)
                    F.DestroyEvenInEditMode(this.gameObject);

                this.onFolderSelect.Invoke(this.CurrentFolder);
            }
        }

        void OnCancelPressed()
        {
            F.DestroyEvenInEditMode(this.gameObject);
            if (this.allowFolders)
                this.onFolderSelect.Invoke(null);
            if (this.allowFiles)
                this.onFileSelect.Invoke(null);
        }

        void OnInputFieldSubmit(string text)
        {
            this.ChangeCurrentFolder(text);
        }

        public void AddToHeader(string item, string directory)
        {
            var go = this.headerItemPrefab.InstantiateAsUIElement(this.headerContainer);
            go.name = item;
            var text = go.GetComponentInChildrenOrThrow<Text>();
            text.text = item;
            text.gameObject.GetOrAddComponent<UIEventsPickup>().onPointerClick += _ => this.ChangeCurrentFolder(directory);
        }

        public Text AddToListView(
            string item,
            Action<Text> onSingleClick,
            Action<Text> onDoubleClick)
        {
            var go = this.folderListItemPrefab.InstantiateAsUIElement(this.folderListContainer);
            go.name = item;
            var text = go.GetComponentInChildrenOrThrow<Text>();
            text.text = item;
            var pickup = text.gameObject.GetOrAddComponent<UIEventsPickup>();
            pickup.onPointerDoubleClick += ev => onDoubleClick(text);
            pickup.onPointerClick += ev => onSingleClick(text);
            return text;
        }

        public void ChangeCurrentFolder(string folder)
        {
            if (folder == this.CurrentFolder)
                return;

            if (this.SelectedTextComponent != null)
                this.SelectedTextComponent.color = m_nonSelectedColor;

            this.CurrentFolder = folder;

            this.IsFileSelected = false;
            this.IsFolderSelected = false;
            this.SelectedTextComponent = null;

            this.currentFolderInputField.text = folder;

            this.PopulateFolderList();
        }

        void ChangeSelectedItem(Text textComponent, bool isFile)
        {
            if (this.SelectedTextComponent != null)
                this.SelectedTextComponent.color = m_nonSelectedColor;

            this.IsFileSelected = isFile;
            this.IsFolderSelected = !isFile;
            this.SelectedTextComponent = textComponent;
            
            textComponent.color = this.selectedColor;
        }

        void OnFolderDoubleClicked(string folder)
        {
            string newFolder = Path.Combine(this.CurrentFolder, folder);
            this.ChangeCurrentFolder(newFolder);
        }

        void OnFileDoubleClicked(Text text)
        {
            this.IsFileSelected = true;
            this.IsFolderSelected = false;
            this.SelectedTextComponent = text;
            this.OnSelectPressed();
        }

        void PopulateFolderList()
        {
            this.folderListContainer.DestroyChildren();
            
            if (!Directory.Exists(this.CurrentFolder))
                return;

            var subFolders = Directory.EnumerateDirectories(this.CurrentFolder, this.folderSearchPattern);
            foreach (string subFolderPath in subFolders)
            {
                string subFolder = Path.GetFileName(subFolderPath);
                this.AddToListView(subFolder, t => this.ChangeSelectedItem(t, false), t => this.OnFolderDoubleClicked(subFolder));
            }

            if (this.allowFiles)
            {
                var files = Directory.EnumerateFiles(this.CurrentFolder, this.fileSearchPattern);
                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);
                    this.AddToListView(fileName, t => this.ChangeSelectedItem(t, true), t => this.OnFileDoubleClicked(t));
                }
            }
        }

        void PopulateHeader()
        {
            this.headerContainer.DestroyChildren();

            foreach (var additionalHeaderFolder in this.additionalFoldersInHeader)
                this.AddToHeader(additionalHeaderFolder.item1, additionalHeaderFolder.item2);

            var directories = FileBrowser.GetDirectoriesForTopPanel();

            foreach (var dir in directories)
                this.AddToHeader(dir.Item1, dir.Item2);
        }
    }
}
