using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }

    private GameState currentState = GameState.Menu;
    private int currentScore = 0;
    public int ballsRemaining = 0;

    private const int INITIAL_BALLS_COUNT = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetGameState(GameState.Menu);
    }

    public void SetGameState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.Menu:
                UIManager.Instance?.ShowMenu();
                break;
            case GameState.Playing:
                UIManager.Instance?.ShowGameplay();
                break;
            case GameState.GameOver:
                UIManager.Instance?.ShowGameOver(currentScore);
                break;
        }
    }
    public void StartGame()
    {
        currentScore = 0;
        ballsRemaining = INITIAL_BALLS_COUNT;
        
        if (!IsSceneLoaded("GameScene"))
        {
            SceneManager.LoadScene("GameScene", LoadSceneMode.Additive);
        }
        else
        {
            BallSpawner spawner = FindFirstObjectByType<BallSpawner>();
            if (spawner != null)
            {
                spawner.ClearAllBalls();
            }
        }

        SetGameState(GameState.Playing);
    }
    public void EndGame()
    {
        BallSpawner spawner = FindFirstObjectByType<BallSpawner>();
        if (spawner != null)
        {
            spawner.ClearAllBalls();
        }
        
        GameCleanupSystem.RequestCleanup();
        
        SetGameState(GameState.GameOver);
        
        if (IsSceneLoaded("GameScene"))
        {
            SceneManager.UnloadSceneAsync("GameScene");
        }
    }

    public void ReturnToMenu()
    {
        if (IsSceneLoaded("GameScene"))
        {
            BallSpawner spawner = FindFirstObjectByType<BallSpawner>();
            if (spawner != null)
            {
                spawner.ClearAllBalls();
            }
            
            GameCleanupSystem.RequestCleanup();
            SceneManager.UnloadSceneAsync("GameScene");
        }

        SetGameState(GameState.Menu);
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UIManager.Instance?.UpdateScore(currentScore);
    }

    public void UseBall()
    {
        ballsRemaining--;
        UIManager.Instance?.UpdateBallsRemaining(ballsRemaining);

        if (ballsRemaining < 0)
        {
            EndGame();
        }
    }

    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    public int GetScore()
    {
        return currentScore;
    }

    public int GetBallsRemaining()
    {
        return ballsRemaining;
    }
}
