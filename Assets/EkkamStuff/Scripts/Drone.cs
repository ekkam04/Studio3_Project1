using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Ekkam
{
    public class Drone : MonoBehaviour
    {
        public float speed = 5f;
        public List<Transform> patrolPoints;
        public GameObject patrolPointHolder;
        
        private int currentPatrolPointIndex;
        public bool reversePatrol;
        Vector3 targetPoint;
        
        public bool playerDetected;
        
        public PathfindingGrid gridToAlert;
        public Collider detectionCollider;

        private Enemy nearestEnemy;

        void Start()
        {
            foreach (Transform child in patrolPointHolder.transform)
            {
                patrolPoints.Add(child);
            }
        }
        
        void Update()
        {
            if (!playerDetected)
            {
                FollowPatrolPath();
            }
            else
            {
                GoToNearestEnemy();
            }
        }

        void FollowPatrolPath()
        {
            if (patrolPoints.Count > 0)
            {
                targetPoint = patrolPoints[currentPatrolPointIndex].position;
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

                transform.position = Vector3.MoveTowards(transform.position,
                    patrolPoints[currentPatrolPointIndex].position, speed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(targetPoint - transform.position), 0.1f);
            }
        }
        
        void GoToNearestEnemy()
        {
            if (nearestEnemy == null)
            {
                nearestEnemy = GetNearestEnemy();
            }
            else
            {
                var targetPosition = new Vector3(nearestEnemy.transform.position.x, transform.position.y, nearestEnemy.transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(nearestEnemy.transform.position - transform.position), 0.1f);
            }
            
            print (Vector3.Distance(transform.position, nearestEnemy.transform.position));
            if (Vector3.Distance(transform.position, nearestEnemy.transform.position) < 3f)
            {
                nearestEnemy.detectionRange = 999;
                playerDetected = false;
                nearestEnemy = null;
            }
        }

        Enemy GetNearestEnemy()
        {
            Enemy nearestEnemy = null;
            float nearestDistance = Mathf.Infinity;
            foreach (var enemy in gridToAlert.enemiesOnThisGrid)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
            return nearestEnemy;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                print("Player detected");
                playerDetected = true;
            }
        }
    }
}