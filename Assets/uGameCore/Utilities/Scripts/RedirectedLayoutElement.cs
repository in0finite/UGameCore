using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Utilities
{
	public class RedirectedLayoutElement : MonoBehaviour, ILayoutElement
	{

		public	GameObject	layoutObject = null;
		private	ILayoutElement	m_redirectedLayoutElement = null;


		void Awake() {
			m_redirectedLayoutElement = layoutObject.GetComponent<ILayoutElement> ();
		}


		public void CalculateLayoutInputHorizontal ()
		{
			m_redirectedLayoutElement.CalculateLayoutInputHorizontal ();
		}

		public void CalculateLayoutInputVertical ()
		{
			m_redirectedLayoutElement.CalculateLayoutInputVertical ();
		}

		public float minWidth {
			get {
				return m_redirectedLayoutElement.minWidth;
			}
		}

		public float preferredWidth {
			get {
				return m_redirectedLayoutElement.preferredWidth;
			}
		}

		public float flexibleWidth {
			get {
				return m_redirectedLayoutElement.flexibleWidth;
			}
		}

		public float minHeight {
			get {
				return m_redirectedLayoutElement.minHeight;
			}
		}

		public float preferredHeight {
			get {
				return m_redirectedLayoutElement.preferredHeight;
			}
		}

		public float flexibleHeight {
			get {
				return m_redirectedLayoutElement.flexibleHeight;
			}
		}

		public int layoutPriority {
			get {
				return m_redirectedLayoutElement.layoutPriority;
			}
		}

	}
}

