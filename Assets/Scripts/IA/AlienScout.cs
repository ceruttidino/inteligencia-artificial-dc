using UnityEngine;

public class AlienScout : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Search,
        Attack
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 2f;

    [Header("Line Of Sight")]
    [SerializeField] private float viewAngle = 70f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attack")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Pathfinding")]
    [SerializeField] private EnemyFollowPath pathFollower;

    [Header("Search")]
    [SerializeField] private float searchDuration = 4f;

    private Vector3 lastKnownPlayerPosition;
    private float searchTimer;

    private float attackTimer;

    private CharacterController controller;
    private State currentState;

    private int currentPatrolIndex;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        currentState = State.Patrol;
        pathFollower.enabled = false;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                SetAnimationSpeed(patrolSpeed);
                Patrol();
                DetectPlayer();
                break;

            case State.Chase:
                SetAnimationSpeed(chaseSpeed);
                Chase();
                break;

            case State.Attack:
                SetAnimationSpeed(0f);
                Attack();
                break;

            case State.Search:
                SetAnimationSpeed(chaseSpeed);
                Search();
                break;
        }
    }

    private void Patrol()
    {
        pathFollower.enabled = false;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            SetAnimationSpeed(0f);
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        Vector3 targetPosition = targetPoint.position;
        targetPosition.y = transform.position.y;

        MoveTo(targetPosition, patrolSpeed);

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(targetPosition.x, 0, targetPosition.z)
        );

        if (distance < 1.2f)
        {
            currentPatrolIndex++;

            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = 0;
            }

        }
    }

    private void DetectPlayer()
    {
        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;

            pathFollower.enabled = true;
            pathFollower.SetTarget(player);

            currentState = State.Chase;
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

        if (Physics.Raycast(rayOrigin, rayDirection.normalized, out RaycastHit hit, detectionRange, obstacleLayer))
        {
            return false;
        }

        return true;
    }

    private void Chase()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
        }
        else
        {
            searchTimer = searchDuration;

            pathFollower.enabled = true;
            pathFollower.SetDestination(lastKnownPlayerPosition);

            currentState = State.Search;
            return;
        }

        if (distance <= attackRange)
        {
            pathFollower.enabled = false;
            currentState = State.Attack;
        }
    }

    private void Search()
    {
        searchTimer -= Time.deltaTime;

        if (CanSeePlayer())
        {
            pathFollower.enabled = true;
            pathFollower.SetTarget(player);

            currentState = State.Chase;
            return;
        }

        float distanceToLastPosition = Vector3.Distance(
            transform.position,
            lastKnownPlayerPosition
        );

        if (distanceToLastPosition < 1.5f || searchTimer <= 0f)
        {
            pathFollower.enabled = false;
            currentState = State.Patrol;
        }
    }

    private void Attack()
    {
        LookAtPlayer();

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            pathFollower.enabled = true;
            pathFollower.SetTarget(player);

            currentState = State.Chase;
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            attackTimer = attackCooldown;
        }
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

        controller.Move(direction * speed * Time.deltaTime);

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
