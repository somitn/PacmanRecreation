using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Countdown,
        Playing,
        PacDying,
        GameOver
    }

    public GameState State { get; private set; }

    [Header("HUD References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameTimerText;
    [SerializeField] private TMP_Text ghostTimerText;
    [SerializeField] private GameObject[] lifeIcons;

    [Header("Countdown")]
    [SerializeField] private GameObject blocker;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private AudioManager audioManager;

    [Header("Ghost References")]
    [SerializeField] private GhostState[] ghosts;

    [Header("Cherry")]
    [SerializeField] private CherryController cherryController;

    [Header("Scared Mode")]
    [SerializeField] private float scaredDuration = 10f;
    [SerializeField] private float recoverAt = 3f;

    private int score = 0;
    private int lives = 3;
    private int pelletsLeft;
    private float gameTime = 0f;
    private float scaredTimer = 0f;
    private bool recoveringStarted = false;

    public float ScaredTimer => scaredTimer;

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
        int normalPellets = GameObject.FindGameObjectsWithTag("Pellet").Length;
        int powerPellets = GameObject.FindGameObjectsWithTag("PowerPellet").Length;
        pelletsLeft = normalPellets + powerPellets;

        Debug.Log("Pellets left: " + pelletsLeft);

        UpdateScoreText();
        UpdateGameTimerText();

        State = GameState.Countdown;
        StartCoroutine(RoundStart());
    }

    void Update()
    {
        if (State != GameState.Playing) return;

        gameTime += Time.deltaTime;
        UpdateGameTimerText();

        if (scaredTimer > 0f)
        {
            scaredTimer -= Time.deltaTime;

            if (ghostTimerText != null)
                ghostTimerText.text = "Scared: " + Mathf.CeilToInt(scaredTimer);

            if (scaredTimer <= recoverAt && !recoveringStarted)
            {
                recoveringStarted = true;
                foreach (var g in ghosts)
                    if (g != null && g.CurrentState != GhostState.State.Dead)
                        g.SetState(GhostState.State.Recovering);
            }

            if (scaredTimer <= 0f)
            {
                scaredTimer = 0f;
                foreach (var g in ghosts)
                    if (g != null && g.CurrentState != GhostState.State.Dead)
                        g.SetState(GhostState.State.Normal);

                if (ghostTimerText != null)
                    ghostTimerText.gameObject.SetActive(false);

                if (audioManager != null) audioManager.PlayNormal();
            }
        }
    }

    private IEnumerator RoundStart()
    {
        if (blocker != null) blocker.SetActive(true);
        if (countdownText != null) countdownText.gameObject.SetActive(true);
        if (audioManager != null) audioManager.PlayIntro();

        string[] labels = { "3", "2", "1", "GO!" };
        foreach (string label in labels)
        {
            if (countdownText != null) countdownText.text = label;
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (blocker != null) blocker.SetActive(false);
        if (audioManager != null) audioManager.PlayNormal();

        State = GameState.Playing;

        if (cherryController != null) cherryController.StartSpawning();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score " + score.ToString("D6");
    }

    public void PelletEaten()
    {
        pelletsLeft--;
        if (pelletsLeft <= 0)
            Debug.Log("All pellets eaten - game over");
    }

    public void PowerPelletEaten()
    {
        AddScore(50);
        scaredTimer = scaredDuration;
        recoveringStarted = false;

        foreach (var g in ghosts)
            if (g != null && g.CurrentState != GhostState.State.Dead)
                g.SetState(GhostState.State.Scared);

        if (ghostTimerText != null)
            ghostTimerText.gameObject.SetActive(true);

        if (audioManager != null) audioManager.PlayScared();
    }

    private void UpdateGameTimerText()
    {
        if (gameTimerText != null)
            gameTimerText.text = "Time: " + FormatTime(gameTime);
    }

    public static string FormatTime(float t)
    {
        int minutes = (int)(t / 60);
        int seconds = (int)(t % 60);
        int hundredths = (int)((t * 100) % 100);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
    }
}