using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public partial class GameCleanupSystem : SystemBase
{
    private static bool cleanupRequested = false;

    protected override void OnUpdate()
    {
        if (!cleanupRequested)
            return;

        Debug.Log("[GameCleanupSystem] Cleaning up all game entities...");

        EntityQuery ballQuery = GetEntityQuery(ComponentType.ReadOnly<BallComponent>());
        EntityQuery brickQuery = GetEntityQuery(ComponentType.ReadOnly<BrickComponent>());
        EntityQuery wallQuery = GetEntityQuery(ComponentType.ReadOnly<WallComponent>());
        EntityQuery bottomWallQuery = GetEntityQuery(ComponentType.ReadOnly<BottomWallComponent>());

        NativeArray<Entity> ballEntities = ballQuery.ToEntityArray(Allocator.Temp);
        NativeArray<Entity> brickEntities = brickQuery.ToEntityArray(Allocator.Temp);
        NativeArray<Entity> wallEntities = wallQuery.ToEntityArray(Allocator.Temp);
        NativeArray<Entity> bottomWallEntities = bottomWallQuery.ToEntityArray(Allocator.Temp);

        DestroyVisualsAndEntities<BallVisualLink>(ballEntities, "balls");
        DestroyVisualsAndEntities<BrickVisualLink>(brickEntities, "bricks");

        EntityManager.DestroyEntity(wallEntities);
        EntityManager.DestroyEntity(bottomWallEntities);

        ballEntities.Dispose();
        brickEntities.Dispose();
        wallEntities.Dispose();
        bottomWallEntities.Dispose();

        ClearAllQueues();

        Debug.Log("[GameCleanupSystem] Cleanup complete!");
        cleanupRequested = false;
    }

    private void DestroyVisualsAndEntities<T>(NativeArray<Entity> entities, string typeName) where T : unmanaged, IComponentData
    {
        ComponentLookup<T> visualLookup = GetComponentLookup<T>(true);

        foreach (Entity entity in entities)
        {
            if (visualLookup.HasComponent(entity))
            {
                var visualLink = EntityManager.GetComponentData<T>(entity);
                
                if (typeof(T) == typeof(BallVisualLink))
                {
                    BallVisualLink link = (BallVisualLink)(object)visualLink;
                    if (link.VisualGameObject.Value != null)
                    {
                        GameObject.Destroy(link.VisualGameObject.Value);
                    }
                }
                else if (typeof(T) == typeof(BrickVisualLink))
                {
                    BrickVisualLink link = (BrickVisualLink)(object)visualLink;
                    if (link.VisualGameObject.Value != null)
                    {
                        GameObject.Destroy(link.VisualGameObject.Value);
                    }
                }
            }
        }

        EntityManager.DestroyEntity(entities);
        Debug.Log($"[GameCleanupSystem] Destroyed {entities.Length} {typeName}");
    }

    private void ClearAllQueues()
    {
        while (BallDestructionQueue.TryDequeue(out _)) { }
        while (BrickHitQueue.TryDequeue(out _)) { }
        Debug.Log("[GameCleanupSystem] Cleared all queues");
    }

    public static void RequestCleanup()
    {
        cleanupRequested = true;
        Debug.Log("[GameCleanupSystem] Cleanup requested");
    }
}
