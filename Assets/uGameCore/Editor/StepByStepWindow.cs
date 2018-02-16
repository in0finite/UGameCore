using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace uGameCore.Editor {

	using Utilities2 = uGameCore.Utilities.Utilities ;


	public class StepByStepWindow : EditorWindow {

		public class StepInfo
		{
			public	int nextIndex = -1;
			public	int previousIndex = -1;
			public	bool	allowsNext = true ;
			public	bool	allowsPrevious = true ;
			public	bool	canSkip = false ;
			public	Action	onGUI = null;
			public	string	title = "";
			public	Vector2	scrollViewPos = Vector2.zero;

			public	StepInfo() { }
			public	StepInfo( string title, Action onGUI ) { this.title = title; this.onGUI = onGUI; }
		}

		protected	StepInfo[]	m_steps = new StepInfo[0];
		private	int	m_currentStepIndex = 0;

		protected	GUIStyle	m_centeredLabelStyle = null;



		public StepByStepWindow() {


		//	this.minSize = new Vector2 (400, 300);

		}


		void OnGUI() {

			// create style for centered label
			if (null == m_centeredLabelStyle) {
				m_centeredLabelStyle = new GUIStyle (GUI.skin.label);
				m_centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
			}


			if (m_currentStepIndex < 0)
				return;


			var currentStep = m_steps [m_currentStepIndex];


			// display title
			GUILayout.Space (5);
			GUILayout.Label (currentStep.title, m_centeredLabelStyle);


			GUILayout.Space(40);

			currentStep.scrollViewPos = EditorGUILayout.BeginScrollView (currentStep.scrollViewPos,
				GUILayout.MaxWidth( this.position.width - 15 ) );

			currentStep.onGUI ();

			EditorGUILayout.EndScrollView ();


			// draw footer => previous, next buttons

			GUILayout.Space(60);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();

			if (currentStep.previousIndex >= 0) {
				if (Utilities2.DisabledButton (currentStep.allowsPrevious, "Back")) {
					m_currentStepIndex = currentStep.previousIndex;
				}
			}

			if (currentStep.canSkip) {
				if (GUILayout.Button ("Skip")) {
					m_currentStepIndex = currentStep.nextIndex;
				}
			}
			
			if (currentStep.nextIndex < 0) {
				// last step
				if (Utilities2.DisabledButton (currentStep.allowsNext, "Finish")) {
					this.Close ();
				}
			} else {
				if (Utilities2.DisabledButton (currentStep.allowsNext, "Next")) {
					m_currentStepIndex = currentStep.nextIndex;
				}
			}

			EditorGUILayout.EndHorizontal ();

		}


		public	StepInfo	GetCurrentStep() {

			return m_steps [m_currentStepIndex];

		}

	}

}
