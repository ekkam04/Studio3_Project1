using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace Ekkam
{
    // public class PathfindingManager : MonoBehaviour
    // {
    //     public List<Astar> waitingAstars = new List<Astar>();
    //
    //     void Update()
    //     {
    //         var allAstars = FindObjectsOfType<Astar>();
    //         if (waitingAstars.Count > 0)
    //         {
    //             foreach (var astar in allAstars)
    //             {
    //                 if (astar.state == Astar.PathfindingState.Running)
    //                 {
    //                     print("Astar is running");
    //                     return;
    //                 }
    //                 else if (astar.state == Astar.PathfindingState.Success)
    //                 {
    //                     print("Astar is successful");
    //                     astar.state = Astar.PathfindingState.Idle;
    //                     waitingAstars.Remove(astar);
    //                 }
    //                 else if (astar.state == Astar.PathfindingState.Failure)
    //                 {
    //                     print("Astar failed");
    //                     astar.state = Astar.PathfindingState.Idle;
    //                     waitingAstars.Remove(astar);
    //                 }
    //             }
    //             if (waitingAstars.Count > 0) {
    //                 waitingAstars[0].findPath = true;
    //                 waitingAstars[0].state = Astar.PathfindingState.Running;
    //             }
    //         }
    //     }
    // }
    public class PathfindingManager : MonoBehaviour
{
    private List<JobHandle> pathfindingJobHandles;
    private List<NativeArray<int2>> pathfindingResults;

    void Awake()
    {
        pathfindingJobHandles = new List<JobHandle>();
        pathfindingResults = new List<NativeArray<int2>>();
    }

    public void RequestPath(int2 start, int2 end, int2 gridSize, int2[] blockedPositions, System.Action<NativeList<int2>> callback)
    {
        NativeArray<int2> pathResult = new NativeArray<int2>(gridSize.x * gridSize.y, Allocator.Persistent);
        NativeArray<int2> blockedPos = new NativeArray<int2>(blockedPositions.Length, Allocator.Persistent);
        for (int i = 0; i < blockedPositions.Length; i++)
        {
            blockedPos[i] = blockedPositions[i];
        }
        
        pathfindingResults.Add(pathResult);

        var findPathJob = new DOTSPathfinding.FindPathJob
        {
            startPosition = start,
            endPosition = end,
            gridSize = gridSize,
            pathNodePositions = pathResult,
            blockedPositions = blockedPos
        };

        JobHandle jobHandle = findPathJob.Schedule();
        pathfindingJobHandles.Add(jobHandle);
        
        StartCoroutine(WaitForPathfinding(jobHandle, pathResult, callback));
    }

    private IEnumerator WaitForPathfinding(JobHandle jobHandle, NativeArray<int2> pathResult, System.Action<NativeList<int2>> callback)
    {
        yield return new WaitUntil(() => jobHandle.IsCompleted);
        jobHandle.Complete();
        
        int pathLength = 0;
        for (int i = 0; i < pathResult.Length; i++)
        {
            if (pathResult[i].x == 0 && pathResult[i].y == 0)
            {
                pathLength = i;
                break;
            }
        }
        pathResult = pathResult.GetSubArray(0, pathLength);
        
        Debug.Log("Pathfinding complete, sending path with length: " + pathResult.Length);
        
        NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
        foreach (var pos in pathResult)
        {
            path.Add(pos);
        }
        
        callback(path);

        path.Dispose();
        pathResult.Dispose();
    }

    void OnDestroy()
    {
        foreach (var handle in pathfindingJobHandles)
        {
            if (!handle.IsCompleted)
            {
                handle.Complete();
            }
        }

        foreach (var result in pathfindingResults)
        {
            if (result.IsCreated)
            {
                result.Dispose();
            }
        }
    }
}
}
