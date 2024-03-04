using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using System.Threading.Tasks;
using Ekkam;
using Unity.VisualScripting;

public class Astar : MonoBehaviour
{
    [SerializeField] PathfindingGrid grid;
    PathfindingManager pathfindingManager;
    [SerializeField] Vector2Int startNodePosition;
    [SerializeField] public static Vector2Int endNodePosition;

    [SerializeField] Color startNodeColor = new Color(0, 0.5f, 0, 1);
    [SerializeField] Color endNodeColor = new Color(0.5f, 0, 0, 1);
    [SerializeField] Color pathNodeColor = new Color(0, 0, 0.5f, 1);
    
    public List<Node> neighbours = new List<Node>();
    public List<Node> openNodes = new List<Node>();
    public List<Node> closedNodes = new List<Node>();
    public List<Node> pathNodes = new List<Node>();
    private List<Node> pathNodesColored = new List<Node>();
    private Node[] allNodes;

    public float recalculationDistance = 4f;
    private float initialRecalculationDistance = 4f;

    public bool findPath;
    public bool assignedInitialTarget;

    Player player;

    void Start()
    {
        pathfindingManager = FindObjectOfType<PathfindingManager>();
        player = FindObjectOfType<Player>();
        initialRecalculationDistance = recalculationDistance;
        
        // startingNode.SetColor(startNodeColor);
        UpdateStartPosition(grid.GetPositionFromWorldPoint(transform.position));
        Node startingNode = grid.GetNode(startNodePosition);
        
        Node endingNode = grid.GetNode(endNodePosition);
        endingNode.SetColor(endNodeColor);
        
        // openNodes.Add(startingNode);
        
        GetNeighbours(startingNode, startNodePosition);
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space)) findPath = true;
        if (findPath) {
            FindPath();
        }

        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     bool los = HasDirectLineOfSight(startNodePosition, endNodePosition);
        //     print("Has direct line of sight: " + los);
        // }

        if (grid.ObjectIsOnGrid(player.transform.position))
        {
            if (!assignedInitialTarget)
            {
                UpdateTargetPosition(grid.GetPositionFromWorldPoint(player.transform.position));
                assignedInitialTarget = true;
            }

            Vector2Int playerGridPosition = grid.GetPositionFromWorldPoint(player.transform.position);
            if (GetDistance(grid.GetNode(playerGridPosition), grid.GetNode(endNodePosition)) > recalculationDistance && !findPath)
            {
                recalculationDistance = initialRecalculationDistance;
                UpdateTargetPosition(playerGridPosition);
            }
        }
        else
        {
            assignedInitialTarget = false;
            findPath = false;
        }

    }

    void FindPath()
    {
        if (openNodes.Count < 1)
        {
            print("No path found");
            findPath = false;
            pathfindingManager.needToFindPath.Remove(this);
            return;
        }
        var currentNode = openNodes[0];
        foreach (var node in openNodes)
        {
            if (node.FCost < currentNode.FCost)
            {
                currentNode = node;
            }
        }
        openNodes.Remove(currentNode);
        closedNodes.Add(currentNode);
        
        if (currentNode == grid.GetNode(endNodePosition))
        {
            print("Path found");
            findPath = false;
            pathfindingManager.needToFindPath.Remove(this);
            SetPathNodes();
            return;
        }
        
        var currentNeighbours = GetNeighbours(currentNode, currentNode.gridPosition);
        foreach (var neighbour in currentNeighbours)
        {
            var neighborGridPosition = neighbour.gridPosition;
            print("neighbour: " + neighborGridPosition);
            // check if it is in blocked positions or closed nodes
            if (neighbour.isBlocked || closedNodes.Contains(neighbour))
            {
                continue;
            }
            // check if new path to neighbour is shorter or neighbour is not in openNodes
            var newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
            if (newMovementCostToNeighbour < neighbour.GCost || !openNodes.Contains(neighbour))
            {
                neighbour.GCost = newMovementCostToNeighbour;
                neighbour.HCost = GetDistance(neighbour, grid.GetNode(endNodePosition));
                neighbour.Parent = currentNode;
                if (!openNodes.Contains(neighbour))
                {
                    openNodes.Add(neighbour);
                }
            }
        }
    }

    // Line of sight check based on Bresenham's line algorithm
    public bool HasDirectLineOfSight(Vector2Int start, Vector2Int end)
    {
        int x = start.x;
        int y = start.y;
        int dx = Math.Abs(end.x - start.x);
        int dy = Math.Abs(end.y - start.y);
        int sx = start.x < end.x ? 1 : -1;
        int sy = start.y < end.y ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // Check if current grid position is blocked
            if (grid.GetNode(new Vector2Int(x, y)).isBlocked)
            {
                return false; // Obstacle found
            }

            if (x == end.x && y == end.y)
            {
                break; // End point reached
            }

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        return true; // No obstacles found
    }

    public void UpdateStartPosition(Vector2Int newStartPosition)
    {
        #if PATHFINDING_DEBUG
            grid.GetNode(startNodePosition).ResetColor();
        #endif

        #if PATHFINDING_DEBUG
            foreach (var node in pathNodesColored)
            {
                node.ResetColor();
            }
            pathNodesColored.Clear();
        #endif
        pathNodes.Clear();

        startNodePosition = newStartPosition;
        openNodes.Clear();
        closedNodes.Clear();

        #if PATHFINDING_DEBUG
            grid.GetNode(startNodePosition).SetColor(startNodeColor);
        #endif

        Node startingNode = grid.GetNode(startNodePosition);
        openNodes.Add(startingNode);
    }

    public void UpdateTargetPosition(Vector2Int newTargetPosition)
    {
        #if PATHFINDING_DEBUG
            grid.GetNode(endNodePosition).ResetColor();
        #endif

        endNodePosition = newTargetPosition;
        openNodes.Clear();
        closedNodes.Clear();

        #if PATHFINDING_DEBUG
            grid.GetNode(endNodePosition).SetColor(endNodeColor);
        #endif

        UpdateStartPosition(grid.GetPositionFromWorldPoint(transform.position));

        if (grid.GetNode(endNodePosition).isBlocked)
        {
            print("End node is blocked");
            recalculationDistance = 0f;
            return;
        }

        if (HasDirectLineOfSight(startNodePosition, endNodePosition))
        {
            print("Direct line of sight, no need to find path");
            findPath = false;
            if (!pathfindingManager.needToFindPath.Contains(this)) pathfindingManager.needToFindPath.Remove(this);
        }
        else
        {
            print("No direct line of sight, finding path");
            if (!pathfindingManager.needToFindPath.Contains(this)) pathfindingManager.needToFindPath.Add(this);
        }
    }

    void SetPathNodes()
    {
        var currentNode = grid.GetNode(endNodePosition);
        while (currentNode != grid.GetNode(startNodePosition))
        {
            if (currentNode != grid.GetNode(endNodePosition)) {
                pathNodes.Add(currentNode);
                #if PATHFINDING_DEBUG
                    currentNode.SetPathColor(pathNodeColor);
                    pathNodesColored.Add(currentNode);
                #endif
            }
            currentNode = currentNode.Parent;
        }
    }
    
    List<Node> GetNeighbours(Node node, Vector2Int nodePosition)
    {
        node.neighbours.Clear();
        Vector2Int rightNodePosition = new Vector2Int(nodePosition.x + 1, nodePosition.y);
        if (rightNodePosition.x < grid.gridCellCountX)
        {
            Node rightNode = grid.GetNode(rightNodePosition);
            // rightNode.SetColor(new Color(0f, 0.25f, 0f , 1));
            node.neighbours.Add(rightNode);
        }
        Vector2Int leftNodePosition = new Vector2Int(nodePosition.x - 1, nodePosition.y);
        if (leftNodePosition.x >= 0)
        {
            Node leftNode = grid.GetNode(leftNodePosition);
            // leftNode.SetColor(new Color(0f, 0.25f, 0f , 1));
            node.neighbours.Add(leftNode);
        }
        Vector2Int upNodePosition = new Vector2Int(nodePosition.x, nodePosition.y + 1);
        if (upNodePosition.y < grid.gridCellCountZ)
        {
            Node upNode = grid.GetNode(upNodePosition);
            // upNode.SetColor(new Color(0f, 0.25f, 0f , 1));
            node.neighbours.Add(upNode);
        }
        Vector2Int downNodePosition = new Vector2Int(nodePosition.x, nodePosition.y - 1);
        if (downNodePosition.y >= 0)
        {
            Node downNode = grid.GetNode(downNodePosition);
            // downNode.SetColor(new Color(0f, 0.25f, 0f , 1));
            node.neighbours.Add(downNode);
        }

        return node.neighbours;
    }
    
    int GetDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int distanceY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);
        // return manhattan distance
        return distanceX + distanceY;
    }
}
