using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Menu
{
    public class ConsoleLogEntryComponent : MonoBehaviour
    {
        public Text textComponent;

        public Console.LogMessage LogMessage { get; internal set; }
    }
}
