using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class WallAuthoring : MonoBehaviour
{
    public bool isBottomWall = false;

    class Baker : Baker<WallAuthoring>
    {
        public override void Bake(WallAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            if(authoring.isBottomWall)
            {
                AddComponent<BottomWallComponent>(entity);
            }
            else
            {
                AddComponent<WallComponent>(entity);
            }
        }
    }
}
