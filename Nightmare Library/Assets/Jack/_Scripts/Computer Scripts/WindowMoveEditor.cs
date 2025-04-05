using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WindowMoveController))]
public class WindowMoveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //Need this to show other variables aside from just the delegates
        DrawDefaultInspector();
    }
}
