using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Components
{
    public struct NetworkFloat : IComponentData
    {
        [GhostField(Quantization = 1000)] public float Value;
    }
}