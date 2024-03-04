using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;

namespace Ekkam
{
    public class Enemy : Damagable
    {
        Astar astar;
        Rigidbody rb;
        Player player;
        PathfindingManager pathfindingManager;

        public float attackRange = 1.5f;

        public float speed = 5f;

        void Start()
        {
            astar = GetComponent<Astar>();
            rb = GetComponent<Rigidbody>();
            player = FindObjectOfType<Player>();
            pathfindingManager = FindObjectOfType<PathfindingManager>();
        }

        void Update()
        {
            FollowPath();
        }

        void FollowPath()
        {
            if (astar.pathNodes.Count > 0)
            {
                Vector3 targetPosition = new Vector3(
                    astar.pathNodes[astar.pathNodes.Count - 1].transform.position.x,
                    transform.position.y,
                    astar.pathNodes[astar.pathNodes.Count - 1].transform.position.z
                );
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 10 * Time.deltaTime);
                rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime));
                if (Vector3.Distance(transform.position, astar.pathNodes[astar.pathNodes.Count - 1].transform.position) < 0.7f)
                {
                    astar.pathNodes.RemoveAt(astar.pathNodes.Count - 1);
                }
                
                print(Vector3.Distance(transform.position, player.transform.position));
                if (Vector3.Distance(transform.position, player.transform.position) < attackRange)
                {
                    astar.pathNodes.Clear();
                }
            }
            else if (
                astar.pathNodes.Count == 0
                && !astar.findPath
                && Vector3.Distance(transform.position, player.transform.position) > attackRange + 0.5f
                && !pathfindingManager.needToFindPath.Contains(astar)
            )
            {
                Vector3 targetPosition = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 10 * Time.deltaTime);
                rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime));
            }
        }
    }
}
