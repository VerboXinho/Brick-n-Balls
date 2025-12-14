using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct BrickSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnBrickRequest>();
    }

    public void OnUpdate(ref SystemState state)
    {
        BrickSpawner spawner = GameObject.FindFirstObjectByType<BrickSpawner>();

        NativeArray<Entity> requestEntities = SystemAPI.QueryBuilder().WithAll<SpawnBrickRequest>().Build().ToEntityArray(Allocator.Temp);
        NativeArray<SpawnBrickRequest> requests = SystemAPI.QueryBuilder().WithAll<SpawnBrickRequest>().Build().ToComponentDataArray<SpawnBrickRequest>(Allocator.Temp);

        Debug.Log($"[BrickSpawnSystem] Processing {requests.Length} brick spawn requests");

        for (int i = 0; i < requests.Length; i++)
        {
            SpawnBrickRequest request = requests[i];

            Entity brickEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(brickEntity, LocalTransform.FromPosition(request.Position));

            Unity.Physics.Material physicsMaterial = new Unity.Physics.Material
            {
                Friction = 0f,
                Restitution = 1f,
                FrictionCombinePolicy = Unity.Physics.Material.CombinePolicy.Minimum,
                RestitutionCombinePolicy = Unity.Physics.Material.CombinePolicy.Maximum,
                CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents
            };

            BlobAssetReference<Unity.Physics.Collider> boxCollider = Unity.Physics.BoxCollider.Create(
                new BoxGeometry
                {
                    Center = float3.zero,
                    Orientation = quaternion.identity,
                    Size = new float3(1.5f, 0.5f, 1f),
                    BevelRadius = 0f
                },
                new CollisionFilter
                {
                    BelongsTo = 1u << 7,
                    CollidesWith = 1u << 6,
                    GroupIndex = 0
                },
                physicsMaterial
            );

            state.EntityManager.AddComponentData(brickEntity, new PhysicsCollider {Value = boxCollider});

            PhysicsMass mass = PhysicsMass.CreateKinematic(boxCollider.Value.MassProperties);
            state.EntityManager.AddComponentData(brickEntity, mass);

            state.EntityManager.AddComponentData(brickEntity, new PhysicsVelocity {Linear = float3.zero, Angular = float3.zero});
            state.EntityManager.AddSharedComponent(brickEntity, new PhysicsWorldIndex(0));

            state.EntityManager.AddComponentData(brickEntity, new BrickComponent
            {
                Health = request.Health,
                PointsValue = request.PointsValue
            });

            if(spawner != null && spawner.brickPrefab != null)
            {
                GameObject brickVisual = GameObject.Instantiate(spawner.brickPrefab, request.Position, quaternion.identity);

                BrickController brickController = brickVisual.GetComponent<BrickController>();
                if (brickController != null)
                {
                    brickController.hitPoints = request.Health;
                }

                state.EntityManager.AddComponentData(brickEntity, new BrickVisualLink
                {
                    VisualGameObject = brickVisual
                });

                Debug.Log($"[BrickSpawnSystem] Spawned brick at {request.Position}, HP={request.Health}");
            }
            else
            {
                Debug.LogWarning("[BrickSpawnSystem] Spawner or brickPrefab is null!");
            }
        }

        state.EntityManager.DestroyEntity(requestEntities);
        
        requestEntities.Dispose();
        requests.Dispose();
    }
}
