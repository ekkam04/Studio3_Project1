using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ekkam;
using QFSW.QC;
using Unity.Mathematics;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

namespace Ekkam
{
    public class Enemy : Damagable
    {
        Node rootNode;
        
        public PathfindingGrid grid;
        PathfindingManager pathfindingManager;
        UIManager uiManager;
        AudioSource audioSource;
        public GameObject targetLockPrompt;
        public GameObject parachute;
        
        [Header("--- Astar Pathfinding ---")]
        [SerializeField] Vector2Int startNodePosition;
        [SerializeField] public Vector2Int endNodePosition;
        public List<PathfindingNode> pathNodes = new List<PathfindingNode>();
        public PathfindingNode lastUnblockedNode;
        public bool findingPath = false;
        public enum PathfindingState { Idle, Running, Success, Failure }
        public PathfindingState pathfindingState;
        
        Enemy closestEnemy;
        CombatManager combatManager;

        [Header("--- Enemy Stats ---")]
        public float speed = 4f;
        public float circlingRadius = 5f;
        public float attackRange = 3f;
        public float detectionRange = 25f;
        private float originalDetectionRange;
        private bool canMove = true;
        public float attackTimer;
        public float attackCooldown = 2f;

        private float calculationTimer;
        private float calculationCooldown = 0f;

        private bool shouldMove;
        private Vector3 nextPosition;
        private Quaternion nextRotation;
        
        [Header("--- Enemy Behaviour ---")]
        public bool followsPlayer = true;
        public GameObject targetObjectToWalkTo;
        
        public enum EnemyType
        {
            Melee,
            Archer,
            Mage
        }
        public EnemyType enemyType;

        void Start()
        {
            pathfindingManager = FindObjectOfType<PathfindingManager>();
            uiManager = FindObjectOfType<UIManager>();
            audioSource = GetComponent<AudioSource>();
            var mainCamera = Camera.main;
            
            originalDetectionRange = detectionRange;
            if (grid != null)
            {
                grid.enemiesOnThisGrid.Add(this);
                
                startNodePosition = grid.GetPositionFromWorldPoint(transform.position);
                lastUnblockedNode = grid.GetNode(startNodePosition);
                endNodePosition = grid.GetPositionFromWorldPoint(Player.Instance.transform.position);
            }
            
            targetLockPrompt = Instantiate(uiManager.targetLockPrompt, transform.position, Quaternion.identity, transform);
            targetLockPrompt.GetComponentInChildren<RotationConstraint>().AddSource(new ConstraintSource
            {
                sourceTransform = mainCamera.transform,
                weight = 1
            });
            targetLockPrompt.SetActive(false);

            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            combatManager = GetComponent<CombatManager>();

            rootNode = new Selector(new List<Node>
            {
                // Check for player presence and idle if not present
                new Sequence(new List<Node>
                {
                    new InvertDecorator(new CheckPlayerPresence(this)),
                    new Idle(this)
                }),
                
                // Engage player based on conditions
                new Selector(new List<Node>{
                    new Sequence(new List<Node>{
                        new CanMove(this),
                        new CheckLineOfSight(this),
                        new InvertDecorator(new CanAttack(this)), // Check if not ready to attack
                        new WalkTowardsPlayer(this),
                    }),
                    new Sequence(new List<Node>{
                        // new CheckLineOfSight(grid, astar, transform),
                        new CanAttack(this), // Confirm ready to attack
                        new AttackPlayer(this)
                    }),
                    // Indirect engagement when line of sight is lost
                    new Sequence(new List<Node>{
                        new CanMove(this),
                        new InvertDecorator(new CheckLineOfSight(this)),
                        new Selector(new List<Node>{
                            // new Sequence(new List<Node>{
                            //     new CheckClosestEnemyDistance(this),
                            //     new CopyEnemyPath(this)
                            // }),
                            new Sequence(new List<Node>{
                                new AddToPathfindingQueue(this),
                                new CheckPathFound(this),
                                new FollowPath(this)
                            })
                        })
                    })
                })
            });
        }
        
        void OnPathfindingComplete(NativeList<int2> path)
        {
            Debug.Log("Pathfinding complete");
            findingPath = false;
            if (path.Length < 1)
            {
                pathfindingState = PathfindingState.Failure;
                anim.SetBool("isMoving", false);
            }
            else
            {
                // remove the first node as it is the starting position
                path.RemoveAt(0);
                
                foreach (var pos in path)
                {
                    pathNodes.Add(grid.GetNode(new Vector2Int(pos.x, pos.y)));
                    // Debug.Log("Path for enemy: " + pos);
                }
                pathfindingState = PathfindingState.Success;
            }
        }

        void Update()
        {
            base.Update();
            
            rootNode.Evaluate();
            
            calculationTimer += Time.deltaTime;

            if (!followsPlayer) return;
            var lastPos = grid.GetPositionFromWorldPoint(transform.position);
            if (!grid.GetNode(lastPos).isBlocked)
            {
                lastUnblockedNode = grid.GetNode(lastPos);
            }
            startNodePosition = lastUnblockedNode.gridPosition;
        }

        private void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (shouldMove)
            {
                rb.AddForce((nextPosition - transform.position).normalized * speed * 10f);
                rb.rotation = nextRotation;
                shouldMove = false;
            }
            else
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
        }
        
        public void OnFootstep()
        {
            SoundManager.Instance.PlaySound("footstep-enemy", audioSource);
        }
        
        private Vector3 GetCirclingDirection(Vector3 toPlayer, float angleDegrees)
        {
            Vector3 direction = toPlayer.normalized * circlingRadius;
            Quaternion rotation = Quaternion.AngleAxis(angleDegrees, Vector3.up);
            return rotation * direction;
        }

        public class CheckPlayerPresence : Node
        {
            Enemy enemy;
            private float recalculationDistance = 5f;
            private PathfindingGrid grid;
            private Transform transform;
            private bool canMove;
            private bool followsPlayer;

            public CheckPlayerPresence(Enemy enemy)
            {
                this.enemy = enemy;
                this.grid = enemy.grid;
                this.transform = enemy.transform;
                this.canMove = enemy.canMove;
                this.followsPlayer = enemy.followsPlayer;
            }

            public override NodeState Evaluate()
            {
                if (Player.Instance.disguiseActive)
                {
                    return NodeState.Failure;
                }
                
                if (
                    ((grid != null && grid.ObjectIsOnGrid(Player.Instance.transform.position)) || !followsPlayer)
                    && (Vector3.Distance(transform.position, Player.Instance.transform.position) < enemy.detectionRange
                    || enemy.pathNodes.Count > 0)
                )
                {
                    // print("Player is present");

                    if (
                        canMove &&
                        followsPlayer &&
                        enemy.calculationTimer >= enemy.calculationCooldown &&
                        grid.GetDistance(
                        grid.GetPositionFromWorldPoint(Player.Instance.transform.position),
                        enemy.endNodePosition
                        ) > recalculationDistance
                    )
                    {
                        enemy.calculationTimer = 0;
                        enemy.endNodePosition = grid.GetPositionFromWorldPoint(Player.Instance.transform.position);
                        enemy.pathNodes.Clear();
                        enemy.pathfindingState = Enemy.PathfindingState.Idle;
        
                        if (grid.GetNode(enemy.endNodePosition).isBlocked)
                        {
                            // print("End node is blocked");
                        }
                    }

                    return NodeState.Success;
                }
                else
                {
                    // print("Player is not present");
                    return NodeState.Failure;
                }
            }
        }

        public class Idle : Node
        {
            Enemy enemy;
            
            public Idle(Enemy enemy)
            {
                this.enemy = enemy;
            }
            public override NodeState Evaluate()
            {
                // print("Idle");
                enemy.anim.SetBool("isMoving", false);
                return NodeState.Success;
            }
        }

        public class CheckLineOfSight : Node
        {
            private Enemy enemy;
            private PathfindingGrid grid;
            private Transform transform;

            public CheckLineOfSight(Enemy enemy)
            {
                this.enemy = enemy;
                this.grid = enemy.grid;
                this.transform = enemy.transform;
            }

            public override NodeState Evaluate()
            {
                if (grid.HasDirectLineOfSight(
                    grid.GetPositionFromWorldPoint(transform.position),
                    grid.GetPositionFromWorldPoint(Player.Instance.transform.position)
                ))
                {
                    // print("Line of sight");
                    enemy.detectionRange = enemy.originalDetectionRange;
                    return NodeState.Success;
                }
                else
                {
                    // print("No line of sight");
                    return NodeState.Failure;
                }
            }
        }

        public class WalkTowardsPlayer : Node
        {
            private Enemy enemy;
            private Transform transform;
            private Rigidbody rb;
            private float speed;

            public WalkTowardsPlayer(Enemy enemy)
            {
                this.enemy = enemy;
                this.transform = enemy.transform;
                this.rb = enemy.rb;
                this.speed = enemy.speed;
            }

            // public override NodeState Evaluate()
            // {
            //     // print("Walking towards player");
            //     enemy.anim.SetBool("isMoving", true);
            //     Vector3 targetPosition = new Vector3(Player.Instance.transform.position.x, transform.position.y, Player.Instance.transform.position.z);
            //     
            //     // transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 10 * Time.deltaTime);
            //     // rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime));
            //     enemy.nextRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 10 * Time.deltaTime);
            //     enemy.nextPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            //     enemy.shouldMove = true;
            //     
            //     return NodeState.Success;
            // }
            public override NodeState Evaluate()
            {
                // print("Walking towards player");
                enemy.anim.SetBool("isMoving", true);

                Vector3 toPlayer = Player.Instance.transform.position - transform.position;
                toPlayer.y = 0;

                float distanceToPlayer = toPlayer.magnitude;
                Vector3 targetPosition;

                if (distanceToPlayer > enemy.circlingRadius + 1.0f)
                {
                    // Direct approach to maintain circling radius distance
                    targetPosition = Vector3.MoveTowards(transform.position, Player.Instance.transform.position, enemy.speed * Time.deltaTime);
                }
                else
                {
                    // Circling behavior
                    Vector3 circlingDirection = enemy.GetCirclingDirection(toPlayer, 90); // Right-hand circling
                    targetPosition = transform.position + circlingDirection * Time.deltaTime * enemy.speed;
                }

                enemy.nextRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer), 10 * Time.deltaTime);
                enemy.nextPosition = targetPosition;
                enemy.shouldMove = true;

                return NodeState.Success;
            }
        }

        public class CanMove : Node
        {
            private Enemy enemy;

            public CanMove(Enemy enemy)
            {
                this.enemy = enemy;
            }

            public override NodeState Evaluate()
            {
                if (enemy.canMove && enemy.followsPlayer)
                {
                    // print("Can move");
                    return NodeState.Success;
                }
                else
                {
                    // print("Can't move");
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
                float distanceToPlayer = Vector3.Distance(Player.Instance.transform.position, enemy.transform.position);
                
                // if (Vector3.Distance(Player.Instance.transform.position, enemy.transform.position) <= enemy.attackRange)
                // {
                //     if (!enemy.followsPlayer)
                //     {
                //         var heightOffset = new Vector3(0, 1, 0);
                //         if (Physics.Raycast(
                //                 enemy.transform.position + heightOffset,
                //                 (Player.Instance.transform.position + heightOffset) - (enemy.transform.position + heightOffset),
                //                 out RaycastHit hit,
                //                 enemy.attackRange * 2f
                //            )
                //         )
                //         {
                //             if (hit.collider.gameObject.layer != 6)
                //             {
                //                 // print("Obstacle in the way");
                //                 return NodeState.Failure;
                //             }
                //         }
                //     }
                //     
                //     // print("Ready to attack");
                //     enemy.canMove = false;
                //     enemy.pathNodes.Clear();
                //     enemy.pathfindingState = Enemy.PathfindingState.Idle;
                //     return NodeState.Success;
                // }
                // else
                // {
                //     enemy.canMove = true;
                //     return NodeState.Failure;
                // }
                
                if (distanceToPlayer <= enemy.attackRange)
                {
                    if (!enemy.followsPlayer)
                    {
                        var heightOffset = new Vector3(0, 1, 0);
                        if (Physics.Raycast(
                                enemy.transform.position + heightOffset,
                                (Player.Instance.transform.position + heightOffset) - (enemy.transform.position + heightOffset),
                                out RaycastHit hit,
                                enemy.attackRange * 2f
                           )
                        )
                        {
                            if (hit.collider.gameObject.layer != 6)
                            {
                                // print("Obstacle in the way");
                                return NodeState.Failure;
                            }
                        }
                    }
                    
                    // print("Ready to attack");
                    enemy.canMove = false;
                    enemy.pathNodes.Clear();
                    enemy.pathfindingState = Enemy.PathfindingState.Idle;
                    return NodeState.Success;
                }
                else
                {
                    enemy.canMove = true;
                    return NodeState.Failure;
                }
            }
        }

        public class AttackPlayer : Node
        {
            Enemy enemy;
            
            public AttackPlayer(Enemy enemy)
            {
                this.enemy = enemy;
            }
            // public override NodeState Evaluate()
            // {
            //     // print("Attacking player");
            //     enemy.anim.SetBool("isMoving", false);
            //     enemy.shouldMove = false;
            //     
            //     Vector3 targetPosition = new Vector3(
            //         Player.Instance.transform.position.x,
            //         enemy.transform.position.y,
            //         Player.Instance.transform.position.z
            //     );
            //     enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(targetPosition - enemy.transform.position), 10 * Time.deltaTime);
            //     
            //     enemy.attackTimer += Time.deltaTime;
            //     if (enemy.attackTimer >= enemy.attackCooldown)
            //     {
            //         enemy.attackTimer = 0;
            //         
            //         switch (enemy.enemyType)
            //         {
            //             case Enemy.EnemyType.Melee:
            //                 enemy.combatManager.MeleeAttack();
            //                 break;
            //             case Enemy.EnemyType.Archer:
            //                 // enemy.combatManager.ArcherAttack();
            //                 break;
            //             case Enemy.EnemyType.Mage:
            //                 enemy.combatManager.MageAttack();
            //                 break;
            //         }
            //         
            //         return NodeState.Success;
            //     }
            //     else
            //     {
            //         return NodeState.Running;
            //     }
            // }
            public override NodeState Evaluate()
            {
                // print("Attacking player");
                enemy.anim.SetBool("isMoving", false);
                enemy.shouldMove = false;
        
                Vector3 targetPosition = Player.Instance.transform.position;
                targetPosition.y = enemy.transform.position.y;
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(targetPosition - enemy.transform.position), 10 * Time.deltaTime);

                enemy.attackTimer += Time.deltaTime;
                if (enemy.attackTimer >= enemy.attackCooldown)
                {
                    enemy.attackTimer = 0;
                    switch (enemy.enemyType)
                    {
                        case Enemy.EnemyType.Melee:
                            enemy.combatManager.MeleeAttack();
                            break;
                        case Enemy.EnemyType.Mage:
                            enemy.combatManager.MageAttack();
                            break;
                        default:
                            break;
                    }
                    return NodeState.Success;
                }
                else
                {
                    return NodeState.Running;
                }
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
                    // print("Closest enemy found");
                    enemy.closestEnemy = closestEnemy;
                    return NodeState.Success;
                }
                else
                {
                    // print("No closest enemy found");
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
                if (enemy.closestEnemy != null && enemy.closestEnemy.pathNodes.Count > 0 && enemy.pathNodes.Count == 0)
                {
                    // print("Copying closest enemy path");
        
                    foreach (var node in enemy.closestEnemy.pathNodes)
                    {
                        enemy.pathNodes.Add(node);
                    }
        
                    if (enemy.findingPath)
                    {
                        enemy.findingPath = false;
                    }
                    
                    return NodeState.Success;
                }
                else
                {
                    // print("No path to copy");
                    return NodeState.Failure;
                }
            }
        }

        public class AddToPathfindingQueue : Node
        {
            private Enemy enemy;
            private PathfindingManager pathfindingManager;
        
            public AddToPathfindingQueue(Enemy enemy)
            {
                this.enemy = enemy;
                this.pathfindingManager = enemy.pathfindingManager;
            }
        
            public override NodeState Evaluate()
            {
                if (enemy.pathfindingState == Enemy.PathfindingState.Failure)
                {
                    return NodeState.Failure;
                }
                
                if (!enemy.findingPath && enemy.pathNodes.Count < 1)
                {
                    int2 startPos = new int2(enemy.startNodePosition.x, enemy.startNodePosition.y);
                    int2 endPos = new int2(enemy.grid.GetPositionFromWorldPoint(Player.Instance.transform.position).x, enemy.grid.GetPositionFromWorldPoint(Player.Instance.transform.position).y);
                    pathfindingManager.RequestPath(
                        startPos,
                        endPos,
                        new int2(enemy.grid.gridCellCountX, enemy.grid.gridCellCountZ),
                        enemy.grid.GetBlockedPositions(),
                        enemy.OnPathfindingComplete
                    );
                    enemy.findingPath = true;
                    enemy.pathfindingState = Enemy.PathfindingState.Running;
                    return NodeState.Success;
                }
                else
                {
                    return NodeState.Running;
                }
            }
        }

        public class CheckPathFound : Node
        {
            private Enemy enemy;
        
            public CheckPathFound(Enemy enemy)
            {
                this.enemy = enemy;
            }
        
            public override NodeState Evaluate()
            {
                if (enemy.pathfindingState == Enemy.PathfindingState.Success)
                {
                    // print("Path found");
                    return NodeState.Success;
                }
                else if (enemy.pathfindingState == Enemy.PathfindingState.Failure)
                {
                    // print("Path not found");
                    return NodeState.Failure;
                }
                else
                {
                    // print("Pathfinding in progress");
                    return NodeState.Running;
                }
            }
        }

        public class FollowPath : Node
        {
            private float nodeReachedDistance = 0.85f;
            private Enemy enemy;
            private Transform transform;
            private Rigidbody rb;
            private float speed;

            public FollowPath(Enemy enemy)
            {
                this.enemy = enemy;
                this.transform = enemy.transform;
                this.rb = enemy.rb;
                this.speed = enemy.speed;
            }
            
            public override NodeState Evaluate()
            {
                if (enemy.pathNodes.Count > 0)
                {
                    // print("Following path");
                    enemy.anim.SetBool("isMoving", true);
                    Vector3 targetPosition = new Vector3(
                        enemy.pathNodes[enemy.pathNodes.Count - 1].transform.position.x,
                        transform.position.y,
                        enemy.pathNodes[enemy.pathNodes.Count - 1].transform.position.z
                    );
                    
                    enemy.nextRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 10 * Time.deltaTime);
                    enemy.nextPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                    enemy.shouldMove = true;
                    
                    if (Vector3.Distance(transform.position, enemy.pathNodes[enemy.pathNodes.Count - 1].transform.position) < nodeReachedDistance)
                    {
                        enemy.pathNodes.RemoveAt(enemy.pathNodes.Count - 1);
                    }
                    else if (enemy.pathNodes.Count > 1 && enemy.pathNodes[enemy.pathNodes.Count - 2] != null && Vector3.Distance(transform.position, enemy.pathNodes[enemy.pathNodes.Count - 2].transform.position) < nodeReachedDistance)
                    {
                        enemy.pathNodes.RemoveAt(enemy.pathNodes.Count - 2);
                        enemy.pathNodes.RemoveAt(enemy.pathNodes.Count - 1);
                    }
                        
                    return NodeState.Running;
                }
                else
                {
                    // print("Path ended");
                    return NodeState.Success;
                }
            }

        }
    }
}