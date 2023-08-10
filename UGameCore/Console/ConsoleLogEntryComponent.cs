using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Console
{
    public class ConsoleLogEntryComponent : MonoBehaviour
    {
        public Text textComponent;
        public Image image;
        public UIEventsPickup eventsPickup;

        public Console.LogMessage LogMessage { get; internal set; }
    }
}
