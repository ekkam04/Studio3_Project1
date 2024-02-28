using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Node[] nodes;
    public int gridCellCountX = 3;
    public int gridCellCountZ = 3;
    private int gridCellCount;
    public GameObject cube;
    
    // x + z * 3 for coordinates to index
    
    void Awake()
    {
        gridCellCount = gridCellCountX * gridCellCountZ;
        nodes = new Node[gridCellCount];
        for (int y = 0; y < gridCellCountZ; y++)
        {
            for (int x = 0; x < gridCellCountX; x++)
            {
                int i = x + y * gridCellCountX;
                
                GameObject go = GameObject.Instantiate(cube, new Vector3(x, 0, y), Quaternion.identity);
                var node = go.GetComponent<Node>();
                nodes[i] = node;
                node.GCost = x;
                node.HCost = y;
            }
        }
    }
    
    public Node GetNode(Vector2Int gridPosition)
    {
        return nodes[gridPosition.x + gridPosition.y * gridCellCountX];
    }
    
    public Vector2Int GetGridPosition(Node node)
    {
        int index = System.Array.IndexOf(nodes, node);
        return new Vector2Int(index % gridCellCountX, index / gridCellCountX);
    }
}
