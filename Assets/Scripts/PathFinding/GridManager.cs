using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public LayerMask obstacleLayer;
    public List<Vector3> debugPath;

    public Vector2 gridWorldSize = new Vector2(50, 50);
    public float nodeRadius = 1f;

    private GridNode[,] grid;

    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2f;

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new GridNode[gridSizeX, gridSizeY];

        Vector3 worldBottomLeft =
            transform.position
            - Vector3.right * gridWorldSize.x / 2f
            - Vector3.forward * gridWorldSize.y / 2f;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint =
                    worldBottomLeft
                    + Vector3.right * (x * nodeDiameter + nodeRadius)
                    + Vector3.forward * (y * nodeDiameter + nodeRadius);

                bool walkable = !Physics.CheckSphere(
                    worldPoint,
                    nodeRadius,
                    obstacleLayer
                );

                grid[x, y] = new GridNode(
                    walkable,
                    worldPoint,
                    x,
                    y
                );
            }
        }
    }

    public GridNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX =
            (worldPosition.x - transform.position.x + gridWorldSize.x / 2f)
            / gridWorldSize.x;

        float percentY =
            (worldPosition.z - transform.position.z + gridWorldSize.y / 2f)
            / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX < 0 || checkX >= gridSizeX ||
                    checkY < 0 || checkY >= gridSizeY)
                {
                    continue;
                }

                if (x != 0 && y != 0)
                {
                    if (!grid[node.gridX + x, node.gridY].walkable ||
                        !grid[node.gridX, node.gridY + y].walkable)
                    {
                        continue;
                    }
                }

                neighbours.Add(grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    public void ResetNodes()
    {
        foreach (GridNode node in grid)
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.parent = null;
        }
    }

    public GridNode GetClosestWalkableNode(GridNode originNode)
    {
        GridNode closestNode = null;
        int closestDistance = int.MaxValue;

        foreach (GridNode node in grid)
        {
            if (!node.walkable)
                continue;

            int distance = Mathf.Abs(originNode.gridX - node.gridX) +
                           Mathf.Abs(originNode.gridY - node.gridY);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    private void OnDrawGizmos()
    {
        if (grid == null)
            return;

        foreach (GridNode node in grid)
        {
            Gizmos.color = node.walkable ? Color.green : Color.red;

            Gizmos.DrawCube(
                node.worldPosition + Vector3.up * 0.2f,
                Vector3.one * (nodeDiameter - 0.1f)
            );
        }

        if (debugPath != null)
        {
            Gizmos.color = Color.cyan;

            foreach (Vector3 point in debugPath)
            {
                Gizmos.DrawSphere(point + Vector3.up * 0.6f, 0.3f);
            }
        }
    }
}