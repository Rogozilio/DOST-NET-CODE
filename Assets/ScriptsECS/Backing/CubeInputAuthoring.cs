using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[GhostComponent(PrefabType=GhostPrefabType.AllPredicted)]
public struct CubeInput : IInputComponentData
{
    public int Horizontal;
    public int Vertical;
    public bool IsControlledCube;
}

[DisallowMultipleComponent]
public class CubeInputAuthoring : MonoBehaviour
{
    class Baking : Baker<CubeInputAuthoring>
    {
        public override void Bake(CubeInputAuthoring authoring)
        {
            AddComponent<CubeInput>(GetEntity(TransformUsageFlags.Dynamic));
        }
    }
}
