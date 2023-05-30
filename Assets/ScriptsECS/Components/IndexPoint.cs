using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    [ChunkSerializable]
    public struct IndexPoint : IComponentData
    {
        public int Index;
    }
}