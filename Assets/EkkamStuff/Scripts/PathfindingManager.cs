using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public List<Astar> needToFindPath = new List<Astar>();

    void Update()
    {
        for (int i = 0; i < needToFindPath.Count; i++)
        {
            if (i == 0)
            {
                needToFindPath[i].findPath = true;
            }
            else
            {
                needToFindPath[i].findPath = false;
            }
        }
    }
}
