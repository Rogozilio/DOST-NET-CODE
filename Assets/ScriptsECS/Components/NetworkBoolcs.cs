using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Components
{
    public struct NetworkBool : IComponentData
    {
        [GhostField] public bool Value;
    }
}