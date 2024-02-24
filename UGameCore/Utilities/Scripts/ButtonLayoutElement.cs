using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Makes <see cref="Button"/> a <see cref="ILayoutElement"/> by retrieving layout properties from it's <see cref="Text"/> component.
    /// </summary>
    public class ButtonLayoutElement : MonoBehaviour, ILayoutElement
	{

        private ILayoutElement m_redirectedLayoutElement => this.GetComponentInChildren<Text>();
        public	int	extraWidth = 4;
		public	int	extraHeight = 4;

		public bool overridePriority = false;
		public int priority = 1;


        public void CalculateLayoutInputHorizontal ()
		{
			m_redirectedLayoutElement.CalculateLayoutInputHorizontal ();
		}

		public void CalculateLayoutInputVertical ()
		{
			m_redirectedLayoutElement.CalculateLayoutInputVertical ();
		}

        public float minWidth => m_redirectedLayoutElement.minWidth;

        public float preferredWidth => m_redirectedLayoutElement.preferredWidth + this.extraWidth;

        public float flexibleWidth => m_redirectedLayoutElement.flexibleWidth;

        public float minHeight => m_redirectedLayoutElement.minHeight;

        public float preferredHeight => m_redirectedLayoutElement.preferredHeight + this.extraHeight;

        public float flexibleHeight => m_redirectedLayoutElement.flexibleHeight;

        public int layoutPriority => overridePriority ? priority : m_redirectedLayoutElement.layoutPriority;

    }
}

