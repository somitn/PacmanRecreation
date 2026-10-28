using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("Level 1 High Score")]
    [SerializeField] private TMP_Text level1HighScoreText;
    [SerializeField] private TMP_Text level1BestTimeText;

    [Header("Level 2 High Score (100% HD band)")]
    [SerializeField] private TMP_Text level2HighScoreText;
    [SerializeField] private TMP_Text level2BestTimeText;

    void Start()
    {
        LoadLevel1Scores();
        LoadLevel2Scores();
    }

    private void LoadLevel1Scores()
    {
        int score = PlayerPrefs.GetInt("HighScore_Level1", 0);
        if (level1HighScoreText != null)
            level1HighScoreText.text = "High Score: " + score.ToString("D6");

        if (level1BestTimeText != null)
        {
            if (PlayerPrefs.HasKey("BestTime_Level1"))
            {
                float t = PlayerPrefs.GetFloat("BestTime_Level1");
                level1BestTimeText.text = "Best Time: " + GameManager.FormatTime(t);
            }
            else
            {
                level1BestTimeText.text = "Best Time: 00:00:00";
            }
        }
    }

    private void LoadLevel2Scores()
    {
        int score = PlayerPrefs.GetInt("HighScore_Level2", 0);
        if (level2HighScoreText != null)
            level2HighScoreText.text = "High Score: " + score.ToString("D6");

        if (level2BestTimeText != null)
        {
            if (PlayerPrefs.HasKey("BestTime_Level2"))
            {
                float t = PlayerPrefs.GetFloat("BestTime_Level2");
                level2BestTimeText.text = "Best Time: " + GameManager.FormatTime(t);
            }
            else
            {
                level2BestTimeText.text = "Best Time: 00:00:00";
            }
        }
    }

    // ---------------------------------------------------------------
    // Button handlers
    // ---------------------------------------------------------------
    public void PlayLevel1()
    {
        SceneManager.LoadScene("Recreated Scene");
    }

    public void PlayLevel2()
    {
      
        Debug.Log("level 2 not implemented as of yet");
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("StartScene");
    }
    
}