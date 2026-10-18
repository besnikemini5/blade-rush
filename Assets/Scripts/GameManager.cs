using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI deathCounterText;

    // Static so they survive the scene reload on Retry
    public static int deathCount = 0;
    public static int bestWave = 0;

    private bool isGameOver;
    private TextMeshProUGUI gameOverText;

    private void Start()
    {
        Time.timeScale = 1f; // in case the last run ended while the game was paused

        if (deathCounterText == null)
        {
            GameObject counter = GameObject.Find("DeathCounterText");
            if (counter != null) deathCounterText = counter.GetComponent<TextMeshProUGUI>();
        }
        if (gameOverPanel != null)
        {
            foreach (TextMeshProUGUI text in gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (text.text.ToLower().Contains("game over")) gameOverText = text;
            }
            gameOverPanel.SetActive(false);
        }
        UpdateStats();
    }

    private void Update()
    {
        if (isGameOver && (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return)))
        {
            RetryGame();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return; // a death is only counted once
        isGameOver = true;
        deathCount++;

        EnemyManager enemies = FindFirstObjectByType<EnemyManager>();
        int wave = enemies != null ? enemies.CurrentWave : 0;
        if (wave > bestWave) bestWave = wave;
        if (gameOverText != null)
        {
            gameOverText.text = "Game Over\n<size=50%>You reached wave " + wave + "  (R to retry)</size>";
        }

        UpdateStats();
        Sfx.Play("game_over");
        StartCoroutine(ShowGameOverPanel());
    }

    private IEnumerator ShowGameOverPanel()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateStats()
    {
        if (deathCounterText == null) return;
        deathCounterText.text = "Deaths: " + deathCount + (bestWave > 0 ? "\nBest wave: " + bestWave : "");
    }
}
