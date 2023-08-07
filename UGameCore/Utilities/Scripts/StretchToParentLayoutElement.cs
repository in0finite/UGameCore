using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.Utilities
{
	/// <summary>
	/// Changes layout dimensions of current UI element to be proportional of dimensions of target element.
	/// </summary>
	public class StretchToParentLayoutElement : MonoBehaviour, ILayoutElement
	{
		public	float	width = 0.9f;
		public	float	height = 0.9f;
		public	RectTransform	stretchElement = null;
		public int priority = 1;


        public void CalculateLayoutInputHorizontal ()
		{
			
		}

		public void CalculateLayoutInputVertical ()
		{
			
		}

        public float minWidth => 0;

        public float preferredWidth => this.stretchElement.rect.width * this.width;

        public float flexibleWidth => -1;

        public float minHeight => 0;

        public float preferredHeight => this.stretchElement.rect.height * this.height;

        public float flexibleHeight => -1;

        public int layoutPriority => this.priority;

    }
}

