﻿using UnityEngine;

namespace UGameCore.Utilities {

	public class MaintainRectTransformPosition : MonoBehaviour
	{
		public	bool	setX = false;
		public	bool	setY = false;

		public	Vector2	offsetPosition = Vector2.zero ;

	//	private	Vector3	m_position = Vector3.zero ;

		private	RectTransform	m_rectTransform = null;


		void Awake() {
			m_rectTransform = GetComponent<RectTransform> ();
		//	if (m_rectTransform != null)
		//		m_position = m_rectTransform.localPosition;
		}

		void Update() {

			if (null == m_rectTransform)
				return;

			Vector2 newMin = m_rectTransform.offsetMin;
			Vector2 newMax = m_rectTransform.offsetMax;

			if (this.setX) {
			//	newPos.x = this.m_position.x;
				newMin.x = this.offsetPosition.x;
				newMax.x = this.offsetPosition.x;
			}

			if (this.setY) {
			//	newPos.y = this.m_position.y;
				newMin.y = this.offsetPosition.y;
				newMax.y = this.offsetPosition.y;
			}

			if (this.setX || this.setY) {
				m_rectTransform.offsetMin = newMin;
				m_rectTransform.offsetMax = newMax;
			}

		}

	}
}

