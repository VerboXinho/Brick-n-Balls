using UnityEngine;

public class BrickController : MonoBehaviour
{
    [Header("Brick Properties")]
    public int hitPoints = 1;

    [Header("Materials")]
    public UnityEngine.Material material1HP;
    public UnityEngine.Material material2HP;
    public UnityEngine.Material material3HP;

    private MeshRenderer meshRenderer;
    private int currentHitPoints;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        currentHitPoints = hitPoints;
        UpdateVisual(currentHitPoints);
    }

    public void TakeDamage()
    {
        currentHitPoints--;
        GameManager.Instance?.AddScore(1);
        if(currentHitPoints <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            UpdateVisual(currentHitPoints);
        }
    }

    public void UpdateVisual(int health)
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogWarning("[BrickController] No MeshRenderer found!");
                return;
            }
        }

        switch (health)
        {
            case 3:
                if (material3HP != null)
                    meshRenderer.material = material3HP;
                else
                    Debug.LogWarning("[BrickController] material3HP is null!");
                break;
            case 2:
                if (material2HP != null)
                    meshRenderer.material = material2HP;
                else
                    Debug.LogWarning("[BrickController] material2HP is null!");
                break;
            case 1:
                if (material1HP != null)
                    meshRenderer.material = material1HP;
                else
                    Debug.LogWarning("[BrickController] material1HP is null!");
                break;
        }
    }
}
