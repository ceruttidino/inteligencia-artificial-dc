using System.Collections.Generic;
using UnityEngine;

public class EnemyFollowPath : MonoBehaviour
{
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private Transform target;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float pathUpdateInterval = 0.4f;

    private CharacterController controller;
    private List<Vector3> currentPath;
    private int currentNodeIndex;
    private float updateTimer;

    private bool useFixedDestination;
    private Vector3 fixedDestination;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        updateTimer = 0f;
        currentNodeIndex = 0;
    }

    private void Update()
    {
        updateTimer -= Time.deltaTime;

        if (updateTimer <= 0f)
        {
            UpdatePath();
            updateTimer = pathUpdateInterval;
        }

        FollowPath();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        useFixedDestination = false;
        updateTimer = 0f;
    }

    public void SetDestination(Vector3 destination)
    {
        fixedDestination = destination;
        useFixedDestination = true;
        updateTimer = 0f;
    }

    private void UpdatePath()
    {
        Vector3 destination;

        if (useFixedDestination)
        {
            destination = fixedDestination;
        }
        else
        {
            if (target == null)
                return;

            destination = target.position;
        }

        currentPath = pathfinder.FindPath(transform.position, destination);
        currentNodeIndex = 0;
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        if (currentNodeIndex >= currentPath.Count)
            return;

        Vector3 targetNode = currentPath[currentNodeIndex];

        Vector3 direction = targetNode - transform.position;
        direction.y = 0f;

        if (direction.magnitude < 0.5f)
        {
            currentNodeIndex++;
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
}
