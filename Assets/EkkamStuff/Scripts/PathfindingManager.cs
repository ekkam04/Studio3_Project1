using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class PathfindingManager : MonoBehaviour
    {
        public List<Astar> waitingAstars = new List<Astar>();

        void Update()
        {
            var allAstars = FindObjectsOfType<Astar>();
            if (waitingAstars.Count > 0)
            {
                foreach (var astar in allAstars)
                {
                    if (astar.state == Astar.PathfindingState.Running)
                    {
                        print("Astar is running");
                        return;
                    }
                    else if (astar.state == Astar.PathfindingState.Success)
                    {
                        print("Astar is successful");
                        astar.state = Astar.PathfindingState.Idle;
                        waitingAstars.Remove(astar);
                    }
                    else if (astar.state == Astar.PathfindingState.Failure)
                    {
                        print("Astar failed");
                        astar.state = Astar.PathfindingState.Idle;
                        waitingAstars.Remove(astar);
                    }
                }
                if (waitingAstars.Count > 0) {
                    waitingAstars[0].findPath = true;
                    waitingAstars[0].state = Astar.PathfindingState.Running;
                }
            }
        }
    }
}
