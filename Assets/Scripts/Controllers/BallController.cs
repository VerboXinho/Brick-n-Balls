using UnityEngine;

public class BallController : MonoBehaviour
{
     private bool isInitialized = false;
    private BallSpawner spawner;

    private void Start()
    {
        isInitialized = true;
    }

    public void SetSpawner(BallSpawner ballSpawner)
    {
        spawner = ballSpawner;
    }

    public void Launch(Vector3 direction, float speed)
    {
        if (!isInitialized)
        {
            Start();
        }
    }

    private void Update()
    {
        if (transform.position.y < -6.5f)
        {
            OnBallLost();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        BrickController brick = collision.gameObject.GetComponent<BrickController>();
        if (brick != null)
        {
            brick.TakeDamage();
        }
    }

    private void OnBallLost()
    {
        GameManager.Instance?.UseBall();
        
        if (spawner != null)
        {
            spawner.OnBallDestroyed();
        }
        
        Destroy(gameObject);
    }
}
