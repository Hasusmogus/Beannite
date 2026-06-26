using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Leave blank to automatically grab the first child panel of this canvas.")]
    public GameObject victoryScreenUI;

    [Header("Tracking Settings")]
    public float checkInterval = 0.5f;
    
    [Header("Audio Settings")]
    [Tooltip("The main background music AudioSource in your scene.")]
    public AudioSource backgroundMusicSource;
    [Tooltip("The track that plays when the player wins.")]
    public AudioClip victoryMusicClip;

    private float nextCheckTime = 0f;
    private bool gameWon = false;

    void Awake()
    {
        // Fallback layout check
        if (victoryScreenUI == null && transform.childCount > 0)
        {
            victoryScreenUI = transform.GetChild(0).gameObject;
        }
    }

    void Update()
    {
        if (gameWon) return;

        if (Time.unscaledTime >= nextCheckTime)
        {
            nextCheckTime = Time.unscaledTime + checkInterval;
            CheckRemainingEnemies();
        }
    }

    private void CheckRemainingEnemies()
    {
        EntityHealth[] allEntities = FindObjectsByType<EntityHealth>(FindObjectsInactive.Exclude);
        int activeEnemiesCount = 0;

        foreach (EntityHealth entity in allEntities)
        {
            if (entity.isTemplateForGeneration || entity.isPlayer || entity.isDead) 
                continue;

            activeEnemiesCount++;
        }

        if (activeEnemiesCount == 0)
        {
            if (Time.timeSinceLevelLoad > 2f) 
            {
                TriggerVictory();
            }
        }
    }

    private void TriggerVictory()
    {
        gameWon = true;

        // Force time scale normal briefly to allow engine processes to process audio/UI activation safely
        Time.timeScale = 1f; 

        // Swap to victory soundscape track
        if (backgroundMusicSource != null && victoryMusicClip != null)
        {
            backgroundMusicSource.Stop();
            backgroundMusicSource.clip = victoryMusicClip;
            backgroundMusicSource.loop = false; 
            backgroundMusicSource.Play();
        }

        if (victoryScreenUI != null)
        {
            victoryScreenUI.SetActive(true);
            Debug.Log($"[VICTORY] Interface panel active: {victoryScreenUI.name}");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f; 
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game application...");
        Application.Quit(); 
    }
}