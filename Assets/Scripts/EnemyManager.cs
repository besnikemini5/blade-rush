using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public GameObject levelUpPanel;

    [Header("Waves")]
    [Tooltip("Wave N spawns N x this many enemies")]
    public int enemiesPerWave = 3;
    public float spawnInterval = 0.6f;
    [Tooltip("Enemies get +1 health every this many waves")]
    public int extraHealthEveryWaves = 2;
    public float speedIncreasePerWave = 0.15f;
    [Tooltip("Also spawn from the mirrored position on the other side of the screen")]
    public bool mirrorSpawnPoints = true;

    public int CurrentWave { get { return currentWave; } }

    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private int currentWave = 1;
    private bool spawning;
    private bool clearing;
    private Health playerHealth;
    private TextMeshProUGUI waveText;
    private TextMeshProUGUI clearedText;

    private void Start()
    {
        if (levelUpPanel != null)
        {
            foreach (TextMeshProUGUI text in levelUpPanel.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (text.text.ToLower().Contains("cleared")) clearedText = text;
            }
            levelUpPanel.SetActive(false);
        }

        // Enemies pass through each other instead of piling into a tower
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0) Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerHealth = player.GetComponent<Health>();

        CreateWaveLabel();
        StartWave();
    }

    private void Update()
    {
        if (levelUpPanel != null && levelUpPanel.activeSelf &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            ContinueToNextWave();
        }
    }

    private void StartWave()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemyManager: no enemy prefabs assigned in the Inspector.");
            return;
        }
        activeEnemies.Clear();
        if (waveText != null) waveText.text = "Wave " + currentWave;
        StartCoroutine(SpawnWave(currentWave * enemiesPerWave));
    }

    // Enemies arrive one after another instead of all at once on the same spot
    private IEnumerator SpawnWave(int count)
    {
        spawning = true;
        List<Vector3> positions = GetSpawnPositions();
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if (prefab != null)
            {
                Vector3 position = positions[i % positions.Count] + new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);
                GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
                ApplyWaveDifficulty(enemy);
                activeEnemies.Add(enemy);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        spawning = false;
        CheckWaveCleared();
    }

    private List<Vector3> GetSpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        float centerX = Camera.main != null ? Camera.main.transform.position.x : 0f;

        if (spawnPoints != null)
        {
            foreach (Transform point in spawnPoints)
            {
                if (point == null) continue;
                positions.Add(point.position);
                if (mirrorSpawnPoints)
                {
                    Vector3 p = point.position;
                    positions.Add(new Vector3(2f * centerX - p.x, p.y, p.z));
                }
            }
        }
        if (positions.Count == 0) positions.Add(transform.position); // fallback so a wave still starts
        return positions;
    }

    private void ApplyWaveDifficulty(GameObject enemy)
    {
        int extraHealth = extraHealthEveryWaves > 0 ? (currentWave - 1) / extraHealthEveryWaves : 0;
        Health health = enemy.GetComponent<Health>();
        if (health != null && extraHealth > 0) health.SetMaxHealth(health.maxHealth + extraHealth);

        EnemyFollow follow = enemy.GetComponent<EnemyFollow>();
        if (follow != null) follow.moveSpeed += speedIncreasePerWave * (currentWave - 1);
    }

    public void OnEnemyKilled(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        Sfx.Play("enemy_die", 0.7f);
        CheckWaveCleared();
    }

    private void CheckWaveCleared()
    {
        activeEnemies.RemoveAll(e => e == null);
        if (spawning || clearing || activeEnemies.Count > 0) return;
        clearing = true;
        StartCoroutine(LevelCleared());
    }

    private IEnumerator LevelCleared()
    {
        yield return new WaitForSeconds(1f); // let the last enemy's death animation finish
        if (playerHealth != null && playerHealth.IsDead) yield break;

        if (clearedText != null) clearedText.text = "Wave " + currentWave + " cleared!";
        Sfx.Play("wave_clear");
        if (levelUpPanel != null) levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ContinueToNextWave()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
        clearing = false;
        currentWave++;
        if (playerHealth != null) playerHealth.Heal(1); // small reward between waves
        StartWave();
    }

    // Adds a "Wave N" label under the death counter (copies its font and style)
    private void CreateWaveLabel()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        TextMeshProUGUI template = gameManager != null ? gameManager.deathCounterText : null;
        if (template == null)
        {
            GameObject found = GameObject.Find("DeathCounterText");
            if (found != null) template = found.GetComponent<TextMeshProUGUI>();
        }
        if (template == null) return;

        waveText = Instantiate(template, template.transform.parent);
        waveText.name = "WaveText";
        RectTransform rect = waveText.rectTransform;
        RectTransform source = template.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, source.anchoredPosition.y);
        waveText.alignment = TextAlignmentOptions.Center;
    }
}
