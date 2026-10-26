using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void Playlevelone()
    {
        // Load the main game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Recreated Scene");
    }
    public void Playleveltwo()
    {
        // Load the main game scene
        Debug.Log("Level 2 not implemented as of yet");
    }
}
