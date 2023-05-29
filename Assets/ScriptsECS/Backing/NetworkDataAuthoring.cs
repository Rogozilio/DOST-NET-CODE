using System;
using System.Collections.Generic;
using Components;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace ScriptsECS.Backing
{
    public class NetworkDataAuthoring : MonoBehaviour
    {
        public enum NetworkDataType
        {
            Bool,
            Int,
            Float,
            Float2,
            Float3,
            Quaternion
        }

        [Serializable]
        public struct NetworkDataItem
        {
            public string Name;
            public NetworkDataType Type;
        }

        public NetworkDataItem[] networkDataItems;
    }

    public class BakerNetworkData : Baker<NetworkDataAuthoring>
    {
        public override void Bake(NetworkDataAuthoring authoring)
        {
            foreach (var data in authoring.networkDataItems)
            {
                switch (data.Type)
                {
                    case NetworkDataAuthoring.NetworkDataType.Bool:
                        AddComponent(GetEntity(TransformUsageFlags.Dynamic), new NetworkBool());
                        break;
                    case NetworkDataAuthoring.NetworkDataType.Int:
                        AddComponent(GetEntity(TransformUsageFlags.Dynamic), new NetworkInt());
                        break;
                    case NetworkDataAuthoring.NetworkDataType.Float:
                        AddComponent(GetEntity(TransformUsageFlags.Dynamic), new NetworkFloat());
                        break;
                    case NetworkDataAuthoring.NetworkDataType.Float2:
                        AddComponent(GetEntity(TransformUsageFlags.Dynamic), new NetworkFloat2());
                        break;
                    case NetworkDataAuthoring.NetworkDataType.Float3:
                        AddComponent(GetEntity(TransformUsageFlags.Dynamic), new NetworkFloat3());
                        break;
                    case NetworkDataAuthoring.NetworkDataType.Quaternion:
                        AddComponent(GetEntity(TransformUsageFlags.Dynamic), new NetworkQuaternion());
                        break;
                }
            }
            
            //AddComponent(GetEntity(TransformUsageFlags.Dynamic), new Ghost());
        }
        
        
    }

#if(UNITY_EDITOR)
    [CustomEditor(typeof(NetworkDataAuthoring))]
    public class NetworkDataEditor : Editor
    {
        private SerializedProperty _propertyNetworkDataType;

        private void OnEnable()
        {
            _propertyNetworkDataType = serializedObject.FindProperty("networkDataItems");
        }

        public override void OnInspectorGUI()
        {
            for (var i = 0; i < _propertyNetworkDataType.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var data = _propertyNetworkDataType.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(data.FindPropertyRelative("Name"), GUIContent.none);
                EditorGUILayout.PropertyField(data.FindPropertyRelative("Type"), GUIContent.none);
                if (GUILayout.Button("X", GUILayout.Width(30f)))
                {
                    _propertyNetworkDataType.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add"))
            {
                _propertyNetworkDataType.arraySize++;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}