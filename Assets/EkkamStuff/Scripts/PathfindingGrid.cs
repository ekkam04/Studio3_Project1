using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PathfindingGrid : MonoBehaviour
{
    public Node[] nodes;
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
        nodes = new Node[gridCellCount];
        for (int y = 0; y < gridCellCountZ; y++)
        {
            for (int x = 0; x < gridCellCountX; x++)
            {
                int i = x + y * gridCellCountX;
                
                GameObject go = GameObject.Instantiate(cube, new Vector3(x, 0, y) + startingPosition, Quaternion.identity, transform);
                var node = go.GetComponent<Node>();
                node.gridPosition = new Vector2Int(x, y);
                nodes[i] = node;
                node.GCost = x;
                node.HCost = y;
            }
        }
        UpdateBlockedNodes();
    }

    // private void Update()
    // {
    //     timer += Time.deltaTime;
    //     if (timer > 1f)
    //     {
    //         timer = 0;
    //         UpdateBlockedNodes();
    //     }
    // }

    public Node GetNode(Vector2Int gridPosition)
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
}
