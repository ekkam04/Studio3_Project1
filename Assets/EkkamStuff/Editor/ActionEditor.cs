using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Action))]
public class ActionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        Action action = (Action)target;
        
        if (GUILayout.Button("Activate Action"))
        {
            action.Signal();
        }

    }
}
