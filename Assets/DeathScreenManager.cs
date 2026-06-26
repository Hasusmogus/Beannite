using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Leave blank to automatically grab the first child panel of this canvas.")]
    public GameObject deathScreenUI;

    [Header("Audio Settings")]
    [Tooltip("The main background music AudioSource in your scene.")]
    public AudioSource backgroundMusicSource;
    [Tooltip("The track that plays when the player dies.")]
    public AudioClip deathMusicClip;

    void Awake()
    {
        // Fallback layout check
        if (deathScreenUI == null && transform.childCount > 0)
        {
            deathScreenUI = transform.GetChild(0).gameObject;
        }
    }

    public void ShowDeathScreen()
    {
        // Swap to defeat soundscape track
        if (backgroundMusicSource != null && deathMusicClip != null)
        {
            backgroundMusicSource.Stop();
            backgroundMusicSource.clip = deathMusicClip;
            backgroundMusicSource.loop = false;
            backgroundMusicSource.Play();
        }

        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(true);
            Debug.Log($"[DEATH] Interface panel active: {deathScreenUI.name}");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f; 
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToDesktop()
    {
        Debug.Log("Quitting game application...");
        Application.Quit(); 
    }
}