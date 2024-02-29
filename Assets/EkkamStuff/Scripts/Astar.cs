using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using System.Threading.Tasks;

public class Astar : MonoBehaviour
{
    private PathfindingGrid grid;
    [SerializeField] Vector2Int startNodePosition;
    [SerializeField] Vector2Int endNodePosition;
    
    public List<Node> neighbours = new List<Node>();
    public List<Node> openNodes = new List<Node>();
    public List<Node> closedNodes = new List<Node>();
    private Node[] allNodes;

    public bool findPath;
    void Start()
    {
        grid = FindObjectOfType<PathfindingGrid>();
        
        Node startingNode = grid.GetNode(startNodePosition);
        startingNode.SetColor(new Color(0, 0.5f, 0, 1));
        
        Node endingNode = grid.GetNode(endNodePosition);
        endingNode.SetColor(new Color(0.5f, 0, 0, 1));
        
        openNodes.Add(startingNode);
        
        GetNeighbours(startingNode, startNodePosition);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) findPath = true;
        if (!findPath) return;
        if (openNodes.Count < 1)
        {
            print("No path found");
            findPath = false;
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
            ColorNodesPathAfterFindingPath();
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

    void ColorNodesPathAfterFindingPath()
    {
        var currentNode = grid.GetNode(endNodePosition);
        while (currentNode != grid.GetNode(startNodePosition))
        {
            if (currentNode != grid.GetNode(endNodePosition)) currentNode.SetPathColor(new Color(0, 0, 0.5f, 1));
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
