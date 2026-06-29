using System.Collections.Generic;
using UnityEngine;

public class EnemyFollowPath : MonoBehaviour
{
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private Transform target;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float pathUpdateInterval = 0.4f;
    [SerializeField] private float nodeReachDistance = 0.7f;
    [SerializeField] private float destinationChangeThreshold = 0.5f;

    private CharacterController controller;
    private List<Vector3> currentPath;
    private int currentNodeIndex;
    private float updateTimer;

    private bool useFixedDestination;
    private Vector3 fixedDestination;

    private Vector3 lastDestination;
    private bool hasLastDestination;

    public bool HasPath
    {
        get
        {
            return currentPath != null &&
                   currentPath.Count > 0 &&
                   currentNodeIndex < currentPath.Count;
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        updateTimer = 0f;
        currentNodeIndex = 0;
        hasLastDestination = false;
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

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetTarget(Transform newTarget)
    {
        if (target == newTarget && !useFixedDestination)
            return;

        target = newTarget;
        useFixedDestination = false;
        hasLastDestination = false;
        updateTimer = 0f;
    }

    public void SetDestination(Vector3 destination)
    {
        if (useFixedDestination &&
            Vector3.Distance(fixedDestination, destination) < destinationChangeThreshold)
        {
            return;
        }

        fixedDestination = destination;
        useFixedDestination = true;
        hasLastDestination = false;
        updateTimer = 0f;
    }

    public void Stop()
    {
        currentPath = null;
        currentNodeIndex = 0;
        hasLastDestination = false;
    }

    private void UpdatePath()
    {
        if (pathfinder == null)
            return;

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

        if (hasLastDestination &&
            Vector3.Distance(lastDestination, destination) < destinationChangeThreshold)
        {
            return;
        }

        currentPath = pathfinder.FindPath(transform.position, destination);
        currentNodeIndex = 0;

        lastDestination = destination;
        hasLastDestination = true;

        SkipReachedNodes();
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        SkipReachedNodes();

        if (currentNodeIndex >= currentPath.Count)
            return;

        Vector3 targetNode = currentPath[currentNodeIndex];

        Vector3 direction = targetNode - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
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

    private void SkipReachedNodes()
    {
        if (currentPath == null)
            return;

        while (currentNodeIndex < currentPath.Count)
        {
            Vector3 nodePosition = currentPath[currentNodeIndex];

            float distance = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(nodePosition.x, 0f, nodePosition.z)
            );

            if (distance > nodeReachDistance)
                break;

            currentNodeIndex++;
        }
    }
}