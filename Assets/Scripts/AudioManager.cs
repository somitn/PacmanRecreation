using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource introSource;
    public AudioSource normalSource;

    [Header("Settings")]
    public float introMaxDuration = 3f;

    private float timer = 0f;
    private bool switched = false;

    void Start()
    {
        if (introSource != null)
        {
            introSource.Play();
        }
    }

    void Update()
    {
        if (switched) return;

        timer += Time.deltaTime;

        bool introFinished = !introSource.isPlaying;
        bool timeUp = timer >= introMaxDuration;

        if (introFinished || timeUp)
        {
            introSource.Stop();
            normalSource.Play();
            switched = true;
        }
    }
}