using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Utilities
{
	public class ButtonLayoutElement : MonoBehaviour, ILayoutElement
	{

		private	ILayoutElement	m_redirectedLayoutElement { get { return this.GetComponentInChildren<Text>(); } }
		public	int	extraWidth = 4;
		public	int	extraHeight = 4;


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
				return m_redirectedLayoutElement.preferredWidth + this.extraWidth;
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
				return m_redirectedLayoutElement.preferredHeight + this.extraHeight;
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

