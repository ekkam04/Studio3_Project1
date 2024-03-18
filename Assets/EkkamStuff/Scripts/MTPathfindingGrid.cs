using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Ekkam
{
    public struct MTPathfindingGrid
    {
        public int gridCellCountX;
        public int gridCellCountZ;
        public Vector3 startingPosition;
        
        public NativeArray<MTPathNode> nodes;

        // void Awake()
        // {
        //     // startingPosition = transform.position;
        //     InitializeGrid();
        // }
        
        public static MTPathfindingGrid CreateGrid(int gridCellCountX, int gridCellCountZ, Vector3 startingPosition)
        {
            var grid = new MTPathfindingGrid
            {
                gridCellCountX = gridCellCountX,
                gridCellCountZ = gridCellCountZ,
                startingPosition = startingPosition
            };
            grid.InitializeGrid();
            return grid;
        }

        void InitializeGrid()
        {
            int nodeCount = gridCellCountX * gridCellCountZ;
            nodes = new NativeArray<MTPathNode>(nodeCount, Allocator.Persistent);
            
            for (int y = 0; y < gridCellCountZ; y++)
            {
                for (int x = 0; x < gridCellCountX; x++)
                {
                    Vector2Int gridPosition = new Vector2Int(x, y);
                    bool isBlocked = CheckIfBlocked(gridPosition);
                    MTPathNode node = MTPathNode.CreateNode(gridPosition, isBlocked, 0, 0);
                    nodes[x + y * gridCellCountX] = node;
                }
            }
            Debug.Log("Grid initialized");
        }

        bool CheckIfBlocked(Vector2Int gridPosition)
        {
            Vector3 position = new Vector3(gridPosition.x, 0, gridPosition.y) + startingPosition;
            int layerToIgnore = 6; // Player layer
            LayerMask mask = ~(1 << layerToIgnore);
            bool isBlocked = Physics.CheckBox(position, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, mask);
            return isBlocked;
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
                if (GetNode(new Vector2Int(x, y)).isBlocked)
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
        
        public int GetDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
        
        public bool ObjectIsOnGrid(Vector3 worldPosition)
        {
            Vector2Int gridPosition = GetPositionFromWorldPoint(worldPosition);
            if (gridPosition.x < 0 || gridPosition.x >= gridCellCountX || gridPosition.y < 0 || gridPosition.y >= gridCellCountZ)
            {
                return false;
            }
            return true;
        }
        
        public Vector2Int GetPositionFromWorldPoint(Vector3 worldPosition)
        {
            Vector3 localPosition = worldPosition - startingPosition;
            int x = Mathf.FloorToInt(localPosition.x);
            int z = Mathf.FloorToInt(localPosition.z);
            return new Vector2Int(x, z);
        }

        void OnDestroy()
        {
            if (nodes.IsCreated)
            {
                nodes.Dispose();
            }
        }
    }
}
