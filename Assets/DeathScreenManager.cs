using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreenManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject deathScreenUI;

    public void ShowDeathScreen()
    {
        // Turn on the UI overlay
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(true);
        }

        // Unlock and show the mouse cursor so the player can click the buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pause game time if you want everything to freeze upon death
        Time.timeScale = 0f; 
    }

    public void RetryGame()
    {
        // Unpause game time before loading
        Time.timeScale = 1f; 

        // Reloads the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        // Closes the built application
        Application.Quit(); 
    }
}