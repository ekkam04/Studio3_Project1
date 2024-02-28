using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> neighbours = new List<Node>();
    public bool isBlocked;
    private int gCost;
    public TMP_Text gCostText;
    public int GCost
    {
        get
        {
            return gCost;
        }
        set
        {
            gCost = value;
            gCostText.text = gCost.ToString();
            fCost = gCost + hCost;
            fCostText.text = fCost.ToString();
        }
    }
    private int hCost;
    public TMP_Text hCostText;
    public int HCost
    {
        get
        {
            return hCost;
        }
        set
        {
            hCost = value;
            hCostText.text = hCost.ToString();
            fCost = gCost + hCost;
            fCostText.text = fCost.ToString();
        }
    }
    private int fCost;
    public TMP_Text fCostText;
    public int FCost
    {
        get
        {
            return fCost;
        }
    }
    public Node Parent;
    
    public void SetColor(Color color)
    {
        if (GetComponent<MeshRenderer>().material.color == Color.black)
        {
            GetComponent<MeshRenderer>().material.color = color;
        }
    }    
    public void SetPathColor(Color color)
    {
        GetComponent<MeshRenderer>().material.color = color;
    }
}
