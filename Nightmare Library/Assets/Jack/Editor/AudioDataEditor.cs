using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioData))]
public class AudioDataEditor : Editor
{
    private AudioData data;

    private SerializedProperty audioClip;

    private void OnEnable()
    {
        data = (AudioData)target;

        audioClip = serializedObject.FindProperty("audioClip");
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
