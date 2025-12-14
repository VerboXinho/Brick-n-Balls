using UnityEngine;

public static class GameManagerProxy
{
    public static void AddScore(int points)
    {
        GameManager.Instance.AddScore(points);
    }

    public static void UseBall()
    {
        GameManager.Instance.UseBall();
    }
}

