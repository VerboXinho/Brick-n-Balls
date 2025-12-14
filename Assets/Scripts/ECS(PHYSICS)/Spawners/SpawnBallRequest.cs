using Unity.Entities;
using Unity.Mathematics;

public struct SpawnBallRequest : IComponentData
{
    public float3 SpawnPosition;
    public float3 Direction;
    public float Speed;
}
