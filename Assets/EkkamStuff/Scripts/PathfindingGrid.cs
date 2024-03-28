using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using QFSW.QC;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace Ekkam
{
    public class PathfindingGrid : MonoBehaviour
    {
        public PathfindingNode[] nodes;
        public int gridCellCountX = 3;
        public int gridCellCountZ = 3;
        private int gridCellCount;
        public Vector3 startingPosition;
        
        public List<Enemy> enemiesOnThisGrid = new List<Enemy>();
        
        public GameObject cube;

        private float timer;
        
        // x + z * 3 for coordinates to index
        
        void Start()
        {
            CreateGrid();
            Action.onActionComplete += UpdateBlockedNodes;
            MovesWithPhysicsOnGrid.onMoveComplete += ConditionallyUpdateBlockedNodes;
        }
        
        void OnDestroy()
        {
            Action.onActionComplete -= UpdateBlockedNodes;
            MovesWithPhysicsOnGrid.onMoveComplete -= ConditionallyUpdateBlockedNodes;
        }
        
        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.B))
        //     {
        //         UpdateBlockedNodes();
        //     }
        // }
        
        void CreateGrid()
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
                // await Task.Delay(1);
            }
            UpdateBlockedNodes();
        }

        public PathfindingNode GetNode(Vector2Int gridPosition)
        {
            if (gridPosition.x < 0 || gridPosition.x >= gridCellCountX || gridPosition.y < 0 || gridPosition.y >= gridCellCountZ)
            {
                return null;
            }
            return nodes[gridPosition.x + gridPosition.y * gridCellCountX];
        }
        
        void ConditionallyUpdateBlockedNodes()
        {
            // check if player is on this grid
            if (ObjectIsOnGrid(Player.Instance.transform.position))
            {
                UpdateBlockedNodes();
            }
        }
        
        [Command("updateBlockedNodes")]
        async void UpdateBlockedNodes()
        {
            await Task.Delay(100);
            if (nodes == null) return;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                int[] layersToIgnore = {6, 8}; // Player and Enemy layers
                LayerMask mask = ~(1 << layersToIgnore[0]) & ~(1 << layersToIgnore[1]);
                bool isBlocked = Physics.CheckBox(node.transform.position, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, mask);
                node.isBlocked = isBlocked;
                if (isBlocked)
                {
                    node.SetColor(new Color(0f, 0f, 0f, 0));
                }
                else
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
        
        public int2[] GetBlockedPositions()
        {
            List<int2> blockedPositions = new List<int2>();
            foreach (var node in nodes)
            {
                if (node.isBlocked)
                {
                    blockedPositions.Add(new int2(node.gridPosition.x, node.gridPosition.y));
                }
            }
            return blockedPositions.ToArray();
        }
        
        public int GetDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
