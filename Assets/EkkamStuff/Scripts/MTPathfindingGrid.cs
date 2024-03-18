using System;
using Unity.Collections;
using UnityEngine;

namespace Ekkam
{
    public class MTPathfindingGrid : MonoBehaviour
    {
        public int gridCellCountX = 3;
        public int gridCellCountZ = 3;
        public Vector3 startingPosition;
        
        public NativeArray<MTPathNode> nodes;

        void Awake()
        {
            startingPosition = transform.position;
            InitializeGrid();
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
