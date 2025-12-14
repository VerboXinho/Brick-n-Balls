using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject gameplayPanel;
    public GameObject gameOverPanel;

    [Header("Menu Panel")]
    public Button startGameButton;

    [Header("Gameplay Panel")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ballsRemainingText;

    [Header("GameOver Panel")]
    public TextMeshProUGUI finalScoreText;
    public Button backToMenuButton;

    private void Awake()
    {
        // Implement singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Register button click listeners
        startGameButton.onClick.AddListener(OnStartGameClicked);
        backToMenuButton.onClick.AddListener(OnBackToMenuClicked);

        ShowMenu();
    }

    public void ShowMenu()
    {
        // Display only the menu panel
        menuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameplay()
    {
        // Display only the gameplay panel and initialize display values
        menuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        gameOverPanel.SetActive(false);

        UpdateScore(0);
        UpdateBallsRemaining(GameManager.Instance.GetBallsRemaining());
    }

    public void ShowGameOver(int finalScore)
    {
        // Display only the game over panel with final score
        menuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        finalScoreText.text = $"Final Score: {finalScore}";
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    public void UpdateBallsRemaining(int ballsLeft)
    {
        ballsRemainingText.text = $"Balls Left: {ballsLeft}";
    }

    private void OnStartGameClicked()
    {
        GameManager.Instance?.StartGame();
    }

    private void OnBackToMenuClicked()
    {
        GameManager.Instance?.ReturnToMenu();
    }
}
