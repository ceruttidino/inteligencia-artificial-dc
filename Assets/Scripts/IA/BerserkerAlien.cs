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
    [SerializeField] private Animator animator;

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

    [Header("Pathfinding")]
    [SerializeField] private EnemyFollowPath pathFollower;

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

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (pathFollower == null)
            pathFollower = GetComponent<EnemyFollowPath>();
    }

    private void Start()
    {
        if (player != null)
            lastPlayerPosition = player.position;

        EnterWander();
    }

    private void Update()
    {
        if (player == null)
            return;

        UpdatePlayerVelocity();

        switch (currentState)
        {
            case State.Wander:
                SetAnimationSpeed(wanderSpeed);
                Wander();
                break;

            case State.Pursue:
                SetAnimationSpeed(pursueSpeed);
                Pursue();
                break;

            case State.Attack:
                SetAnimationSpeed(0f);
                Attack();
                break;
        }
    }

    private void EnterWander()
    {
        currentState = State.Wander;

        if (pathFollower != null)
        {
            pathFollower.Stop();
            pathFollower.enabled = false;
        }

        PickNewWanderTarget();
    }

    private void EnterPursue()
    {
        currentState = State.Pursue;

        if (pathFollower != null)
        {
            pathFollower.enabled = true;
            pathFollower.SetSpeed(pursueSpeed);
            pathFollower.SetTarget(player);
        }
    }

    private void EnterAttack()
    {
        currentState = State.Attack;

        if (pathFollower != null)
        {
            pathFollower.Stop();
            pathFollower.enabled = false;
        }
    }

    private void UpdatePlayerVelocity()
    {
        if (Time.deltaTime <= 0f)
            return;

        playerVelocity = (player.position - lastPlayerPosition) / Time.deltaTime;
        lastPlayerPosition = player.position;
    }

    private void Wander()
    {
        if (CanSeePlayer())
        {
            EnterPursue();
            return;
        }

        wanderTimer -= Time.deltaTime;

        float distanceToTarget = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(wanderTarget.x, 0f, wanderTarget.z)
        );

        if (wanderTimer <= 0f || distanceToTarget < 1f)
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
        float distance = Vector3.Distance(transform.position, player.position);

        if (pathFollower == null)
        {
            Vector3 predictedPosition = player.position + playerVelocity * predictionTime;
            MoveTo(predictedPosition, pursueSpeed);
        }

        if (distance <= attackRange)
        {
            EnterAttack();
            return;
        }

        if (distance > detectionRange + 8f)
        {
            EnterWander();
        }
    }

    private void Attack()
    {
        LookAtPlayer();

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            EnterPursue();
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            if (animator != null)
                animator.SetTrigger("Attack");

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

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

        if (Physics.Raycast(
            rayOrigin,
            rayDirection.normalized,
            rayDirection.magnitude,
            obstacleLayer))
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
        {
            SetAnimationSpeed(0f);
            return;
        }

        direction.Normalize();

        if (controller != null)
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

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void SetAnimationSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }
}