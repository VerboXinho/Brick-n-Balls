using Unity.Entities;

public struct BallComponent : IComponentData
{
    public float Speed;
}

public struct BallVisualLink : IComponentData
{
    public UnityObjectRef<UnityEngine.GameObject> VisualGameObject;
}
