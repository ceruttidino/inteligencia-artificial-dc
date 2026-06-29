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
    [SerializeField] private Animator animator;

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float fleeSpeed = 8f;
    [SerializeField] private float detectionRange = 12f;

    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float damage = 8f;
    [SerializeField] private float attackCooldown = 0.8f;

    [SerializeField] private float reachedHideDistance = 0.6f;
    [SerializeField] private float sameHidePenalty = 8f;

    private CharacterController controller;
    private State currentState;

    private int currentHideIndex = -1;
    private float attackTimer;

    [SerializeField] private EnemyFollowPath pathFollower;

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
        ChooseBestHidePoint();
        EnterHide();
    }

    private void Update()
    {
        if (player == null || hidePoints == null || hidePoints.Length == 0)
            return;

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
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            EnterAmbush();
            return;
        }

        SetAnimationSpeed(0f);
    }

    private void Ambush()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            SetAnimationSpeed(moveSpeed);

            if (pathFollower != null)
            {
                pathFollower.enabled = true;
                pathFollower.SetSpeed(moveSpeed);
                pathFollower.SetTarget(player);
            }
            else
            {
                MoveTo(player.position, moveSpeed);
            }

            return;
        }

        if (pathFollower != null)
        {
            pathFollower.Stop();
            pathFollower.enabled = false;
        }

        SetAnimationSpeed(0f);
        LookAtDirection(player.position - transform.position);

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

    private void Flee()
    {
        SetAnimationSpeed(fleeSpeed);

        Transform hidePoint = hidePoints[currentHideIndex];

        if (pathFollower == null)
            MoveTo(hidePoint.position, fleeSpeed);

        float distanceToHide = Vector3.Distance(transform.position, hidePoint.position);

        if (distanceToHide <= reachedHideDistance)
        {
            EnterHide();
        }
    }

    public void TriggerFlee()
    {
        EnterFlee();
    }

    private void ChooseBestHidePoint()
    {
        int bestIndex = 0;
        float bestScore = float.MinValue;

        for (int i = 0; i < hidePoints.Length; i++)
        {
            if (hidePoints[i] == null)
                continue;

            float distanceFromPlayer = Vector3.Distance(hidePoints[i].position, player.position);
            float distanceFromAlien = Vector3.Distance(hidePoints[i].position, transform.position);

            float score = distanceFromPlayer - distanceFromAlien * 0.35f;

            if (i == currentHideIndex && hidePoints.Length > 1)
                score -= sameHidePenalty;

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        currentHideIndex = bestIndex;
    }

    private void MoveTo(Vector3 target, float speed)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.magnitude < 0.1f)
        {
            SetAnimationSpeed(0f);
            return;
        }

        direction.Normalize();

        controller.Move(direction * speed * Time.deltaTime);

        LookAtDirection(direction);
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

    private void SetAnimationSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }

    private void EnterHide()
    {
        currentState = State.Hide;

        if (pathFollower != null)
        {
            pathFollower.Stop();
            pathFollower.enabled = false;
        }

        SetAnimationSpeed(0f);
    }

    private void EnterAmbush()
    {
        currentState = State.Ambush;

        if (pathFollower != null)
        {
            pathFollower.enabled = true;
            pathFollower.SetSpeed(moveSpeed);
            pathFollower.SetTarget(player);
        }
    }

    private void EnterFlee()
    {
        ChooseBestHidePoint();

        currentState = State.Flee;

        if (pathFollower != null)
        {
            pathFollower.enabled = true;
            pathFollower.SetSpeed(fleeSpeed);
            pathFollower.SetDestination(hidePoints[currentHideIndex].position);
        }
    }
}