using System;
using NetworkAPI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkSendTransform))]
public class NetworkSendTransformEditor : Editor
{
    private SerializedProperty _isRecipient;
    private SerializedProperty _target;
    private SerializedProperty _syncTarget;
    private SerializedProperty _syncPosition;
    private SerializedProperty _syncRotation;
    private SerializedProperty _syncScale;
    private SerializedProperty _sendEveryoneExceptMe;
    private SerializedProperty _sendType;
    private SerializedProperty _cycleSend;

    private void OnEnable()
    {
        _isRecipient = serializedObject.FindProperty("isRecipient");
        _target = serializedObject.FindProperty("target");
        _syncTarget = serializedObject.FindProperty("syncTarget");
        _syncPosition = serializedObject.FindProperty("syncPosition");
        _syncRotation = serializedObject.FindProperty("syncRotation");
        _syncScale = serializedObject.FindProperty("syncScale");
        _sendType = serializedObject.FindProperty("sendType");
        _sendEveryoneExceptMe = serializedObject.FindProperty("sendEveryoneExceptMe");
        _cycleSend = serializedObject.FindProperty("cycleSend");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_isRecipient);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_target);
        if (!_isRecipient.boolValue)
        {
            EditorGUILayout.LabelField("Sync:");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_syncTarget, new GUIContent("Target"));
            EditorGUILayout.PropertyField(_syncPosition, new GUIContent("Position"));
            EditorGUILayout.PropertyField(_syncRotation, new GUIContent("Rotation"));
            EditorGUILayout.PropertyField(_syncScale, new GUIContent("Scale"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_sendType);

            if (_sendType.enumValueIndex != 1)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_sendEveryoneExceptMe);
                EditorGUI.indentLevel--;
            }
            else 
            {
                _sendEveryoneExceptMe.boolValue = false;
            }
        
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_cycleSend);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}