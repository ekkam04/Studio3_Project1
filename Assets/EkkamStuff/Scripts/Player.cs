using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Ekkam
{
    public class Player : Damagable
    {
        public Rigidbody rb;
        public Animator anim;
        Inventory inventory;
        CombatManager combatManager;
        
        public bool allowMovement = true;
        public bool allowFall = true;

        public float freeWill = 50f;
        public Slider freeWillSlider;

        public Vector3 viewDirection;
        public Transform orientation;
        public Transform cameraObj;
        private Vector3 cameraOffset;
        public float rotationSpeed = 5f;
        public float horizontalInput = 0f;
        public float verticalInput = 0f;
        Vector3 moveDirection;
        public float speed = 1.0f;
        private float initialSpeed;
        public float maxSpeed = 5.0f;
        public float groundDrag = 3f;

        public float groundDistance = 0.5f;
        public float groundDistanceLandingOffset = 0.2f;
        public bool isGrounded;
        public bool isJumping;
        public bool allowDoubleJump;
        public bool doubleJumped;
        public bool hasLanded;

        public float jumpHeightApex = 2f;
        public float jumpDuration = 1f;
        float currentJumpDuration;
        public float downwardsGravityMultiplier = 1f;
        float gravity;
        private float initialJumpVelocity;
        private float jumpStartTime;

        public float interactDistance = 3f;
        float swordTimer;
        float swordResetCooldown = 1.25f;
        float swordAttackCooldown = 0.25f;

        private float bowTimer;
        private float bowResetCooldown = 1.25f;
        private float bowAttackCooldown = 1.25f;

        private bool targetLock;
        private Enemy previousNearestEnemy;

        [SerializeField] public GameObject itemHolderRight;
        [SerializeField] public GameObject itemHolderLeft;
        [SerializeField] public GameObject arrow;
        [SerializeField] public GameObject spellBall;
        [SerializeField] public GameObject swordHitbox;

        public static Player Instance { get; private set; }

        private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            inventory = FindObjectOfType<Inventory>();
            combatManager = GetComponent<CombatManager>();

            // cameraObj = GameObject.FindObjectOfType<Camera>().transform;
            cameraOffset = cameraObj.position - transform.position;

            gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
            initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;
            initialSpeed = speed;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            // Movement
            viewDirection = transform.position - new Vector3(cameraObj.position.x, transform.position.y, cameraObj.position.z);
            orientation.forward = viewDirection.normalized;

            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
             
            if(moveDirection != Vector3.zero)
            {
                if(!targetLock) transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, Time.deltaTime * rotationSpeed);
                anim.SetBool("isMoving", true);
            }
            else
            {
                anim.SetBool("isMoving", false);
            }
            
            ControlSpeed();
            CheckForGround();

            // temporary, need to use new input system but for now this will do
            if (Input.GetKeyDown(KeyCode.Mouse0)) UseItem();
            if (Input.GetKey(KeyCode.Mouse1)) LookAtNearestEnemy();
            
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                targetLock = false;
                if (previousNearestEnemy != null) previousNearestEnemy.targetLockPrompt.SetActive(false);
            }
            
            swordTimer += Time.deltaTime;
            bowTimer += Time.deltaTime;
            
            if (swordTimer >= swordResetCooldown)
            {
                anim.SetLayerWeight(1, Mathf.Lerp(anim.GetLayerWeight(1), 0.75f, Time.deltaTime * 10));
            }
            else
            {
                anim.SetLayerWeight(1, 0);
            }

            if (swordTimer >= swordResetCooldown - 0.25f && bowTimer >= bowResetCooldown - 0.25f)
            {
                allowMovement = true;
                if (verticalInput != 0 || horizontalInput != 0)
                {
                    anim.SetLayerWeight(1, 0.75f);
                }
            }

            freeWillSlider.value = Mathf.Lerp(freeWillSlider.value, freeWill, Time.deltaTime * 5);
        }

        void FixedUpdate()
        {
            // Move player
            MovePlayer();

            // Jumping
            if (isJumping)
            {
                rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);

                if (Time.time - jumpStartTime >= currentJumpDuration)
                {
                    isJumping = false;
                    hasLanded = false;
                }
            }
            else
            {
                if (!allowFall || isGrounded) return;
                rb.AddForce(Vector3.down * -gravity * downwardsGravityMultiplier, ForceMode.Acceleration);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            horizontalInput = input.x;
            verticalInput = input.y;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (!isGrounded && allowDoubleJump && !doubleJumped)
                {
                    doubleJumped = true;
                    anim.SetTrigger("doubleJump");
                    StartJump(jumpHeightApex, jumpDuration);
                }
                else if (isGrounded)
                {
                    doubleJumped = false;
                    StartJump(jumpHeightApex, jumpDuration);
                }
            }
        }

        public void OnInventoryCycle(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                inventory.CycleSlot(context.ReadValue<float>() > 0);
            }
        }

        void MovePlayer()
        {
            if (!allowMovement) return;
            // Calculate movement direction
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            float moveSpeed = speed;
            if (targetLock)
            {
                moveSpeed = speed / 2f;
            }
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }

        void ControlSpeed()
        {
            // Limit velocity if needed
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            if (flatVelocity.magnitude > maxSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }

        void StartJump(float heightApex, float duration)
        {
            // Recalculate gravity and initial velocity
            gravity = -2 * heightApex / (duration * duration);
            initialJumpVelocity = Mathf.Abs(gravity) * duration;
            currentJumpDuration = duration;

            isJumping = true;
            anim.SetBool("isJumping", true);
            jumpStartTime = Time.time;
            rb.velocity = Vector3.up * initialJumpVelocity;
        }
        
        void CheckForGround()
        {
            RaycastHit hit1;
            if (Physics.BoxCast(transform.position + new Vector3(0, 0.5f, 0), new Vector3(0.5f, 0.5f, 0.5f), Vector3.down, out hit1, Quaternion.identity, groundDistance + 0.1f))
            {
                isGrounded = true;
                rb.drag = groundDrag;

                if (!hasLanded)
                {
                    hasLanded = true;
                    anim.SetBool("isJumping", false);
                }
                
                if (hit1.collider.CompareTag("Movable"))
                {
                    transform.parent = hit1.transform;
                }
            }
            else
            {
                isGrounded = false;
                rb.drag = 0;
                transform.parent = null;
            }
        }

        void LookAtNearestEnemy()
        {
            var enemies = GameObject.FindObjectsOfType<Enemy>();
            var nearestEnemy = enemies[0];
            var nearestDistance = Mathf.Infinity;
            foreach (var enemy in enemies)
            {
                var distance = Vector3.Distance(enemy.transform.position, transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                    if (previousNearestEnemy != null) previousNearestEnemy.targetLockPrompt.SetActive(false);
                    previousNearestEnemy = nearestEnemy;
                }
            }

            if (nearestDistance < 10f)
            {
                targetLock = true;
                nearestEnemy.targetLockPrompt.SetActive(true);
                var viewDirection = nearestEnemy.transform.position - transform.position;
                viewDirection.y = 0;
                transform.forward = Vector3.Slerp(transform.forward, viewDirection.normalized,
                    Time.deltaTime * rotationSpeed);
                print("Locked on to " + nearestEnemy.name);
            }
            else
            {
                targetLock = false;
                if (previousNearestEnemy != null) previousNearestEnemy.targetLockPrompt.SetActive(false);
            }
        }
        
        private void UseItem()
        {
            Item item = inventory.GetSelectedItem();
            if (item == null) return;
            switch (item.tag)
            {
                case "Sword":
                    if (swordTimer < swordAttackCooldown || isGrounded == false) return;
                    allowMovement = false;
                    swordTimer = 0;
                    combatManager.MeleeAttack();
                    break;
                case "Bow":
                    if (bowTimer < bowAttackCooldown || isGrounded == false) return;
                    allowMovement = false;
                    bowTimer = 0;
                    combatManager.ArcherAttack();
                    break;
                case "Staff":
                    if (swordTimer < swordAttackCooldown || isGrounded == false) return;
                    swordTimer = 0;
                    allowMovement = false;
                    combatManager.MageAttack();
                    break;
                default:
                    break;
            }
        }
    }
}
