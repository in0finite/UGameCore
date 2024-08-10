using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UGameCore.Console
{
    public class ConsoleLogEntryComponent : MonoBehaviour
    {
        public TextMeshProUGUI textComponent;
        public Image image;
        public UIEventsPickup eventsPickup;

        public Console.LogMessage LogMessage { get; internal set; }
    }
}
