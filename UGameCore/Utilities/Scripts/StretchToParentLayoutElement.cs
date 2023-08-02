using UnityEngine;
using UnityEngine.UI;

namespace uGameCore.Utilities
{
	public class StretchToParentLayoutElement : MonoBehaviour, ILayoutElement
	{

	//	private	RectTransform	m_parent { get { return this.transform.parent as RectTransform; } }
		public	float	width = 0.9f;
		public	float	height = 0.9f;
		public	RectTransform	stretchElement = null;



		public void CalculateLayoutInputHorizontal ()
		{
			
		}

		public void CalculateLayoutInputVertical ()
		{
			
		}

		public float minWidth {
			get {
				return 0;
			}
		}

		public float preferredWidth {
			get {
				return this.stretchElement.rect.width * this.width;
			}
		}

		public float flexibleWidth {
			get {
				return -1;
			}
		}

		public float minHeight {
			get {
				return 0;
			}
		}

		public float preferredHeight {
			get {
				return this.stretchElement.rect.height * this.height;
			}
		}

		public float flexibleHeight {
			get {
				return -1;
			}
		}

		public int layoutPriority {
			get {
				return 0;
			}
		}

	}
}

