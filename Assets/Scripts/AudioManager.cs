using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource introSource;
    public AudioSource normalSource;
    public AudioSource scaredSource;
    public AudioSource deadSource;

    public void PlayIntro()
    {
        if (introSource != null) introSource.Play();
    }

    public void PlayNormal()
    {
        if (introSource != null) introSource.Stop();
        if (scaredSource != null) scaredSource.Stop();
        if (deadSource != null) deadSource.Stop();
        if (normalSource != null && !normalSource.isPlaying) normalSource.Play();
    }

    public void PlayScared()
    {
        if (introSource != null) introSource.Stop();
        if (normalSource != null) normalSource.Stop();
        if (deadSource != null) deadSource.Stop();

        if (scaredSource != null && !scaredSource.isPlaying)
            scaredSource.Play();
    }

    public void PlayDead()
    {
        if (introSource != null) introSource.Stop();
        if (normalSource != null) normalSource.Stop();
        if (scaredSource != null) scaredSource.Stop();

        if (deadSource != null && !deadSource.isPlaying)
            deadSource.Play();
    }
}