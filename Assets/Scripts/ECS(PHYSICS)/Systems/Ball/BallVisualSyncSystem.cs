using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(LocalToWorldSystem))]
public partial struct BallVisualSyncSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach(var(transform, visualLink) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<BallVisualLink>>())
        {
            if(visualLink.ValueRO.VisualGameObject.Value != null)
            {
                visualLink.ValueRO.VisualGameObject.Value.transform.position = transform.ValueRO.Position;
                visualLink.ValueRO.VisualGameObject.Value.transform.rotation = transform.ValueRO.Rotation;
            }
        }
    }
}
