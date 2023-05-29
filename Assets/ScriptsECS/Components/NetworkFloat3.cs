using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Components
{
    public struct NetworkFloat3 : IComponentData
    {
        [GhostField(Quantization = 1000)] public float3 Value;
    }
}