using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public Transform spawnPoint;
    public float ballSpeed = 15f;

    private GameObject activeBall;
    private Entity activeBallEntity;

    public void ShootBallWithDirection(Vector3 direction)
    {
        if (ballPrefab == null || spawnPoint == null) return;

        if (activeBall != null) return;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity requestEntity = entityManager.CreateEntity();

        entityManager.AddComponentData(requestEntity, new SpawnBallRequest
        {
            SpawnPosition = spawnPoint.position,
            Direction = new float3(direction.x, direction.y, direction.z),
            Speed = ballSpeed
        });

        GameManager.Instance?.UseBall();

        Debug.Log($"[BallSpawner] Shooting ball - Direction: {direction}, Speed: {ballSpeed}");
    }

    public void RegisterBall(GameObject ball, Entity entity)
    {
        activeBall = ball;
        activeBallEntity = entity;
    }

    public void OnBallDestroyed()
    {
        activeBall = null;
        activeBallEntity = Entity.Null;
    }

    public bool HasActiveBall()
    {
        return activeBall != null;
    }

    public void ClearAllBalls()
    {
        if (activeBall != null)
        {
            Destroy(activeBall);
            activeBall = null;
        }

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if(entityManager != null && entityManager.Exists(activeBallEntity))
        {
            entityManager.DestroyEntity(activeBallEntity);
            activeBallEntity = Entity.Null;
        }
    }
}
