using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Main Level";

    public void PlayGame()
    {
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT GAME");
        Application.Quit();
    }
}