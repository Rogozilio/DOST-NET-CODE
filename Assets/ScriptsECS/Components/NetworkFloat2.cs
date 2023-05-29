using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Components
{
    public struct NetworkFloat2 : IComponentData
    {
        [GhostField(Quantization = 1000)] public float2 Value;
    }
}