using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct SampleCubeInput : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        bool left = UnityEngine.Input.GetKey("left");
        bool right = UnityEngine.Input.GetKey("right");
        bool down = UnityEngine.Input.GetKey("down");
        bool up = UnityEngine.Input.GetKey("up");
        bool isControlledCube = UnityEngine.Input.GetKeyUp("space");

        foreach (var playerInput in SystemAPI.Query<RefRW<CubeInput>>().WithAll<GhostOwnerIsLocal>())
        {
            if (isControlledCube)
                playerInput.ValueRW.IsControlledCube = !playerInput.ValueRO.IsControlledCube;
            if (left)
                playerInput.ValueRW.Horizontal = -1;
            else if (right)
                playerInput.ValueRW.Horizontal = 1;
            else
                playerInput.ValueRW.Horizontal = 0;
            if (down)
                playerInput.ValueRW.Vertical = -1;
            else if (up)
                playerInput.ValueRW.Vertical = 1;
            else
                playerInput.ValueRW.Vertical = 0;

        }
    }
}
