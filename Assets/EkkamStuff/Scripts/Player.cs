using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Threading.Tasks;
using Cinemachine;
using QFSW.QC;
using Unity.VisualScripting;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

namespace Ekkam
{
    public class Player : Damagable
    {
        Inventory inventory;
        CombatManager combatManager;
        UIManager uiManager;
        
        public enum MovementState
        {
            Walking,
            Sprinting,
            Air
        }
        public MovementState movementState;
        
        public int coins;
        public int tokens;
        
        public enum CameraStyle
        {
            Exploration,
            Combat
        }
        public CameraStyle cameraStyle;
        public Rig bowRig;
        public TwoBoneIKConstraint secondHandArrowIK;
        public float secondHandArrowIKWeight;

        public GameObject[] facePlates;
        
        private bool allowMovement = true;
        public bool allowFall = true;
        
        public bool disguiseActive = false;
        public float disguiseBattery = 60f;
        public Slider disguiseSlider;

        public float freeWill = 50f;
        public Slider freeWillSlider;
        
        public Slider healthSlider;

        public Vector3 viewDirection;
        public Transform orientation;
        
        private Transform cameraObj;
        public GameObject explorationCamera;
        public GameObject combatCamera;
        
        public Transform combatLookAt;
        private Vector3 cameraOffset;
        public float rotationSpeed = 5f;
        
        public float horizontalInput = 0f;
        public float verticalInput = 0f;
        Vector3 moveDirection;
        Vector3 combatRotationDirection;
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
        
        private float staffTimer;
        // private float staffResetCooldown = 1.25f;
        private float staffAttackCooldown = 0.1f;

        private bool targetLock;
        private Enemy previousNearestEnemy;

        [SerializeField] public GameObject itemHolderRight;
        [SerializeField] public GameObject itemHolderLeft;

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
            uiManager = FindObjectOfType<UIManager>();
            combatManager = GetComponent<CombatManager>();
            
            foreach (var facePlate in facePlates)
            {
                facePlate.SetActive(false);
            }
            facePlates[0].SetActive(true);
            disguiseSlider.maxValue = disguiseBattery;
            disguiseSlider.gameObject.SetActive(false);

            cameraObj = explorationCamera.transform;
            cameraStyle = CameraStyle.Exploration;
            cameraOffset = cameraObj.position - transform.position;

            gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
            initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;
            initialSpeed = speed;
        }

        void Update()
        {
            // Movement
            viewDirection = transform.position - new Vector3(cameraObj.position.x, transform.position.y, cameraObj.position.z);

            if (cameraStyle == CameraStyle.Exploration)
            {
                orientation.forward = viewDirection.normalized;
                moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
                
                // if(moveDirection != Vector3.zero)
                // {
                //     transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, Time.deltaTime * rotationSpeed);
                //     // anim.SetBool("isMoving", true);
                // }
                // else
                // {
                //     anim.SetBool("isMoving", false);
                // }
            }
            else if (cameraStyle == CameraStyle.Combat)
            {
                moveDirection = combatLookAt.position - new Vector3(cameraObj.position.x, combatLookAt.position.y, cameraObj.position.z);
                orientation.forward = moveDirection.normalized;
                combatRotationDirection = new Vector3(viewDirection.x, 0, viewDirection.z).normalized;
                // transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, Time.deltaTime * rotationSpeed);
            }
            
            // set isMoving parameter in animator to true if player is moving
            anim.SetBool("isMoving", verticalInput != 0 || horizontalInput != 0);
            
            ControlSpeed();
            CheckForGround();
            MovementStateHandler();

            // temporary, need to use new input system but for now this will do
            if (Input.GetKeyDown(KeyCode.Mouse0)) UseItem();
            if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.L)) LookAtNearestEnemy();
            
            if (Input.GetKeyUp(KeyCode.Mouse1) || Input.GetKeyUp(KeyCode.L))
            {
                targetLock = false;
                if (previousNearestEnemy != null) previousNearestEnemy.targetLockPrompt.SetActive(false);
            }
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleDisguise();
            }
            
            if (disguiseActive)
            {
                disguiseBattery -= Time.deltaTime;
                disguiseSlider.value = disguiseBattery;
                if (disguiseBattery <= 0)
                {
                    disguiseBattery = 0;
                    ToggleDisguise();
                }
            }
            
            swordTimer += Time.deltaTime;
            bowTimer += Time.deltaTime;
            staffTimer += Time.deltaTime;
            
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
            healthSlider.value = Mathf.Lerp(healthSlider.value, health, Time.deltaTime * 5);
            
            secondHandArrowIK.weight = Mathf.Lerp(secondHandArrowIK.weight, secondHandArrowIKWeight, Time.deltaTime * 5);
        }

        void FixedUpdate()
        {
            // Move player
            MovePlayer();
            
            // Orient player
            if (cameraStyle == CameraStyle.Exploration)
            {
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
                }
            }
            else if (cameraStyle == CameraStyle.Combat)
            {
                Quaternion combatTargetRotation = Quaternion.LookRotation(combatRotationDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, combatTargetRotation, Time.fixedDeltaTime * rotationSpeed));
            }

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
                if (!this.enabled) return;
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
            RaycastHit hit;
            // if (Physics.BoxCast(transform.position + new Vector3(0, 0.5f, 0), new Vector3(0.2f, 0.5f, 0.2f), Vector3.down, out hit, Quaternion.identity, groundDistance + 0.1f))
            bool foundGround = Physics.Raycast(transform.position, Vector3.down, out hit, groundDistance + 0.1f);
            if (foundGround)
            {
                isGrounded = true;
                rb.drag = groundDrag;

                if (!hasLanded)
                {
                    hasLanded = true;
                    anim.SetBool("isJumping", false);
                }
                
                if (hit.collider.CompareTag("Movable"))
                {
                    transform.parent = hit.transform;
                    rb.interpolation = RigidbodyInterpolation.None;
                }
            }
            else
            {
                isGrounded = false;
                rb.drag = 0;
                transform.parent = null;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
        
        void MovementStateHandler()
        {
            if (isGrounded && Input.GetKey(KeyCode.LeftShift))
            {
                movementState = MovementState.Sprinting;
            }
            else if (isGrounded)
            {
                movementState = MovementState.Walking;
            }
            else
            {
                movementState = MovementState.Air;
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
                    combatManager.ArcherAttack(item, this);
                    break;
                case "Staff":
                    if (staffTimer < staffAttackCooldown || isGrounded == false) return;
                    staffTimer = 0;
                    combatManager.MageAttack();
                    break;
                case "FireExtinguisher":
                    var fireExtinguisher = item.GetComponent<FireExtinguisher>();
                    fireExtinguisher.Toggle();
                    break;
                default:
                    break;
            }
        }
        
        public override void OnDamageTaken()
        {
            foreach (var facePlate in facePlates)
            {
                facePlate.SetActive(false);
            }
            facePlates[1].SetActive(true);
            Invoke("RevertFacePlate", 1f);
        }

        private void RevertFacePlate()
        {
            foreach (var facePlate in facePlates)
            {
                facePlate.SetActive(false);
            }

            facePlates[0].SetActive(true);
        }
        
        public void ToggleDisguise()
        {
            disguiseActive = !disguiseActive;
            disguiseSlider.gameObject.SetActive(disguiseActive);
            foreach (var facePlate in facePlates)
            {
                facePlate.SetActive(false);
            }
            facePlates[disguiseActive ? 3 : 0].SetActive(true);
        }
        
        public void SwitchCameraStyle(CameraStyle style)
        {
            var combatCamCinemachine = combatCamera.GetComponent<CinemachineFreeLook>();
            var explorationCamCinemachine = explorationCamera.GetComponent<CinemachineFreeLook>();
            
            float combatX = combatCamCinemachine.m_XAxis.Value;
            float combatY = combatCamCinemachine.m_YAxis.Value;
            
            float explorationX = explorationCamCinemachine.m_XAxis.Value;
            float explorationY = explorationCamCinemachine.m_YAxis.Value;
            
            cameraStyle = style;
            switch (style)
            {
                case CameraStyle.Exploration:
                    cameraObj = explorationCamera.transform;
                    explorationCamCinemachine.m_XAxis.Value = combatX;
                    explorationCamCinemachine.m_YAxis.Value = combatY;
                    explorationCamera.SetActive(true);
                    combatCamera.SetActive(false);
                    uiManager.combatReticle.SetActive(false);
                    uiManager.explorationReticle.SetActive(true);
                    break;
                case CameraStyle.Combat:
                    cameraObj = combatCamera.transform;
                    // combatCamCinemachine.m_XAxis.Value = explorationX;
                    // combatCamCinemachine.m_YAxis.Value = explorationY;
                    combatCamera.SetActive(true);
                    explorationCamera.SetActive(false);
                    uiManager.explorationReticle.SetActive(false);
                    uiManager.combatReticle.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }
}
