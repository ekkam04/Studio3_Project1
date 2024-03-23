using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class Patrol : MonoBehaviour
    {
        public List<Transform> patrolPoints;
        
        private int currentPatrolPointIndex;
        public bool reversePatrol;
        Vector3 targetPoint;
        
        void Update()
        {
            if (patrolPoints.Count > 0)
            {
                if (Vector3.Distance(transform.position, patrolPoints[currentPatrolPointIndex].position) < 0.5f)
                {
                    if (reversePatrol)
                    {
                        if (currentPatrolPointIndex == 0)
                        {
                            reversePatrol = false;
                        }
                        else
                        {
                            currentPatrolPointIndex--;
                        }
                    }
                    else
                    {
                        if (currentPatrolPointIndex == patrolPoints.Count - 1)
                        {
                            reversePatrol = true;
                        }
                        else
                        {
                            currentPatrolPointIndex++;
                        }
                    }
                }
                transform.position = Vector3.MoveTowards(transform.position, patrolPoints[currentPatrolPointIndex].position, 0.1f);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPoint - transform.position), 0.1f);
            }
        }
        
    }
}