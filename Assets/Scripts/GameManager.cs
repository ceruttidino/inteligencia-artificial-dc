using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Enemies")]
    public List<GameObject> enemies = new List<GameObject>();

    [Header("Win Screen")]
    public GameObject winPanel;
    public string mainMenuSceneName = "MainMenu";

    private bool gameWon = false;

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    void Update()
    {
        if (gameWon) return;

        enemies.RemoveAll(enemy => enemy == null);

        if (enemies.Count == 0)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        gameWon = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}