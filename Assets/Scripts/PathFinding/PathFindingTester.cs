using System.Collections.Generic;
using UnityEngine;

public class PathfindingTester : MonoBehaviour
{
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            List<Vector3> path = pathfinder.FindPath(
                startPoint.position,
                endPoint.position
            );

            gridManager.debugPath = path;
        }
    }
}
