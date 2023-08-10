using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.UI.Windows
{
    public class MessageBoxConfirmation : MonoBehaviour
    {
        public MessageBox messageBox;
        public Button okButton;

        public string OKButtonText
        { 
            get => this.okButton.GetComponentInChildrenOrThrow<Text>().text;
            set => this.okButton.GetComponentInChildrenOrThrow<Text>().text = value;
        }


        private void Start()
        {
            this.EnsureSerializableReferencesAssigned();
        }
    }
}
