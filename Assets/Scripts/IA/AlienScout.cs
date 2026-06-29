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

        if (pathFollower == null)
            pathFollower = GetComponent<EnemyFollowPath>();
    }

    private void Start()
    {
        EnterPatrol();
    }

    private void Update()
    {
        if (player == null)
            return;

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

            case State.Search:
                SetAnimationSpeed(chaseSpeed);
                Search();
                break;

            case State.Attack:
                SetAnimationSpeed(0f);
                Attack();
                break;
        }
    }

    private void EnterPatrol()
    {
        currentState = State.Patrol;

        if (pathFollower != null)
            pathFollower.enabled = false;
    }

    private void EnterChase()
    {
        currentState = State.Chase;

        if (pathFollower != null)
        {
            pathFollower.enabled = true;
            pathFollower.SetSpeed(chaseSpeed);
            pathFollower.SetTarget(player);
        }
    }

    private void EnterSearch()
    {
        currentState = State.Search;
        searchTimer = searchDuration;

        if (pathFollower != null)
        {
            pathFollower.enabled = true;
            pathFollower.SetSpeed(chaseSpeed);
            pathFollower.SetDestination(lastKnownPlayerPosition);
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

    private void Patrol()
    {
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
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(targetPosition.x, 0f, targetPosition.z)
        );

        if (distance < 1.2f)
        {
            currentPatrolIndex++;

            if (currentPatrolIndex >= patrolPoints.Length)
                currentPatrolIndex = 0;
        }
    }

    private void DetectPlayer()
    {
        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            EnterChase();
        }
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
            EnterSearch();
            return;
        }

        if (distance <= attackRange)
        {
            EnterAttack();
        }
    }

    private void Search()
    {
        searchTimer -= Time.deltaTime;

        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            EnterChase();
            return;
        }

        float distanceToLastPosition = Vector3.Distance(
            transform.position,
            lastKnownPlayerPosition
        );

        if (distanceToLastPosition < 1.5f || searchTimer <= 0f)
        {
            EnterPatrol();
        }
    }

    private void Attack()
    {
        LookAtPlayer();

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            EnterChase();
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