using Unity.Entities;

public struct BrickComponent : IComponentData
{
    public int Health;
    public int PointsValue;
}

public struct BrickVisualLink : IComponentData
{
    public UnityObjectRef<UnityEngine.GameObject> VisualGameObject;
}
