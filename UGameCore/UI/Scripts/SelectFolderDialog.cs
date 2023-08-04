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

        public Text currentFolderDisplayText;
        public Button goUpButton;

        public Button selectButton;
        public Button cancelButton;

        public RectTransform headerContainer;
        public RectTransform folderListContainer;

        public GameObject headerItemPrefab;
        public GameObject folderListItemPrefab;

        public bool destroyOnSelect = true;

        public Color selectedColor = Color.gray;
        public Color nonSelectedColor = Color.white;

        public string SelectedItemText { get; private set; }
        public Text SelectedTextComponent { get; private set; }
        public string CurrentFolder { get; private set; }

        public string initialFolder;


        private void Start()
        {
            this.goUpButton.onClick.AddListener(this.GoUp);
            this.selectButton.onClick.AddListener(this.OnSelectPressed);
            this.cancelButton.onClick.AddListener(this.OnCancelPressed);

            this.CurrentFolder = this.initialFolder;
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
            this.onSelect.Invoke(this.SelectedItemText);
        }

        void OnCancelPressed()
        {
            F.DestroyEvenInEditMode(this.gameObject);
            this.onSelect.Invoke(null);
        }

        public void AddToHeader(string item, string directory)
        {
            var go = this.headerItemPrefab.InstantiateAsUIElement(this.headerContainer);
            go.name = item;
            var text = go.GetComponentOrThrow<Text>();
            text.text = item;
            text.gameObject.GetOrAddComponent<UIEventsPickup>().onPointerClick += _ => this.ChangeCurrentFolder(directory);
        }

        public void AddToFolderList(string item)
        {
            var go = this.folderListItemPrefab.InstantiateAsUIElement(this.folderListContainer);
            go.name = item;
            var text = go.GetComponentOrThrow<Text>();
            text.text = item;
            text.gameObject.GetOrAddComponent<UIEventsPickup>().onPointerClick += _ => this.ChangeSelectedItem(text);
        }

        public void ChangeCurrentFolder(string folder)
        {
            if (folder == this.CurrentFolder)
                return;

            if (this.SelectedTextComponent != null)
                this.SelectedTextComponent.color = this.nonSelectedColor;

            this.CurrentFolder = folder;
            this.SelectedItemText = null;
            this.SelectedTextComponent = null;

            this.currentFolderDisplayText.text = folder;

            this.PopulateFolderList();
        }

        void ChangeSelectedItem(Text text)
        {
            if (this.SelectedTextComponent != null)
                this.SelectedTextComponent.color = this.nonSelectedColor;

            this.SelectedTextComponent = text;
            this.SelectedItemText = Path.Combine(this.CurrentFolder, text.text);
            text.color = this.selectedColor;
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

            var directories = FileBrowser.GetDirectoriesForTopPanel();

            foreach (var dir in directories)
            {
                this.AddToHeader(dir.Item1, dir.Item2);
            }
        }
    }
}
