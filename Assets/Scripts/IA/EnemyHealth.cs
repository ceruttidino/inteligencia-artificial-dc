using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private DamageFlash damageFlash;

    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        ParasiteAlien parasite = GetComponent<ParasiteAlien>();

        if (parasite != null)
        {
            parasite.TriggerFlee();
        }

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        Debug.Log(gameObject.name + " HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " DEAD");
        Destroy(gameObject);
    }
}
