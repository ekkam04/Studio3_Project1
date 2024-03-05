using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ekkam
{
    public class PathfindingGrid : MonoBehaviour
    {
        public PathfindingNode[] nodes;
        public int gridCellCountX = 3;
        public int gridCellCountZ = 3;
        private int gridCellCount;
        public GameObject cube;
        public Vector3 startingPosition;

        private float timer;
        
        // x + z * 3 for coordinates to index
        
        void Awake()
        {
            startingPosition = transform.position;
            gridCellCount = gridCellCountX * gridCellCountZ;
            nodes = new PathfindingNode[gridCellCount];
            for (int y = 0; y < gridCellCountZ; y++)
            {
                for (int x = 0; x < gridCellCountX; x++)
                {
                    int i = x + y * gridCellCountX;
                    
                    GameObject go = GameObject.Instantiate(cube, new Vector3(x, 0, y) + startingPosition, Quaternion.identity, transform);
                    var node = go.GetComponent<PathfindingNode>();
                    node.gridPosition = new Vector2Int(x, y);
                    nodes[i] = node;
                    node.GCost = x;
                    node.HCost = y;
                }
            }
            UpdateBlockedNodes();
        }

        public PathfindingNode GetNode(Vector2Int gridPosition)
        {
            return nodes[gridPosition.x + gridPosition.y * gridCellCountX];
        }
        
        void UpdateBlockedNodes()
        {
            // var allNodes = FindObjectsOfType<Node>();
            foreach (var node in nodes)
            {
                int layerToIgnore = 6; // Player layer
                LayerMask mask = ~(1 << layerToIgnore);
                bool isBlocked = Physics.CheckBox(node.transform.position, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, mask);
                node.isBlocked = isBlocked;
                if (isBlocked)
                {
                    node.SetColor(new Color(0f, 0f, 0f, 0));
                }
                else //if (node.gridPosition != startNodePosition && node.gridPosition != endNodePosition)
                {
                    node.ResetColor();
                }
            }
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
    }
}
