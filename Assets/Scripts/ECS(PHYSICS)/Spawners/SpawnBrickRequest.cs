using Unity.Entities;
using Unity.Mathematics;using System.ComponentModel;

public struct SpawnBrickRequest : IComponentData
{
    public float3 Position;
    public int Health;
    public int PointsValue;
}
