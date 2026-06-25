using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Drag your background MP3 clip here!")]
    public AudioClip backgroundMusicClip;

    [Range(0f, 1f)]
    public float volume = 0.5f;
    public bool playOnAwake = true;
    public bool loop = true;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Configure the audio source properties
        audioSource.clip = backgroundMusicClip;
        audioSource.volume = volume;
        audioSource.playOnAwake = playOnAwake;
        audioSource.loop = loop;

        // If it's set to play immediately, fire it off
        if (playOnAwake && backgroundMusicClip != null)
        {
            audioSource.Play();
        }
    }

    // Optional helper function to change tracks dynamically later if needed
    public void ChangeMusicTrack(AudioClip newClip)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            backgroundMusicClip = newClip;
            audioSource.clip = newClip;
            audioSource.Play();
        }
    }
}
