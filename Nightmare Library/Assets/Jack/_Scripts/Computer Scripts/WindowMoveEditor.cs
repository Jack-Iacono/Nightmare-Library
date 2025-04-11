using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(WindowMoveController))]
public class WindowMoveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //Need this to show other variables aside from just the delegates
        DrawDefaultInspector();
    }
}

#endif