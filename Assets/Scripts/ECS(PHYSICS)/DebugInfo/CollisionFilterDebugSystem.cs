using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct CollisionFilterDebugSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        int brickCount = 0;
        int wallCount = 0;
        int ballCount = 0;
        int totalPhysicsColliders = 0;

        // Log collision filters for each entity type
        foreach (var (collider, entity) in SystemAPI.Query<RefRO<PhysicsCollider>>().WithEntityAccess())
        {
            totalPhysicsColliders++;
            
            if (SystemAPI.HasComponent<BrickComponent>(entity))
            {
                brickCount++;
                if (brickCount == 1)
                {
                    var filter = collider.ValueRO.Value.Value.GetCollisionFilter();
                    Debug.Log($"[CollisionDebug] BRICK - BelongsTo: {filter.BelongsTo}, CollidesWith: {filter.CollidesWith}");
                }
            }
            else if (SystemAPI.HasComponent<WallComponent>(entity))
            {
                wallCount++;
                if (wallCount == 1)
                {
                    var filter = collider.ValueRO.Value.Value.GetCollisionFilter();
                    Debug.Log($"[CollisionDebug] WALL - BelongsTo: {filter.BelongsTo}, CollidesWith: {filter.CollidesWith}");
                }
            }
            else if (SystemAPI.HasComponent<BottomWallComponent>(entity))
            {
                wallCount++;
                if (wallCount == 1)
                {
                    var filter = collider.ValueRO.Value.Value.GetCollisionFilter();
                    Debug.Log($"[CollisionDebug] BOTTOM WALL - BelongsTo: {filter.BelongsTo}, CollidesWith: {filter.CollidesWith}");
                }
            }
            else if (SystemAPI.HasComponent<BallComponent>(entity))
            {
                ballCount++;
                if (ballCount == 1)
                {
                    var filter = collider.ValueRO.Value.Value.GetCollisionFilter();
                    Debug.Log($"[CollisionDebug] BALL - BelongsTo: {filter.BelongsTo}, CollidesWith: {filter.CollidesWith}");
                }
            }
        }

        Debug.Log($"[CollisionDebug] Total entities with PhysicsCollider: {totalPhysicsColliders}");
        Debug.Log($"[CollisionDebug] Found - Bricks: {brickCount}, Walls: {wallCount}, Balls: {ballCount}");
    }
}
