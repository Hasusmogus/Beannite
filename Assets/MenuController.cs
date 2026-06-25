using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    [SerializeField] private string gameplaySceneName = "GameScene"; // Replace with your actual game scene name

    public void StartSingleplayer()
    {
        // Loads the main gameplay scene
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void StartMultiplayer()
    {
        // Does nothing for now, perfect for hooking up Photon/Mirror later
        Debug.Log("Multiplayer clicked! (Not implemented yet)");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game requested.");
        Application.Quit(); // Closes the built game application
    }
}
