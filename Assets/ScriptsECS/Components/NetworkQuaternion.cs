using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Components
{
    public struct NetworkQuaternion : IComponentData
    {
        [GhostField(Quantization = 1000)] public quaternion Value;
    }
}