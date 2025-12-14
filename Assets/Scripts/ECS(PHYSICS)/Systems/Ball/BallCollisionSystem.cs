using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct BallCollisionSystem : ISystem
{
    private ComponentLookup<BallComponent> ballLookup;
    private ComponentLookup<BottomWallComponent> bottomWallLookup;
    private ComponentLookup<BrickComponent> brickLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        
        ballLookup = state.GetComponentLookup<BallComponent>(true);
        bottomWallLookup = state.GetComponentLookup<BottomWallComponent>(true);
        brickLookup = state.GetComponentLookup<BrickComponent>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        ballLookup.Update(ref state);
        bottomWallLookup.Update(ref state);
        brickLookup.Update(ref state);

        NativeList<Entity> ballsToDestroy = new NativeList<Entity>(Allocator.TempJob);
        NativeList<Entity> bricksHit = new NativeList<Entity>(Allocator.TempJob);

        state.Dependency = new CollisionEventJob
        {
            BallLookup = ballLookup,
            BottomWallLookup = bottomWallLookup,
            BrickLookup = brickLookup,
            BallsToDestroy = ballsToDestroy,
            BricksHit = bricksHit
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        state.Dependency.Complete();

        BallSpawner spawner = GameObject.FindFirstObjectByType<BallSpawner>();

        if (ballsToDestroy.Length > 0)
        {
            foreach (Entity ball in ballsToDestroy)
            {
                Debug.Log($"[BallCollisionSystem] Ball hit bottom wall - destroying");
                
                if (spawner != null)
                {
                    spawner.OnBallDestroyed();
                }
                
                BallDestructionQueue.Enqueue(ball);
                if(GameManager.Instance.ballsRemaining <= 0)
                {
                    GameManager.Instance.EndGame();
                }
            }
        }

        if (bricksHit.Length > 0)
        {
            foreach (Entity brick in bricksHit)
            {
                BrickHitQueue.Enqueue(brick);
            }
        }

        ballsToDestroy.Dispose();
        bricksHit.Dispose();
    }

    [BurstCompile]
    private struct CollisionEventJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<BallComponent> BallLookup;
        [ReadOnly] public ComponentLookup<BottomWallComponent> BottomWallLookup;
        [ReadOnly] public ComponentLookup<BrickComponent> BrickLookup;
        public NativeList<Entity> BallsToDestroy;
        public NativeList<Entity> BricksHit;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;

            bool aIsBall = BallLookup.HasComponent(entityA);
            bool bIsBall = BallLookup.HasComponent(entityB);

            if (!aIsBall && !bIsBall) return;

            Entity ballEntity = aIsBall ? entityA : entityB;
            Entity otherEntity = aIsBall ? entityB : entityA;

            if (BottomWallLookup.HasComponent(otherEntity))
            {
                if (!BallsToDestroy.Contains(ballEntity))
                {
                    BallsToDestroy.Add(ballEntity);
                }
            }
            else if (BrickLookup.HasComponent(otherEntity))
            {
                if (!BricksHit.Contains(otherEntity))
                {
                    BricksHit.Add(otherEntity);
                }
            }
        }
    }
}

public static class BallDestructionQueue
{
    private static System.Collections.Generic.Queue<Entity> queue = new System.Collections.Generic.Queue<Entity>();
    
    public static void Enqueue(Entity entity)
    {
        queue.Enqueue(entity);
    }
    
    public static bool TryDequeue(out Entity entity)
    {
        if (queue.Count > 0)
        {
            entity = queue.Dequeue();
            return true;
        }
        entity = Entity.Null;
        return false;
    }
}

public static class BrickHitQueue
{
    private static System.Collections.Generic.Queue<Entity> queue = new System.Collections.Generic.Queue<Entity>();
    
    public static void Enqueue(Entity entity)
    {
        queue.Enqueue(entity);
    }
    
    public static bool TryDequeue(out Entity entity)
    {
        if (queue.Count > 0)
        {
            entity = queue.Dequeue();
            return true;
        }
        entity = Entity.Null;
        return false;
    }
}
