using UnityEngine;

public class BerserkerAlien : MonoBehaviour
{
    private enum State
    {
        Wander,
        Pursue,
        Attack
    }

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float wanderSpeed = 2.5f;
    [SerializeField] private float pursueSpeed = 5.5f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float wanderChangeTime = 2f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 14f;
    [SerializeField] private float viewAngle = 110f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Pursue")]
    [SerializeField] private float predictionTime = 0.6f;

    private CharacterController controller;
    private State currentState;

    private Vector3 wanderTarget;
    private float wanderTimer;
    private float attackTimer;

    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        currentState = State.Wander;
        PickNewWanderTarget();
        lastPlayerPosition = player.position;
    }

    private void Update()
    {
        UpdatePlayerVelocity();

        switch (currentState)
        {
            case State.Wander:
                Wander();
                if (CanSeePlayer())
                    currentState = State.Pursue;
                break;

            case State.Pursue:
                Pursue();
                break;

            case State.Attack:
                Attack();
                break;
        }
    }

    private void UpdatePlayerVelocity()
    {
        playerVelocity = (player.position - lastPlayerPosition) / Time.deltaTime;
        lastPlayerPosition = player.position;
    }

    private void Wander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || Vector3.Distance(transform.position, wanderTarget) < 1f)
        {
            PickNewWanderTarget();
        }

        MoveTo(wanderTarget, wanderSpeed);
    }

    private void PickNewWanderTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;

        wanderTarget = new Vector3(
            transform.position.x + randomCircle.x,
            transform.position.y,
            transform.position.z + randomCircle.y
        );

        wanderTimer = wanderChangeTime;
    }

    private void Pursue()
    {
        Vector3 predictedPosition = player.position + playerVelocity * predictionTime;

        MoveTo(predictedPosition, pursueSpeed);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            currentState = State.Attack;
            return;
        }

        if (distance > detectionRange + 8f)
        {
            currentState = State.Wander;
        }
    }

    private void Attack()
    {
        LookAtPlayer();

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            currentState = State.Pursue;
            return;
        }

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

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange)
            return false;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer > viewAngle * 0.5f)
            return false;

        Vector3 rayOrigin = transform.position + Vector3.up * 1.2f;
        Vector3 rayTarget = player.position + Vector3.up * 1.2f;
        Vector3 rayDirection = rayTarget - rayOrigin;

        if (Physics.Raycast(rayOrigin, rayDirection.normalized, detectionRange, obstacleLayer))
        {
            return false;
        }

        return true;
    }

    private void MoveTo(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.magnitude < 0.1f)
            return;

        direction.Normalize();

        controller.Move(direction * speed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
