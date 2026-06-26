using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject victoryScreenUI;

    [Header("Tracking Settings")]
    [Tooltip("How often (in seconds) the script checks the scene for remaining enemies.")]
    public float checkInterval = 0.5f;
    
    private float nextCheckTime = 0f;
    private bool gameWon = false;

    void Update()
    {
        if (gameWon) return;

        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            CheckRemainingEnemies();
        }
    }

    private void CheckRemainingEnemies()
    {
        // --- FIXED: Removed FindObjectsSortMode parameter to resolve deprecation warning ---
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

        if (victoryScreenUI != null)
        {
            victoryScreenUI.SetActive(true);
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