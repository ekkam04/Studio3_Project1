using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace Ekkam
{
    public class Astar : MonoBehaviour
    {
        [SerializeField] public PathfindingGrid grid;
        PathfindingManager pathfindingManager;
        [SerializeField] Vector2Int startNodePosition;
        [SerializeField] public Vector2Int endNodePosition;

        [SerializeField] Color startNodeColor = new Color(0, 0.5f, 0, 1);
        [SerializeField] Color endNodeColor = new Color(0.5f, 0, 0, 1);
        [SerializeField] Color pathNodeColor = new Color(0, 0, 0.5f, 1);

        public enum PathfindingState { Idle, Waiting, Running, Success, Failure }
        public PathfindingState state;
        
        public List<PathfindingNode> neighbours = new List<PathfindingNode>();
        public List<PathfindingNode> openNodes = new List<PathfindingNode>();
        public List<PathfindingNode> closedNodes = new List<PathfindingNode>();
        public List<PathfindingNode> pathNodes = new List<PathfindingNode>();
        private List<PathfindingNode> pathNodesColored = new List<PathfindingNode>();
        private PathfindingNode[] allNodes;

        public bool findPath;
        public bool assignedInitialTarget;

        void Start()
        {
            pathfindingManager = FindObjectOfType<PathfindingManager>();
            PathfindingGrid.onGridInitialized += Initialize;
        }

        void Initialize()
        {
            print("Astar initialized");
            // UpdateStartPosition(grid.GetPositionFromWorldPoint(transform.position));
            // PathfindingNode startingNode = grid.GetNode(startNodePosition);
            
            PathfindingNode endingNode = grid.GetNode(endNodePosition);
            endingNode.SetColor(endNodeColor);
            
            // GetNeighbours(startingNode, startNodePosition);
        }

        private void Update()
        {
            // if (findPath) FindPath();

            //UpdateTargetPosition(grid.GetPositionFromWorldPoint(Player.Instance.transform.position));
        }

        // void FindPath()
        // {
        //     if (pathNodes.Count > 0)
        //     {
        //         print("Path already found");
        //         findPath = false;
        //         state = PathfindingState.Success;
        //         return;
        //     }
        //
        //     if (openNodes.Count < 1)
        //     {
        //         print("No path found");
        //         findPath = false;
        //         state = PathfindingState.Failure;
        //         return;
        //     }
        //     var currentNode = openNodes[0];
        //     foreach (var node in openNodes)
        //     {
        //         if (node.FCost < currentNode.FCost)
        //         {
        //             currentNode = node;
        //         }
        //     }
        //     openNodes.Remove(currentNode);
        //     closedNodes.Add(currentNode);
        //     
        //     if (currentNode == grid.GetNode(endNodePosition))
        //     {
        //         print("Path found");
        //         findPath = false;
        //         SetPathNodes();
        //         state = PathfindingState.Success;
        //         return;
        //     }
        //     
        //     var currentNeighbours = GetNeighbours(currentNode, currentNode.gridPosition);
        //     foreach (var neighbour in currentNeighbours)
        //     {
        //         var neighborGridPosition = neighbour.gridPosition;
        //         // print("neighbour: " + neighborGridPosition);
        //         // check if it is in blocked positions or closed nodes
        //         if (neighbour.isBlocked || closedNodes.Contains(neighbour))
        //         {
        //             continue;
        //         }
        //         // check if new path to neighbour is shorter or neighbour is not in openNodes
        //         var newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
        //         if (newMovementCostToNeighbour < neighbour.GCost || !openNodes.Contains(neighbour))
        //         {
        //             neighbour.GCost = newMovementCostToNeighbour;
        //             neighbour.HCost = GetDistance(neighbour, grid.GetNode(endNodePosition));
        //             neighbour.Parent = currentNode;
        //             if (!openNodes.Contains(neighbour))
        //             {
        //                 openNodes.Add(neighbour);
        //             }
        //         }
        //     }
        //     state = PathfindingState.Running;
        // }

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
        
            PathfindingNode startingNode = grid.GetNode(startNodePosition);
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
        
            // fix this
            UpdateStartPosition(grid.GetPositionFromWorldPoint(transform.position));
        
            if (grid.GetNode(endNodePosition).isBlocked)
            {
                print("End node is blocked");
                return;
            }
        }

        // void SetPathNodes()
        // {
        //     var currentNode = grid.GetNode(endNodePosition);
        //     while (currentNode != grid.GetNode(startNodePosition))
        //     {
        //         if (currentNode != grid.GetNode(endNodePosition)) {
        //             pathNodes.Add(currentNode);
        //             #if PATHFINDING_DEBUG
        //                 currentNode.SetPathColor(pathNodeColor);
        //                 pathNodesColored.Add(currentNode);
        //             #endif
        //         }
        //         currentNode = currentNode.Parent;
        //     }
        // }
        //
        // List<PathfindingNode> GetNeighbours(PathfindingNode node, Vector2Int nodePosition)
        // {
        //     node.neighbours.Clear();
        //     Vector2Int rightNodePosition = new Vector2Int(nodePosition.x + 1, nodePosition.y);
        //     if (rightNodePosition.x < grid.gridCellCountX)
        //     {
        //         PathfindingNode rightNode = grid.GetNode(rightNodePosition);
        //         // rightNode.SetColor(new Color(0f, 0.25f, 0f , 1));
        //         node.neighbours.Add(rightNode);
        //     }
        //     Vector2Int leftNodePosition = new Vector2Int(nodePosition.x - 1, nodePosition.y);
        //     if (leftNodePosition.x >= 0)
        //     {
        //         PathfindingNode leftNode = grid.GetNode(leftNodePosition);
        //         // leftNode.SetColor(new Color(0f, 0.25f, 0f , 1));
        //         node.neighbours.Add(leftNode);
        //     }
        //     Vector2Int upNodePosition = new Vector2Int(nodePosition.x, nodePosition.y + 1);
        //     if (upNodePosition.y < grid.gridCellCountZ)
        //     {
        //         PathfindingNode upNode = grid.GetNode(upNodePosition);
        //         // upNode.SetColor(new Color(0f, 0.25f, 0f , 1));
        //         node.neighbours.Add(upNode);
        //     }
        //     Vector2Int downNodePosition = new Vector2Int(nodePosition.x, nodePosition.y - 1);
        //     if (downNodePosition.y >= 0)
        //     {
        //         PathfindingNode downNode = grid.GetNode(downNodePosition);
        //         // downNode.SetColor(new Color(0f, 0.25f, 0f , 1));
        //         node.neighbours.Add(downNode);
        //     }
        //
        //     return node.neighbours;
        // }
        
        public int GetDistance(PathfindingNode nodeA, PathfindingNode nodeB)
        {
            int distanceX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int distanceY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);
            // return manhattan distance
            return distanceX + distanceY;
        }
    }
}
