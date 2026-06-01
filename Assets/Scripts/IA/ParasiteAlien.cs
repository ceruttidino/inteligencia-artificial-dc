using UnityEngine;

public class ParasiteAlien : MonoBehaviour
{
    private enum State
    {
        Hide,
        Ambush,
        Flee
    }

    [SerializeField] private Transform player;
    [SerializeField] private Transform[] hidePoints;

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float fleeSpeed = 8f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float fleeDuration = 3f;

    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float damage = 8f;
    [SerializeField] private float attackCooldown = 0.8f;

    private float attackTimer;

    private CharacterController controller;
    private State currentState;

    private int currentHideIndex;
    private float fleeTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        currentState = State.Hide;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Hide:
                Hide();
                break;

            case State.Ambush:
                Ambush();
                break;

            case State.Flee:
                Flee();
                break;
        }
    }

    private void Hide()
    {
        MoveTo(
            hidePoints[currentHideIndex].position,
            moveSpeed
        );

        if (Vector3.Distance(
            transform.position,
            player.position)
            <= detectionRange)
        {
            currentState = State.Ambush;
        }
    }

    private void Ambush()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            MoveTo(player.position, moveSpeed);
            return;
        }

        LookAtDirection(player.position - transform.position);

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            attackTimer = attackCooldown;
        }
    }

    private void Flee()
    {
        fleeTimer -= Time.deltaTime;

        Vector3 fleeDirection = transform.position - player.position;
        fleeDirection.y = 0f;
        fleeDirection.Normalize();

        controller.Move(fleeDirection * fleeSpeed * Time.deltaTime);

        LookAtDirection(fleeDirection);

        if (fleeTimer <= 0f)
        {
            currentState = State.Hide;

            currentHideIndex = Random.Range(0, hidePoints.Length);
        }
    }

    private void LookAtDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            10f * Time.deltaTime
        );
    }

    public void TriggerFlee()
    {
        fleeTimer = fleeDuration;

        currentState = State.Flee;
    }

    private void MoveTo(Vector3 target, float speed)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.magnitude < 0.1f)
            return;

        direction.Normalize();

        controller.Move(direction * speed * Time.deltaTime);

        LookAtDirection(direction);
    }
}
