using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    [Header("Brick Prefab")]
    public GameObject brickPrefab;

    [Header("Brick Settings")]
    public int rows = 5;
    public int columns = 8;
    public float brickSpacing = 0.2f;
    public Vector3 gridStartPosition = new Vector3(-6f, 8f, 0f);

    [Header("Hit Points Distribution")]
    [Range(0f, 1f)] public float oneHitPointChance = 0.5f;
    [Range(0f, 1f)] public float twoHitPointChance = 0.3f;

    private void Start()
    {
        SpawnBricks();
    }

    public void SpawnBricks()
    {
        if (brickPrefab == null)
        {
            Debug.LogError("BrickSpawner: brickPrefab is not assigned!");
            return;
        }

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        float brickWidth = 1.5f + brickSpacing;
        float brickHeight = 0.5f + brickSpacing;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 position = gridStartPosition + new Vector3(
                    col * brickWidth,
                    -row * brickHeight,
                    0f
                );

                int health = DetermineHitPoints();

                Entity requestEntity = entityManager.CreateEntity();
                entityManager.AddComponentData(requestEntity, new SpawnBrickRequest
                {
                    Position = position,
                    Health = health,
                    PointsValue = 1
                });
            }
        }

        Debug.Log($"Requested spawning {rows * columns} bricks via ECS");
    }

    private int DetermineHitPoints()
    {
        float random = UnityEngine.Random.value;

        if (random < oneHitPointChance)
        {
            return 1;
        }
        else if (random < oneHitPointChance + twoHitPointChance)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }
}
