using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

public class DOTSPathfinding : MonoBehaviour
{
    private const int moveStraightCost = 10;
    private const int moveDiagonalCost = 14;

    void Start()
    {
        // Debug.Log("DOTSPathfinding Start");
        //
        // NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(5, Allocator.Temp);
        // for (int i = 0; i < 5; i++)
        // {
        //     FindPathJob findPathJob = new FindPathJob
        //     {
        //         startPosition = new int2(0, 0),
        //         endPosition = new int2(3, 3)
        //     };
        //     jobHandles[i] = findPathJob.Schedule();
        // }
        //
        // JobHandle.CompleteAll(jobHandles);
        // jobHandles.Dispose();
    }

    [BurstCompile]
    public struct FindPathJob : IJob
    {
        public int2 startPosition;
        public int2 endPosition;
        public NativeArray<int2> pathNodePositions;
        public NativeArray<int2> blockedPositions;
        public int2 gridSize;
        
        public void Execute()
        {
            int2 gridSize = this.gridSize;
            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

            // Debug.Log("FindPathJob called with data: " + startPosition + " " + endPosition + " " + gridSize);
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    PathNode pathNode = new PathNode();
                    pathNode.x = x;
                    pathNode.y = y;
                    pathNode.index = CalculateIndex(x, y, gridSize.x);

                    pathNode.gCost = int.MaxValue;
                    pathNode.hCost = CalculateDistanceCost(new int2(x, y), endPosition);
                    pathNode.CalculateFCost();

                    // pathNode.isWalkable = true;
                    pathNode.isWalkable = !blockedPositions.Contains(new int2(x, y));
                    pathNode.cameFromNodeIndex = -1;

                    pathNodeArray[pathNode.index] = pathNode;
                }
            }

            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(new int2[]
            {
                new int2(-1, 0), // Left
                new int2(+1, 0), // Right
                new int2(0, +1), // Up
                new int2(0, -1), // Down
                // new int2(-1, -1), // Left Down
                // new int2(-1, +1), // Left Up
                // new int2(+1, -1), // Right Down
                // new int2(+1, +1) // Right Up
            }, Allocator.Temp);
            
            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;
            
            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);
            
            openList.Add(startNode.index);
            
            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];
                
                if (currentNodeIndex == CalculateIndex(endPosition.x, endPosition.y, gridSize.x))
                {
                    break;
                }
                
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }
                
                closedList.Add(currentNodeIndex);
                
                for (int i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);
                    
                    if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                    {
                        // Neighbour not inside the grid
                        continue;
                    }
                    
                    int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);
                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        // Neighbour already searched
                        continue;
                    }
                    
                    PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                    if (!neighbourNode.isWalkable)
                    {
                        // Neighbour not walkable
                        continue;
                    }
                    
                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);
                    
                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();
                        pathNodeArray[neighbourNodeIndex] = neighbourNode;
                        
                        if (!openList.Contains(neighbourNode.index))
                        {
                            openList.Add(neighbourNode.index);
                        }
                    }
                }
            }
            
            PathNode endNode = pathNodeArray[CalculateIndex(endPosition.x, endPosition.y, gridSize.x)];
            if (endNode.cameFromNodeIndex == -1)
            {
                // Path not found
                // Debug.Log("Path not found");
            }
            else
            {
                // Path found
                // Debug.Log("Path found");
                NativeList<int2> path = CalculatePath(pathNodeArray, endNode);
                
                for (int i = 0; i < path.Length; i++)
                {
                    // Debug.Log(path[i]);
                    pathNodePositions[i] = path[i];
                }
                // Debug.Log("path length in job: " + pathNodePositions.Length);
                
                path.Dispose();
            }
            
            pathNodeArray.Dispose();
            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
        }
        
        private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                // Path not found
                return new NativeList<int2>(Allocator.Temp);
            }
            else
            {
                // Path found
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));
            
                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }
            
                return path;
            }
        }
    
        private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
        {
            return gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < gridSize.x && gridPosition.y < gridSize.y;
        }
    
        private int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }
    
        private int CalculateDistanceCost(int2 a, int2 b)
        {
            int xDistance = math.abs(a.x - b.x);
            int yDistance = math.abs(a.y - b.y);
            int remaining = math.abs(xDistance - yDistance);
            return moveDiagonalCost * math.min(xDistance, yDistance) + moveStraightCost * remaining;
        }
    
        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostPathNode.fCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }
            return lowestCostPathNode.index;
        }
    }

    private struct PathNode
    {
        public int x;
        public int y;

        public int index;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;
        
        public int cameFromNodeIndex;
        
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }
}
