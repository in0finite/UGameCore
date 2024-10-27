using System.Collections.Generic;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UGameCore.UI
{
    public class ContextMenuElement : MonoBehaviour, IPointerClickHandler
    {
        public ContextMenuContainer ContextMenuContainer { get; private set; }

        [System.Serializable]
        public struct EntryData
        {
            public string Text;
            public bool OverrideTextColor;
            public Color TextColor;
            public bool OverrideBackgroundColor;
            public Color BackgroundColor;
            public UnityEvent OnClicked;

            public EntryData(string text, UnityAction action)
                : this()
            {
                Text = text;
                OnClicked = new UnityEvent();
                OnClicked.AddListener(action);
            }
        }

        public List<EntryData> Entries = new();

        public bool ShowOnLeftClick = false;
        public bool ShowOnRightClick = true;


        void Awake()
        {
            ContextMenuContainer = this.gameObject.GetComponentInParentOrThrow<ContextMenuContainer>();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            bool isLeftPointer = eventData.button == PointerEventData.InputButton.Left;
            bool isRightPointer = eventData.button == PointerEventData.InputButton.Right;

            bool bShow = ShowOnLeftClick && isLeftPointer || ShowOnRightClick && isRightPointer;

            if (!bShow)
                return;

            Debug.Log("open from element");
            this.ContextMenuContainer.OpenContextMenu(this);
        }
    }
}
