using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private GameObject losePanel;

    private float currentHealth;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("PLAYER DEAD");

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
    }
}
