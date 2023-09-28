using System;
using NetworkAPI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkSendRigidbody))]
public class NetworkSendRigidbodyEditor : Editor
{
    private SerializedProperty _isRecipient;
    private SerializedProperty _target;
    private SerializedProperty _isRequiresAuthority;
    private SerializedProperty _syncTarget;
    private SerializedProperty _syncVelocity;
    private SerializedProperty _syncAngularVelocity;
    private SerializedProperty _syncMass;
    private SerializedProperty _syncDrag;
    private SerializedProperty _syncAngularDrag;
    private SerializedProperty _syncUseGravity;
    private SerializedProperty _syncIsKinematic;
    private SerializedProperty _sendType;
    private SerializedProperty _cycleSend;


    private void OnEnable()
    {
        _isRecipient = serializedObject.FindProperty("isRecipient");
        _target = serializedObject.FindProperty("target");
        _isRequiresAuthority = serializedObject.FindProperty("isRequiresAuthority");
        _syncTarget = serializedObject.FindProperty("syncTarget");
        _syncVelocity = serializedObject.FindProperty("syncVelocity");
        _syncAngularVelocity = serializedObject.FindProperty("syncAngularVelocity");
        _syncMass = serializedObject.FindProperty("syncMass");
        _syncDrag = serializedObject.FindProperty("syncDrag");
        _syncAngularDrag = serializedObject.FindProperty("syncAngularDrag");
        _syncUseGravity = serializedObject.FindProperty("syncUseGravity");
        _syncIsKinematic= serializedObject.FindProperty("syncIsKinematic");
        _sendType = serializedObject.FindProperty("sendType");
        _cycleSend = serializedObject.FindProperty("cycleSend");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_isRecipient);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_target);
        if (!_isRecipient.boolValue)
        {
            EditorGUILayout.PropertyField(_isRequiresAuthority, new GUIContent("IsRequiresAuthority"));
            EditorGUILayout.LabelField("Sync:");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_syncTarget, new GUIContent("Target"));
            EditorGUILayout.PropertyField(_syncVelocity, new GUIContent("Velocity"));
            EditorGUILayout.PropertyField(_syncAngularVelocity, new GUIContent("AngularVelocity"));
            EditorGUILayout.PropertyField(_syncMass, new GUIContent("Mass"));
            EditorGUILayout.PropertyField(_syncDrag, new GUIContent("Drag"));
            EditorGUILayout.PropertyField(_syncAngularDrag, new GUIContent("AngularDrag"));
            EditorGUILayout.PropertyField(_syncUseGravity, new GUIContent("UseGravity"));
            EditorGUILayout.PropertyField(_syncIsKinematic, new GUIContent("IsKinematic"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_sendType);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_cycleSend);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}