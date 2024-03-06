using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ekkam;
using Unity.VisualScripting;
using UnityEngine;

namespace Ekkam
{
    public class Enemy : Damagable
    {
        Node rootNode;

        Astar astar;
        PathfindingGrid grid;
        PathfindingManager pathfindingManager;

        Rigidbody rb;
        Enemy closestEnemy;
        CombatManager combatManager;

        public float speed = 2f;
        public float attackRange = 3f;
        public bool canMove = true;
        private float attackTimer;
        public float attackCooldown = 2f;
        public enum EnemyType
        {
            Melee,
            Archer,
            Mage
        }
        public EnemyType enemyType;

        void Start()
        {
            astar = GetComponent<Astar>();
            grid = FindObjectOfType<PathfindingGrid>();
            pathfindingManager = FindObjectOfType<PathfindingManager>();

            rb = GetComponent<Rigidbody>();
            combatManager = GetComponent<CombatManager>();

            rootNode = new Selector(new List<Node>
            {
                // Check for player presence and idle if not present
                new Sequence(new List<Node>
                {
                    new InvertDecorator(new CheckPlayerPresence(grid, astar, transform, canMove)),
                    new Idle()
                }),
                
                // Engage player based on conditions
                new Selector(new List<Node>{
                    new Sequence(new List<Node>{
                        new CanMove(canMove),
                        new CheckLineOfSight(grid, astar, transform),
                        new InvertDecorator(new CanAttack(this)), // Check if not ready to attack
                        new WalkTowardsPlayer(transform, rb, speed, canMove),
                    }),
                    new Sequence(new List<Node>{
                        // new CheckLineOfSight(grid, astar, transform),
                        new CanAttack(this), // Confirm ready to attack
                        new AttackPlayer()
                    }),
                    // Indirect engagement when line of sight is lost
                    new Sequence(new List<Node>{
                        new CanMove(canMove),
                        new InvertDecorator(new CheckLineOfSight(grid, astar, transform)),
                        new Selector(new List<Node>{
                            new Sequence(new List<Node>{
                                new CheckClosestEnemyDistance(this),
                                new CopyEnemyPath(this)
                            }),
                            new Sequence(new List<Node>{
                                new AddToPathfindingQueue(pathfindingManager, astar),
                                new CheckPathFound(astar),
                                new FollowPath(this)
                            })
                        })
                    })
                })
            });
        }

        void Update()
        {
            rootNode.Evaluate();
        }

        public class CheckPlayerPresence : Node
        {
            private float detectionRange = 25f;
            private float recalculationDistance = 20f;
            private PathfindingGrid grid;
            private Astar astar;
            private Transform transform;
            private bool canMove;

            public CheckPlayerPresence(PathfindingGrid grid, Astar astar, Transform transform, bool canMove)
            {
                this.grid = grid;
                this.astar = astar;
                this.transform = transform;
                this.canMove = canMove;
            }

            public override NodeState Evaluate()
            {
                if (
                    (grid.ObjectIsOnGrid(Player.Instance.transform.position) || !canMove)
                    && (Vector3.Distance(transform.position, Player.Instance.transform.position) < detectionRange
                    || astar.pathNodes.Count > 0)
                )
                {
                    print("Player is present");

                    if (
                        canMove &&
                        astar.GetDistance(
                        grid.GetNode(grid.GetPositionFromWorldPoint(Player.Instance.transform.position)),
                        grid.GetNode(astar.endNodePosition)
                    ) > recalculationDistance)
                    {
                        astar.UpdateTargetPosition(grid.GetPositionFromWorldPoint(Player.Instance.transform.position));
                    }

                    return NodeState.Success;
                }
                else
                {
                    print("Player is not present");
                    return NodeState.Failure;
                }
            }
        }

        public class Idle : Node
        {
            public override NodeState Evaluate()
            {
                print("Idle");
                return NodeState.Success;
            }
        }

        public class CheckLineOfSight : Node
        {
            private PathfindingGrid grid;
            private Astar astar;
            private Transform transform;

            public CheckLineOfSight(PathfindingGrid grid, Astar astar, Transform transform)
            {
                this.grid = grid;
                this.astar = astar;
                this.transform = transform;
            }

            public override NodeState Evaluate()
            {
                if (grid.HasDirectLineOfSight(
                    grid.GetPositionFromWorldPoint(transform.position),
                    astar.endNodePosition
                ))
                {
                    print("Line of sight");
                    return NodeState.Success;
                }
                else
                {
                    print("No line of sight");
                    return NodeState.Failure;
                }
            }
        }

        public class WalkTowardsPlayer : Node
        {
            private Transform transform;
            private Rigidbody rb;
            private float speed;

            public WalkTowardsPlayer(Transform transform, Rigidbody rb, float speed, bool canMove)
            {
                this.transform = transform;
                this.rb = rb;
                this.speed = speed;
            }

            public override NodeState Evaluate()
            {
                print("Walking towards player");
                Vector3 targetPosition = new Vector3(Player.Instance.transform.position.x, transform.position.y, Player.Instance.transform.position.z);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 10 * Time.deltaTime);
                rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime));
                return NodeState.Success;
            }
        }

        public class CanMove : Node
        {
            private bool canMove;

            public CanMove(bool canMove)
            {
                this.canMove = canMove;
            }

            public override NodeState Evaluate()
            {
                if (canMove)
                {
                    print("Can move");
                    return NodeState.Success;
                }
                else
                {
                    print("Can't move");
                    return NodeState.Failure;
                }
            }
        }

        public class CanAttack : Node
        {
            private Enemy enemy;

            public CanAttack(Enemy enemy)
            {
                this.enemy = enemy;
            }

            public override NodeState Evaluate()
            {
                if (Vector3.Distance(Player.Instance.transform.position, enemy.transform.position) <= enemy.attackRange)
                {
                    print("Ready to attack");
                    return NodeState.Success;
                }
                else
                {
                    return NodeState.Failure;
                }
            }
        }

        public class AttackPlayer : Node
        {
            public override NodeState Evaluate()
            {
                print("Attacking player");
                return NodeState.Success;
            }
        }

        public class CheckClosestEnemyDistance : Node
        {
            private float closestRange = 3f;
            private Enemy enemy;

            public CheckClosestEnemyDistance(Enemy enemy)
            {
                this.enemy = enemy;
            }

            public override NodeState Evaluate()
            {
                var enemies = FindObjectsOfType<Enemy>();
                var closestEnemy = enemy.closestEnemy;
                float closestDistance = Mathf.Infinity;
                foreach (var enemy in enemies)
                {
                    if (enemy != this.enemy)
                    {
                        float distance = Vector3.Distance(this.enemy.transform.position, enemy.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestEnemy = enemy;
                        }
                    }
                }
                if (closestDistance < closestRange)
                {
                    print("Closest enemy found");
                    enemy.closestEnemy = closestEnemy;
                    return NodeState.Success;
                }
                else
                {
                    print("No closest enemy found");
                    enemy.closestEnemy = null;
                    return NodeState.Failure;
                }
            }
        }

        public class CopyEnemyPath : Node
        {
            private Enemy enemy;
            private PathfindingManager pathfindingManager;

            public CopyEnemyPath(Enemy enemy)
            {
                this.enemy = enemy;
                this.pathfindingManager = enemy.pathfindingManager;
            }

            public override NodeState Evaluate()
            {
                if (enemy.closestEnemy != null && enemy.closestEnemy.astar.pathNodes.Count > 0 && enemy.astar.pathNodes.Count == 0)
                {
                    print("Copying closest enemy path");

                    foreach (var node in enemy.closestEnemy.astar.pathNodes)
                    {
                        enemy.astar.pathNodes.Add(node);
                    }

                    if (pathfindingManager.waitingAstars.Contains(enemy.astar))
                    {
                        pathfindingManager.waitingAstars.Remove(enemy.astar);
                    }

                    enemy.astar.findPath = false;
                    enemy.astar.state = Astar.PathfindingState.Idle;
                    return NodeState.Success;
                }
                else
                {
                    print("No path to copy");
                    return NodeState.Failure;
                }
            }
        }

        public class AddToPathfindingQueue : Node
        {
            private PathfindingManager pathfindingManager;
            private Astar astar;

            public AddToPathfindingQueue(PathfindingManager pathfindingManager, Astar astar)
            {
                this.pathfindingManager = pathfindingManager;
                this.astar = astar;
            }

            public override NodeState Evaluate()
            {
                if (pathfindingManager.waitingAstars.Contains(astar) || astar.pathNodes.Count > 0)
                {
                    print("Already in queue");
                    return NodeState.Running;
                }
                else
                {
                    print("Adding to queue");
                    pathfindingManager.waitingAstars.Add(astar);
                    return NodeState.Success;
                }
            }
        }

        public class CheckPathFound : Node
        {
            private Astar astar;

            public CheckPathFound(Astar astar)
            {
                this.astar = astar;
            }

            public override NodeState Evaluate()
            {
                if (astar.state == Astar.PathfindingState.Success)
                {
                    print("Path found");
                    return NodeState.Success;
                }
                else if (astar.state == Astar.PathfindingState.Failure)
                {
                    print("Path not found");
                    return NodeState.Failure;
                }
                else
                {
                    print("Pathfinding in progress");
                    return NodeState.Running;
                }
            }
        }

        public class FollowPath : Node
        {
            private float nodeReachedDistance = 1f;
            private Enemy enemy;
            private Transform transform;
            private Astar astar;
            private Rigidbody rb;
            private float speed;

            public FollowPath(Enemy enemy)
            {
                this.enemy = enemy;
                this.transform = enemy.transform;
                this.astar = enemy.astar;
                this.rb = enemy.rb;
                this.speed = enemy.speed;
            }

            public override NodeState Evaluate()
            {
                if (enemy.astar.pathNodes.Count > 0)
                {
                    print("Following path");
                    Vector3 targetPosition = new Vector3(
                        astar.pathNodes[astar.pathNodes.Count - 1].transform.position.x,
                        transform.position.y,
                        astar.pathNodes[astar.pathNodes.Count - 1].transform.position.z
                    );
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 10 * Time.deltaTime);
                    rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime));
                    if (Vector3.Distance(transform.position, astar.pathNodes[astar.pathNodes.Count - 1].transform.position) < nodeReachedDistance)
                    {
                        astar.pathNodes.RemoveAt(astar.pathNodes.Count - 1);
                    }
                    else if (astar.pathNodes[astar.pathNodes.Count - 2] != null && Vector3.Distance(transform.position, astar.pathNodes[astar.pathNodes.Count - 2].transform.position) < nodeReachedDistance)
                    {
                        astar.pathNodes.RemoveAt(astar.pathNodes.Count - 2);
                        astar.pathNodes.RemoveAt(astar.pathNodes.Count - 1);
                    }
                        
                    return NodeState.Running;
                }
                else
                {
                    print("Path ended");
                    return NodeState.Success;
                }
            }
        }
    }
}
