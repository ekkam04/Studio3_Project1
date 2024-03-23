using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Ekkam
{
    public struct MTAstar : IJobParallelFor
    {
        // public MTPathfindingGrid grid;
        public NativeArray<MTPathNode> nodes;
        public int gridCellCountX;
        public int gridCellCountZ;
        
        public Vector2Int startNodePosition;
        public Vector2Int endNodePosition;

        public NativeList<Vector2Int> openList;
        public NativeList<Vector2Int> closedList;
        public NativeArray<Vector2Int> pathNodePositions;

        [ReadOnly] public NativeHashMap<Vector2Int, Vector2Int> cameFrom;
        [ReadOnly] public NativeHashMap<Vector2Int, int> gScore;
        [ReadOnly] public NativeHashMap<Vector2Int, int> fScore;
        
        public static MTAstar CreateAstar(NativeArray<MTPathNode> nodes, int gridCellCountX, int gridCellCountZ, Vector2Int startNodePosition, Vector2Int endNodePosition, NativeArray<Vector2Int> pathNodePositions)
        {
            var astar = new MTAstar
            {
                nodes = nodes,
                gridCellCountX = gridCellCountX,
                gridCellCountZ = gridCellCountZ,
                startNodePosition = startNodePosition,
                endNodePosition = endNodePosition,
                pathNodePositions = pathNodePositions,
                openList = new NativeList<Vector2Int>(0, Allocator.TempJob),
                closedList = new NativeList<Vector2Int>(0, Allocator.TempJob),
                cameFrom = new NativeHashMap<Vector2Int, Vector2Int>(0, Allocator.TempJob),
                gScore = new NativeHashMap<Vector2Int, int>(0, Allocator.TempJob),
                fScore = new NativeHashMap<Vector2Int, int>(0, Allocator.TempJob),
            };
            return astar;
        }
        
        public void Execute(int index)
        {
            FindPath(startNodePosition, endNodePosition, index);
        }

        public void FindPath(Vector2Int start, Vector2Int goal, int index)
        {
            openList.Clear();
            closedList.Clear();
            cameFrom.Clear();
            gScore.Clear();
            fScore.Clear();

            openList.Add(start);
            gScore[start] = 0;
            fScore[start] = GetDistance(start, goal);

            while (openList.Length > 0)
            {
                Vector2Int current = GetLowestFScoreNode(openList, fScore);
                if (current == goal)
                {
                    Debug.Log("Path found");
                    ReconstructPath(cameFrom, current, index);
                    return;
                }
                Debug.Log("Finding path");

                // openList.Remove(current);
                openList.RemoveAtSwapBack(openList.IndexOf(current));
                closedList.Add(current);

                foreach (Vector2Int neighbor in GetNeighbourPositions(current))
                {
                    if (closedList.Contains(neighbor) || GetNode(neighbor).isBlocked)
                        continue;

                    int newMovementCostToNeighbour = gScore[current] + GetDistance(current, neighbor);
                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                    else if (newMovementCostToNeighbour >= gScore[neighbor])
                        continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = newMovementCostToNeighbour;
                    fScore[neighbor] = gScore[neighbor] + GetDistance(neighbor, goal);
                }
            }
        }

        private Vector2Int GetLowestFScoreNode(NativeList<Vector2Int> openList, NativeHashMap<Vector2Int, int> fScore)
        {
            Vector2Int lowest = openList[0];
            foreach (Vector2Int pos in openList)
            {
                if (fScore[pos] < fScore[lowest])
                {
                    lowest = pos;
                }
            }
            return lowest;
        }

        private void ReconstructPath(NativeHashMap<Vector2Int, Vector2Int> cameFrom, Vector2Int current, int index)
        {
            // while (cameFrom.ContainsKey(current))
            // {
            //     current = cameFrom[current];
            //     totalPath.Add(current);
            // }
            // totalPath.Reverse();
            
            while (cameFrom.ContainsKey(current))
            {
                pathNodePositions[index] = current;
                current = cameFrom[current];
            }
            
            foreach (Vector2Int pos in pathNodePositions)
            {
                Debug.Log(pos);
            }
        }

        private int GetDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
        
        public List<Vector2Int> GetNeighbourPositions(Vector2Int nodePosition)
        {
            List<Vector2Int> neighbours = new List<Vector2Int>();
            
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(1, 0),   // Right
                new Vector2Int(0, -1),  // Down
                new Vector2Int(-1, 0)   // Left
            };

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbourPos = nodePosition + direction;
                // Check if the neighbour is within grid bounds before adding
                if (IsPositionInsideGrid(neighbourPos))
                {
                    neighbours.Add(neighbourPos);
                }
            }

            return neighbours;
        }

        private bool IsPositionInsideGrid(Vector2Int position)
        {
            return position.x >= 0 && position.x < gridCellCountX && position.y >= 0 && position.y < gridCellCountZ;
        }
        
        public MTPathNode GetNode(Vector2Int gridPosition)
        {
            int index = gridPosition.x + gridPosition.y * gridCellCountX;
            if (index >= 0 && index < nodes.Length)
            {
                return nodes[index];
            }
            else
            {
                Debug.LogError("Grid position out of bounds: " + gridPosition);
                return default(MTPathNode);
            }
        }
    }
}
