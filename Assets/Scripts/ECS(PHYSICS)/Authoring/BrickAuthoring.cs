using Unity.Entities;
using UnityEngine;

public class BrickAuthoring : MonoBehaviour
{
    public int health = 1;
    public int pointsValue = 1;

    class Baker : Baker<BrickAuthoring>
    {
        public override void Bake(BrickAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BrickComponent
            {
                Health = authoring.health,
                PointsValue = authoring.pointsValue
            });

            AddComponent(entity, new BrickVisualLink
            {
                VisualGameObject  = authoring.gameObject
            });
        }
    }
}
