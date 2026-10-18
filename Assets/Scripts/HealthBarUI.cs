using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image healthFillImage;
    public Color fullColor = new Color(0.32f, 0.82f, 0.42f);
    public Color lowColor = new Color(0.9f, 0.25f, 0.25f);

    private Health playerHealth;
    private float shownFill = 1f;

    private void Start()
    {
        if (healthFillImage == null)
        {
            GameObject fill = GameObject.Find("HealthBarFill");
            if (fill != null) healthFillImage = fill.GetComponent<Image>();
        }
        if (healthFillImage != null)
        {
            // fillAmount only has an effect when the Image is in Filled mode
            healthFillImage.type = Image.Type.Filled;
            healthFillImage.fillMethod = Image.FillMethod.Horizontal;
            healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerHealth = player.GetComponent<Health>();
        else Debug.LogWarning("HealthBarUI: no GameObject tagged 'Player' in the scene.");
    }

    private void Update()
    {
        if (playerHealth == null || healthFillImage == null) return;

        float target = playerHealth.maxHealth > 0 ? (float)playerHealth.GetCurrentHealth() / playerHealth.maxHealth : 0f;
        shownFill = Mathf.MoveTowards(shownFill, target, Time.unscaledDeltaTime * 2f); // slide instead of jumping
        healthFillImage.fillAmount = shownFill;
        healthFillImage.color = Color.Lerp(lowColor, fullColor, shownFill);
    }
}
