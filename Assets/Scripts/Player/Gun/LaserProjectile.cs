using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody rb;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Init(Vector3 direction)
    {
        if (rb == null)
        {
            Debug.LogError("El proyectil no tiene Rigidbody");
            return;
        }

        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            return;

        if (hasHit) return;
        hasHit = true;

        EnemyHealth enemy = collision.gameObject.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
