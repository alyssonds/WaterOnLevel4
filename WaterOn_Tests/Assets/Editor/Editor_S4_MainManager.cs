using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(S4_MainManager))]
public class Editor_S4_MainManager : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();
	/*	S4_MainManager refScript = (S4_MainManager)target;

		GameObject selectedGO = null;

		GUILayout.Space (20);

		GUILayout.Label ("River Generator!");

		if (GUILayout.Button ("Add a River Point!")) {
			selectedGO = refScript.AddingPoint ();
			Selection.activeObject = selectedGO;
		}
		
		if (GUILayout.Button ("Delete last River Point!")) 
			refScript.DeleteLastPoint ();
		
		if (GUILayout.Button ("Reset All!")) 
			refScript.ResetAll ();

		if (GUILayout.Button ("CREATE RIVER!"))
			refScript.CreatingRiver ();
*/
	}
		
		
}
