using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct CubeMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var points = new NativeArray<float3>(
            new[]
            {
                new float3(-2, 0, -2),
                new float3(-2, 0, 2),
                new float3(2, 0, 2),
                new float3(2, 0, -2)
            }, Allocator.Temp
        );
        
        var speed = SystemAPI.Time.DeltaTime * 4;
        foreach (var (input, pointsForMove, trans) in SystemAPI
                     .Query<RefRO<CubeInput>, RefRW<IndexPoint>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            if (input.ValueRO.IsControlledCube)
            {
                var moveInput = new float2(input.ValueRO.Horizontal, input.ValueRO.Vertical);
                moveInput = math.normalizesafe(moveInput) * speed;
                trans.ValueRW.Position += new float3(moveInput.x, 0, moveInput.y);
            }
            else
            {
                if (math.distance(trans.ValueRO.Position,
                        points[pointsForMove.ValueRO.Index]) < 0.01)
                    pointsForMove.ValueRW.Index =
                        (pointsForMove.ValueRO.Index == 3) ? 0 : pointsForMove.ValueRO.Index + 1;
                trans.ValueRW.Position +=
                    (points[pointsForMove.ValueRO.Index] - trans.ValueRO.Position) * speed;
            }
        }

        points.Dispose();
    }
}