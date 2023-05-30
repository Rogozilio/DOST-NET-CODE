using Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct Cube : IComponentData
{
    public Vector3 Position;
}

[DisallowMultipleComponent]
public class CubeAuthoring : MonoBehaviour
{
    public Vector3 vector;
    class Baker : Baker<CubeAuthoring>
    {
        public override void Bake(CubeAuthoring authoring)
        {
            Cube component = default(Cube);
            component.Position = authoring.vector;
            IndexPoint indexPoint = default(IndexPoint);
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), component);
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), indexPoint);
        }
    }
}
