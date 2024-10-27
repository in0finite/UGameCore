using System.Collections.Generic;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.UI
{
    /// <summary>
    /// Represents parent of <see cref="ContextMenuElement"/> objects.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class ContextMenuContainer : MonoBehaviour
    {
        [SerializeField] ContextMenuCurrentElementUI CurrentElement;
        [SerializeField] GameObject ElementEntryPrefab;

        public ContextMenuElement ActiveElement { get; private set; }
        public Button OriginalButton { get; private set; }

        readonly ComponentPoolList<Button> PooledEntriesButtons = new();
        readonly List<Button> CreatedEntriesButtons = new();

        bool ShouldCloseSoon = false;
        double TimeWhenClicked = double.NegativeInfinity;
        double TimeWhenOpened = double.NegativeInfinity;
        public float TimeToCloseAfterClicked = 0f;
        public float TolerationTimeAfterOpening = 0f;


        void Start()
        {
            this.EnsureSerializableReferencesAssigned();
            CurrentElement.EnsureSerializableReferencesAssigned();

            OriginalButton = ElementEntryPrefab.GetComponentOrThrow<Button>();

            PooledEntriesButtons.PrefabGameObject = ElementEntryPrefab;
            PooledEntriesButtons.ParentTransform = CurrentElement.ContentParent;

            CloseContextMenu();
        }

        void Update()
        {
            double timeNow = Time.unscaledTimeAsDouble;

            if (ShouldCloseSoon && timeNow - TimeWhenClicked >= TimeToCloseAfterClicked)
            {
                Debug.Log("close from update");
                CloseContextMenu();
            }

            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                // mouse clicked

                GameObject go = F.UIFocusedObject();
                if (go != null && (go == CurrentElement.gameObject || go.transform.IsChildOf(CurrentElement.transform)))
                    return;

                if (timeNow - TimeWhenOpened <= TolerationTimeAfterOpening)
                    return;

                ShouldCloseSoon = true;
                TimeWhenClicked = timeNow;
            }
        }

        void ReturnAllToPool()
        {
            CreatedEntriesButtons.RemoveDeadObjects();
            foreach (Button button in CreatedEntriesButtons)
            {
                if (button.TryGetComponent(out UIEventsPickup uIEventsPickup))
                    uIEventsPickup.ClearAllEvents(); // release references
            }
            PooledEntriesButtons.ReturnMultipleToPool(CreatedEntriesButtons);
            CreatedEntriesButtons.Clear();
        }

        public void OpenContextMenu(ContextMenuElement contextMenuElement)
        {
            CloseContextMenu();

            Debug.Log("OpenContextMenu");

            TimeWhenOpened = Time.unscaledTimeAsDouble;

            ActiveElement = contextMenuElement;

            foreach (ContextMenuElement.EntryData entry in contextMenuElement.Entries)
            {
                Button button = PooledEntriesButtons.Rent();
                CreatedEntriesButtons.Add(button);

                button.WithText(entry.Text);
                button.WithTextColor(entry.OverrideTextColor ? entry.TextColor : OriginalButton.GetTextColor());
                button.WithBackgroundColor(entry.OverrideBackgroundColor ? entry.BackgroundColor : OriginalButton.GetBackgroundColor());

                button.gameObject.GetOrAddComponent<UIEventsPickup>().onLeftPointerClick =
                    ev =>
                    {
                        Debug.Log("invoking for " + entry.Text);
                        CloseContextMenu();
                        entry.OnClicked?.Invoke();
                    };
                
                button.BringToFront();
            }

            // set position of ContextMenu
            Vector2 mousePos = Input.mousePosition.ToVec2XY();
            Vector2 screenSize = GUIUtils.ScreenSize;
            RectTransform rectTransform = CurrentElement.GetRectTransform();
            rectTransform.anchorMin = mousePos / screenSize;
            rectTransform.anchorMax = mousePos / screenSize;
            rectTransform.anchoredPosition = Vector2.zero;

            rectTransform.BringToFront();
            CurrentElement.gameObject.SetActive(true);
        }

        public void CloseContextMenu()
        {
            ShouldCloseSoon = false;

            CurrentElement.gameObject.SetActive(false);
            ReturnAllToPool(); // release references

            ActiveElement = null;
        }
    }
}
