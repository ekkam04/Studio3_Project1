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
            // checkbox for isBlocked
            bool isBlocked = Physics.CheckBox(node.transform.position, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity);
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
}
