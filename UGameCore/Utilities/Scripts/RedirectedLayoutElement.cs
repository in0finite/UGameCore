using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Utilities
{
	public class RedirectedLayoutElement : MonoBehaviour, ILayoutElement
	{

		public	GameObject	layoutObject = null;
		private	ILayoutElement	m_redirectedLayoutElement => this.layoutObject != null ? this.layoutObject.GetComponent<ILayoutElement>() : null;



		public void CalculateLayoutInputHorizontal ()
		{
			m_redirectedLayoutElement?.CalculateLayoutInputHorizontal ();
		}

		public void CalculateLayoutInputVertical ()
		{
			m_redirectedLayoutElement?.CalculateLayoutInputVertical ();
		}

        public float minWidth => m_redirectedLayoutElement?.minWidth ?? 0f;

        public float preferredWidth => m_redirectedLayoutElement?.preferredWidth ?? 0f;

        public float flexibleWidth => m_redirectedLayoutElement?.flexibleWidth ?? 0f;

        public float minHeight => m_redirectedLayoutElement?.minHeight ?? 0f;

        public float preferredHeight => m_redirectedLayoutElement?.preferredHeight ?? 0f;

        public float flexibleHeight => m_redirectedLayoutElement?.flexibleHeight ?? 0f;

        public int layoutPriority => m_redirectedLayoutElement?.layoutPriority ?? 0;

    }
}

