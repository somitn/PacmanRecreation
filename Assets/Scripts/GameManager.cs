using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Singleton
    // ---------------------------------------------------------------
    public static GameManager Instance { get; private set; }

    // ---------------------------------------------------------------
    // Game state
    // ---------------------------------------------------------------
    public enum GameState
    {
        Countdown,
        Playing,
        PacDying,
        GameOver
    }

    public GameState State { get; private set; }

    // ---------------------------------------------------------------
    // HUD references
    // ---------------------------------------------------------------
    [Header("HUD References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameTimerText;
    [SerializeField] private TMP_Text ghostTimerText;
    [SerializeField] private GameObject[] lifeIcons;

    // ---------------------------------------------------------------
    // Game values
    // ---------------------------------------------------------------
    private int score = 0;
    private int lives = 3;
    private int pelletsLeft;
    private float gameTime = 0f;

    // ---------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Count total pellets using tags
        int normalPellets = GameObject.FindGameObjectsWithTag("Pellet").Length;
        int powerPellets = GameObject.FindGameObjectsWithTag("PowerPellet").Length;
        pelletsLeft = normalPellets + powerPellets;

        Debug.Log("Pellets left: " + pelletsLeft); // temporary, for testing

        // TEMPORARY: Step 9 changes this to Countdown
        State = GameState.Playing;

        UpdateScoreText();
    }

    // ---------------------------------------------------------------
    // Score
    // ---------------------------------------------------------------
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score " + score.ToString("D6");
        }
    }

    // ---------------------------------------------------------------
    // Pellet tracking
    // ---------------------------------------------------------------
    public void PelletEaten()
    {
        pelletsLeft--;

        if (pelletsLeft <= 0)
        {
            Debug.Log("All pellets eaten - game over"); // placeholder until Step 11
        }
    }
}