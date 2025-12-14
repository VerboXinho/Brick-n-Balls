using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class WallEntitySpawner : SystemBase
{
    private bool hasSpawned = false;

    protected override void OnCreate()
    {
        Debug.Log("[WallEntitySpawner] System created");
    }

    protected override void OnUpdate()
    {
        if (!SceneManager.GetSceneByName("GameScene").isLoaded)
        {
            hasSpawned = false;
            return;
        }

        if (hasSpawned) return;

        WallAuthoring[] walls = GameObject.FindObjectsByType<WallAuthoring>(FindObjectsSortMode.None);

        if (walls.Length == 0)
        {
            Debug.LogWarning("[WallEntitySpawner] No WallAuthoring components found - waiting...");
            return;
        }

        hasSpawned = true;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (WallAuthoring wall in walls)
        {
            Entity wallEntity = ecb.CreateEntity();

            ecb.AddComponent(wallEntity, LocalTransform.FromPosition(wall.transform.position));

            float3 size;
            if (wall.name.Contains("Top") || wall.name.Contains("Bottom"))
            {
                size = new float3(20f, 1f, 1f);
            }
            else
            {
                size = new float3(1f, 20f, 1f);
            }

            Debug.Log($"[WallEntitySpawner] Creating {wall.name} - Position: {wall.transform.position}, Collider Size: {size}");

            Unity.Physics.Material physicsMaterial = Unity.Physics.Material.Default;
            physicsMaterial.Friction = 0f;
            physicsMaterial.Restitution = 1f;
            physicsMaterial.CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents;

            BlobAssetReference<Unity.Physics.Collider> boxCollider = Unity.Physics.BoxCollider.Create(
                new BoxGeometry
                {
                    Size = size,
                    Orientation = quaternion.identity,
                    Center = float3.zero,
                    BevelRadius = 0f
                },
                new CollisionFilter
                {
                    BelongsTo = 1u << 8,
                    CollidesWith = 1u << 6,
                    GroupIndex = 0
                },
                physicsMaterial
            );

            ecb.AddComponent(wallEntity, new PhysicsCollider { Value = boxCollider });

            PhysicsMass mass = PhysicsMass.CreateKinematic(MassProperties.UnitSphere);
            ecb.AddComponent(wallEntity, mass);

            ecb.AddComponent(wallEntity, new PhysicsVelocity());

            ecb.AddSharedComponent(wallEntity, new PhysicsWorldIndex(0));

            if (wall.isBottomWall)
            {
                ecb.AddComponent<BottomWallComponent>(wallEntity);
                Debug.Log($"[WallEntitySpawner] Created BOTTOM WALL entity: {wall.name}");
            }
            else
            {
                ecb.AddComponent<WallComponent>(wallEntity);
                Debug.Log($"[WallEntitySpawner] Created wall entity: {wall.name}");
            }
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();

        Debug.Log($"[WallEntitySpawner] Spawned {walls.Length} wall entities successfully!");
    }
}
