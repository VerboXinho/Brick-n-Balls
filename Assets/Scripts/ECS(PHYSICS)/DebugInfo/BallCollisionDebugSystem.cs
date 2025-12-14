using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

// Debug system for logging ball collision events
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct BallCollisionDebugSystem : ISystem
{
    private int frameCounter;

    public void OnCreate(ref SystemState state)
    {
        Debug.Log("[BallCollisionDebugSystem] Created");
        frameCounter = 0;
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<SimulationSingleton>(out var simulationSingleton))
        {
            if (frameCounter % 60 == 0)
            {
                Debug.LogWarning("[BallCollisionDebugSystem] SimulationSingleton not found!");
            }
            frameCounter++;
            return;
        }

        NativeReference<int> collisionCount = new NativeReference<int>(0, Allocator.TempJob);

        // Schedule job to count and log collisions
        state.Dependency = new CollisionDebugJob
        {
            BallGroup = SystemAPI.GetComponentLookup<BallComponent>(true),
            BrickGroup = SystemAPI.GetComponentLookup<BrickComponent>(true),
            WallGroup = SystemAPI.GetComponentLookup<WallComponent>(true),
            BottomWallGroup = SystemAPI.GetComponentLookup<BottomWallComponent>(true),
            CollisionCount = collisionCount
        }.Schedule(simulationSingleton, state.Dependency);

        state.Dependency.Complete();

        int count = collisionCount.Value;
        if (count > 0 || frameCounter % 120 == 0)
        {
            Debug.Log($"[BallCollisionDebugSystem] Frame {frameCounter}: {count} collisions detected");
        }

        collisionCount.Dispose();
        frameCounter++;
    }

    [BurstCompile]
    private struct CollisionDebugJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<BallComponent> BallGroup;
        [ReadOnly] public ComponentLookup<BrickComponent> BrickGroup;
        [ReadOnly] public ComponentLookup<WallComponent> WallGroup;
        [ReadOnly] public ComponentLookup<BottomWallComponent> BottomWallGroup;
        public NativeReference<int> CollisionCount;

        public void Execute(CollisionEvent collisionEvent)
        {
            CollisionCount.Value++;

            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;

            bool isBallA = BallGroup.HasComponent(entityA);
            bool isBallB = BallGroup.HasComponent(entityB);

            FixedString128Bytes typeA = GetEntityType(entityA);
            FixedString128Bytes typeB = GetEntityType(entityB);

            // Log ball collisions
            if (isBallA || isBallB)
            {
                Debug.Log($"[BallCollision] {typeA}({entityA.Index}) <-> {typeB}({entityB.Index}) | Normal: {collisionEvent.Normal}");
            }
        }

        private FixedString128Bytes GetEntityType(Entity entity)
        {
            if (BallGroup.HasComponent(entity)) return "BALL";
            if (BrickGroup.HasComponent(entity)) return "BRICK";
            if (WallGroup.HasComponent(entity)) return "WALL";
            if (BottomWallGroup.HasComponent(entity)) return "BOTTOM_WALL";
            return "UNKNOWN";
        }
    }
}
