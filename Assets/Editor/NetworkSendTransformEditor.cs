using System;
using NetworkAPI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkSendTransform))]
public class NetworkSendTransformEditor : Editor
{
    private SerializedProperty _recipient;
    private SerializedProperty _target;
    private SerializedProperty _isRequiresAuthority;
    private SerializedProperty _syncTarget;
    private SerializedProperty _syncPosition;
    private SerializedProperty _syncRotation;
    private SerializedProperty _syncScale;
    private SerializedProperty _sendType;
    private SerializedProperty _cycleSend;

    private void OnEnable()
    {
        _recipient = serializedObject.FindProperty("recipient");
        _target = serializedObject.FindProperty("target");
        _isRequiresAuthority = serializedObject.FindProperty("isRequiresAuthority");
        _syncTarget = serializedObject.FindProperty("syncTarget");
        _syncPosition = serializedObject.FindProperty("syncPosition");
        _syncRotation = serializedObject.FindProperty("syncRotation");
        _syncScale = serializedObject.FindProperty("syncScale");
        _sendType = serializedObject.FindProperty("sendType");
        _cycleSend = serializedObject.FindProperty("cycleSend");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_recipient);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_target);
        if (_recipient.enumValueIndex != 2)
        {
            EditorGUILayout.PropertyField(_isRequiresAuthority, new GUIContent("IsRequiresAuthority"));
            EditorGUILayout.LabelField("Sync:");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_syncTarget, new GUIContent("Target"));
            EditorGUILayout.PropertyField(_syncPosition, new GUIContent("Position"));
            EditorGUILayout.PropertyField(_syncRotation, new GUIContent("Rotation"));
            EditorGUILayout.PropertyField(_syncScale, new GUIContent("Scale"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_sendType);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_cycleSend);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}