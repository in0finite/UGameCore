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

        public FolderSelectedEvent onSelect = new FolderSelectedEvent();

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

        public string SelectedItemText { get; private set; }
        public Text SelectedTextComponent { get; private set; }
        public string CurrentFolder { get; private set; }

        public string initialFolder;

        public SerializablePair<string, string>[] additionalFoldersInHeader = Array.Empty<SerializablePair<string, string>>();


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
            if (this.destroyOnSelect)
                F.DestroyEvenInEditMode(this.gameObject);
            this.onSelect.Invoke(this.CurrentFolder);
        }

        void OnCancelPressed()
        {
            F.DestroyEvenInEditMode(this.gameObject);
            this.onSelect.Invoke(null);
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

        public void AddToFolderList(string item)
        {
            var go = this.folderListItemPrefab.InstantiateAsUIElement(this.folderListContainer);
            go.name = item;
            var text = go.GetComponentInChildrenOrThrow<Text>();
            text.text = item;
            text.gameObject.GetOrAddComponent<UIEventsPickup>().onPointerClick += _ => this.OnItemClicked(text);
        }

        public void ChangeCurrentFolder(string folder)
        {
            if (folder == this.CurrentFolder)
                return;

            if (this.SelectedTextComponent != null)
                this.SelectedTextComponent.color = m_nonSelectedColor;

            this.CurrentFolder = folder;
            this.SelectedItemText = null;
            this.SelectedTextComponent = null;

            this.currentFolderInputField.text = folder;

            this.PopulateFolderList();
        }

        void ChangeSelectedItem(Text text)
        {
            if (this.SelectedTextComponent != null)
                this.SelectedTextComponent.color = m_nonSelectedColor;

            this.SelectedTextComponent = text;
            this.SelectedItemText = Path.Combine(this.CurrentFolder, text.text);
            text.color = this.selectedColor;
        }

        void OnItemClicked(Text text)
        {
            string newFolder = Path.Combine(this.CurrentFolder, text.text);
            this.ChangeCurrentFolder(newFolder);
        }

        void PopulateFolderList()
        {
            this.folderListContainer.DestroyChildren();
            
            if (!Directory.Exists(this.CurrentFolder))
                return;

            var subFolders = Directory.EnumerateDirectories(this.CurrentFolder);
            foreach (string subFolder in subFolders)
            {
                this.AddToFolderList(Path.GetFileName(subFolder));
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
