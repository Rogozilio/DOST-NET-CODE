using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Components
{
    public struct NetworkInt : IComponentData
    {
        [GhostField] public int Value;
    }
}