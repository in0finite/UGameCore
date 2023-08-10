using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.UI.Windows
{
    public class MessageBox : MonoBehaviour
    {
        public Button closeButton;

        public string CloseButtonText
        {
            get => this.closeButton.GetComponentInChildrenOrThrow<Text>().text;
            set => this.closeButton.GetComponentInChildrenOrThrow<Text>().text = value;
        }

        private void Start()
        {
            this.EnsureSerializableReferencesAssigned();
        }
    }
}
