using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct BallSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnBallRequest>();
        Debug.Log("[BallSpawnSystem] System created and waiting for SpawnBallRequest");
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        BallSpawner spawner = GameObject.FindFirstObjectByType<BallSpawner>();

        Debug.Log($"[BallSpawnSystem] OnUpdate called. Spawner found: {spawner != null}");

        foreach(var(request, requestEntity) in SystemAPI.Query<RefRO<SpawnBallRequest>>().WithEntityAccess())
        {
            Debug.Log($"[BallSpawnSystem] Processing spawn request at position: {request.ValueRO.SpawnPosition}, direction: {request.ValueRO.Direction}, speed: {request.ValueRO.Speed}");

            Entity ballEntity = state.EntityManager.CreateEntity();
            ecb.AddComponent(ballEntity, LocalTransform.FromPosition(request.ValueRO.SpawnPosition));

            Unity.Physics.Material physicsMaterial = new Unity.Physics.Material
            {
                Friction = 0f,
                Restitution = 1f,
                FrictionCombinePolicy = Unity.Physics.Material.CombinePolicy.Maximum,
                RestitutionCombinePolicy = Unity.Physics.Material.CombinePolicy.Maximum,
                CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents
            };

            BlobAssetReference<Unity.Physics.Collider> sphereCollider = Unity.Physics.SphereCollider.Create(
                new SphereGeometry
                {
                    Center = float3.zero,
                    Radius = 0.375f
                },
                new CollisionFilter
                {
                    BelongsTo = 1u << 6,
                    CollidesWith = (1u << 7) | (1u << 8),
                    GroupIndex = 0
                },
                physicsMaterial
            );

            ecb.AddComponent(ballEntity, new PhysicsCollider {Value = sphereCollider});

            PhysicsMass mass = PhysicsMass.CreateDynamic(sphereCollider.Value.MassProperties, 1f);
            ecb.AddComponent(ballEntity, mass);

            float3 velocity = math.normalize(request.ValueRO.Direction) * request.ValueRO.Speed;
            Debug.Log($"[BallSpawnSystem] Calculated velocity: {velocity}");
            
            ecb.AddComponent(ballEntity, new PhysicsVelocity
            {
                Linear = velocity,
                Angular = float3.zero
            });

            ecb.AddComponent(ballEntity, new PhysicsGravityFactor {Value = 0f});

            ecb.AddComponent(ballEntity, new PhysicsDamping
            {
                Linear = 0f,
                Angular = 0f
            });

            ecb.AddSharedComponent(ballEntity, new PhysicsWorldIndex(0));

            ecb.AddComponent(ballEntity, new BallComponent {Speed = request.ValueRO.Speed});

            if(spawner != null && spawner.ballPrefab != null)
            {
                GameObject ballVisual = GameObject.Instantiate(spawner.ballPrefab, request.ValueRO.SpawnPosition, quaternion.identity);
                ecb.AddComponent(ballEntity, new BallVisualLink
                {
                    VisualGameObject = ballVisual
                });

                spawner.RegisterBall(ballVisual, ballEntity);
                Debug.Log($"[BallSpawnSystem] Visual GameObject created and registered: {ballVisual.name}");
            }
            else
            {
                Debug.LogWarning($"[BallSpawnSystem] Spawner or ballPrefab is null! spawner: {spawner != null}, prefab: {spawner?.ballPrefab != null}");
            }

            ecb.DestroyEntity(requestEntity);
            Debug.Log($"[BallSpawnSystem] Ball entity created successfully. Entity: {ballEntity}");
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct BallSpeedMaintainerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (velocity, ballComponent) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<BallComponent>>())
        {
            float currentSpeed = math.length(velocity.ValueRO.Linear);
            
            if (currentSpeed < 0.1f)
            {
                continue;
            }

            float targetSpeed = ballComponent.ValueRO.Speed;
            
            if (math.abs(currentSpeed - targetSpeed) > 0.5f)
            {
                float3 direction = math.normalize(velocity.ValueRO.Linear);
                velocity.ValueRW.Linear = direction * targetSpeed;
            }
        }
    }
}
