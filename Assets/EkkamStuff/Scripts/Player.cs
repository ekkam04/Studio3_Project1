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
        AudioSource audioSource;
        
        public enum MovementState
        {
            Walking,
            Sprinting,
            Air
        }
        [Header("--- Player Settings and References ---")]
        public MovementState movementState;
        
        public float energy = 100f;
        public float maxEnergy = 100f;
        public Slider energySlider;
        
        public float sprintEnergyDrain = 10f;
        public float sprintEnergyRecharge = 10f;
        public AudioSource hoverSound;
        
        public int coins;
        public int tokens;
        
        public enum CameraStyle
        {
            Exploration,
            Combat
        }
        public CameraStyle cameraStyle;
        
        private bool allowMovement = true;
        public bool allowFall = true;
        
        public bool disguiseActive = false;
        public float disguiseBattery = 60f;
        public Slider disguiseSlider;

        public float freeWill = 50f;
        public Slider freeWillSlider;
        
        public float interactDistance = 3f;
        
        public GameObject[] facePlates;
        
        public Slider healthSlider;

        public Vector3 viewDirection;
        public Transform orientation;
        
        private Transform cameraObj;
        public GameObject explorationCamera;
        public GameObject combatCamera;
        
        public Transform combatLookAt;
        private Vector3 cameraOffset;
        
        public GameObject playerSilhouette;
        private float silhouetteTimer;
        
        public float freeFlowSphereCastRadius = 2f;
        public float freeFlowSphereCastDistance = 5f;
        public float freeFlowLeapSpeedMultiplier = 3f;
        
        private Enemy attackTarget = null;
        private bool isAttacking = false;
        private float attackStopDistance = 1.5f;

        public ParticleSystem hoverParticlesL;
        public ParticleSystem hoverParticlesR;
        
        [Header("--- Rig Settings ---")]
        public Rig bowRig;
        public TwoBoneIKConstraint secondHandArrowIK;
        public float secondHandArrowIKWeight;
        
        [Header("--- Movement Settings ---")]
        public float rotationSpeed = 5f;
        public float horizontalInput = 0f;
        public float verticalInput = 0f;
        Vector3 moveDirection;
        Vector3 combatRotationDirection;
        private float speed = 3.0f;
        private float maxSpeed = 5.0f;
        public float walkSpeed = 3.0f;
        public float sprintSpeed = 5.0f;
        public float maxSpeedOffset = 2.0f;
        public float groundDrag = 3f;

        public float groundDistance = 0.5f;
        public float groundDistanceLandingOffset = 0.2f;
        public bool isGrounded;
        public bool isJumping;
        public bool isSprinting;
        public bool allowDoubleJump;
        public bool doubleJumped;
        public bool hasLanded;

        public float jumpHeightApex = 2f;
        public float jumpDuration = 1f;
        float currentJumpDuration;
        private float downwardsGravityMultiplier = 1.5f;
        public float normalDownwardsGravityMultiplier = 1.5f;
        public float hoveringDownwardsGravityMultiplier = 0.5f;
        float gravity;
        private float initialJumpVelocity;
        private float jumpStartTime;
        
        // --- constant values ---
        float swordTimer;
        float swordResetCooldown = 1.25f;
        float swordAttackCooldown = 0.25f;
        
        private float bowTimer;
        private float bowResetCooldown = 1.25f;
        private float bowAttackCooldown = 1.25f;
        
        private float staffTimer;
        private float staffAttackCooldown = 0.1f;

        private bool targetLock;
        private Enemy previousNearestEnemy;
        
        [Header("--- Model Settings ---")]

        [SerializeField] public GameObject itemHolderRight;
        [SerializeField] public GameObject itemHolderLeft;
        
        public SkinnedMeshRenderer playerMesh;
        
        public DisguiseDetails[] disguiseDetails;
        public ParticleSystem disguiseParticles;

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
            audioSource = GetComponent<AudioSource>();
            
            energySlider.maxValue = maxEnergy;
            
            disguiseSlider.maxValue = disguiseBattery;
            disguiseSlider.gameObject.SetActive(false);

            cameraObj = explorationCamera.transform;
            cameraStyle = CameraStyle.Exploration;
            cameraOffset = cameraObj.position - transform.position;

            gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
            initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;
        }

        void Update()
        {
            base.Update();
            
            // Movement
            viewDirection = transform.position - new Vector3(cameraObj.position.x, transform.position.y, cameraObj.position.z);

            if (cameraStyle == CameraStyle.Exploration)
            {
                orientation.forward = viewDirection.normalized;
                moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
            }
            else if (cameraStyle == CameraStyle.Combat)
            {
                moveDirection = combatLookAt.position - new Vector3(cameraObj.position.x, combatLookAt.position.y, cameraObj.position.z);
                orientation.forward = moveDirection.normalized;
                combatRotationDirection = new Vector3(viewDirection.x, 0, viewDirection.z).normalized;
            }
            
            anim.SetBool("isMoving", verticalInput != 0 || horizontalInput != 0);
            anim.SetBool("isHovering", isSprinting);
            float rbVelocity2D = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
            float rbVelocity2DNormalized = rbVelocity2D / maxSpeed;
            anim.SetFloat("hoverTilt", Mathf.Lerp(anim.GetFloat("hoverTilt"), rbVelocity2DNormalized, Time.deltaTime * 5));
            
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (isSprinting)
                {
                    isSprinting = false;
                }
                else if (energy > 15f)
                {
                    isSprinting = true;
                }
            }
            
            speed = isSprinting ? sprintSpeed : walkSpeed;
            maxSpeed = speed + maxSpeedOffset;
            downwardsGravityMultiplier = isSprinting ? hoveringDownwardsGravityMultiplier : normalDownwardsGravityMultiplier;
            ControlSpeed();
            CheckForGround();
            MovementStateHandler();
            
            if (isSprinting)
            {
                energy -= sprintEnergyDrain * Time.deltaTime;
                if (energy <= 0)
                {
                    energy = 0;
                    isSprinting = false;
                }
            }
            else
            {
                energy += sprintEnergyRecharge * Time.deltaTime;
                if (energy > maxEnergy) energy = maxEnergy;
            }
            
            if (isSprinting && (verticalInput != 0 || horizontalInput != 0))
            {
                if (hoverParticlesL.isStopped) hoverParticlesL.Play();
                if (hoverParticlesR.isStopped) hoverParticlesR.Play();
                if (!hoverSound.isPlaying)
                {
                    hoverSound.volume = 0.75f;
                    hoverSound.Play();
                }
            }
            else
            {
                if (hoverParticlesL.isPlaying) hoverParticlesL.Stop();
                if (hoverParticlesR.isPlaying) hoverParticlesR.Stop();
                if (hoverSound.isPlaying)
                {
                    hoverSound.volume -= Time.deltaTime * 2;
                    if (hoverSound.volume <= 0)
                    {
                        hoverSound.Stop();
                    }
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Mouse0)) UseItem();
            
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
            silhouetteTimer += Time.deltaTime;
            
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
            energySlider.value = Mathf.Lerp(energySlider.value, energy, Time.deltaTime * 5);
            
            secondHandArrowIK.weight = Mathf.Lerp(secondHandArrowIK.weight, secondHandArrowIKWeight, Time.deltaTime * 5);
        }

        void FixedUpdate()
        {
            base.FixedUpdate();
            
            // Move player
            if (isAttacking)
            {
                MoveTowardsAttackTarget();
                if (silhouetteTimer > 0.075f)
                {
                    GameObject newPlayerSilhouette = Instantiate(playerSilhouette, transform.position, transform.rotation);
                    silhouetteTimer = 0f;
                }
            }
            else
            {
                MovePlayer();
            }
            
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
                    SoundManager.Instance.PlaySound("jump", audioSource);
                }
                else if (isGrounded)
                {
                    doubleJumped = false;
                    StartJump(jumpHeightApex, jumpDuration);
                    SoundManager.Instance.PlaySound("jump", audioSource);
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
        
        void MoveTowardsAttackTarget()
        {
            if (attackTarget != null && Vector3.Distance(transform.position, attackTarget.transform.position) > attackStopDistance)
            {
                Vector3 moveDirection = Vector3.MoveTowards(transform.position, attackTarget.transform.position, speed * freeFlowLeapSpeedMultiplier * Time.fixedDeltaTime);
                rb.MovePosition(moveDirection);
                
                Vector3 lookDirection = attackTarget.transform.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed * freeFlowLeapSpeedMultiplier));
            }
            else
            {
                isAttacking = false;
                attackTarget = null;
                combatManager.MeleeAttack();
            }
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
                    SoundManager.Instance.PlaySound("land", audioSource);
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
            if (isGrounded && isSprinting)
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
        
        private void UseItem()
        {
            Item item = inventory.GetSelectedItem();
            if (item == null) return;
            switch (item.tag)
            {
                case "Sword":
                    if (swordTimer < swordAttackCooldown || isGrounded == false || isAttacking) return;
                    allowMovement = false;
                    swordTimer = 0;
                    if (CheckForNearbyEnemies())
                    {
                        PerformFreeFlowAttack();
                    }
                    else
                    {
                        combatManager.MeleeAttack();
                    }
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
        
        bool CheckForNearbyEnemies()
        {
            float detectionRadius = 10f;
            LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayerMask);
            return hitColliders.Length > 0;
        }
        
        void PerformFreeFlowAttack()
        {
            RaycastHit hit;
            float sphereCastRadius = freeFlowSphereCastRadius;
            float sphereCastDistance = freeFlowSphereCastDistance;
            Vector3 sphereCastDirection = orientation.forward;
            LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");
            if (Physics.SphereCast(transform.position, sphereCastRadius, sphereCastDirection, out hit, sphereCastDistance, enemyLayerMask))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    attackTarget = enemy;
                    isAttacking = true;
                }
            }
            else
            {
                combatManager.MeleeAttack();
            }
        }
        
        public override void OnDamageTaken()
        {
            uiManager.PulseVignette(Color.red, 0.25f, 0.5f);
            foreach (var facePlate in facePlates)
            {
                facePlate.SetActive(false);
            }
            facePlates[1].SetActive(true);
            Invoke("RevertFacePlate", 1f);
        }

        public override void OnDeath()
        {
            CheckpointManager.Instance.LoadCheckpointData();
        }

        public override void OnHeal()
        {
            uiManager.PulseVignette(Color.green, 0.25f, 5f);
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
            foreach (var disguiseDetail in disguiseDetails)
            {
                disguiseDetail.facePlate.SetActive(false);
            }
            disguiseDetails[disguiseActive ? 1 : 0].facePlate.SetActive(true);
            playerMesh.material = disguiseDetails[disguiseActive ? 1 : 0].material;
            var disguiseParticlesMain = disguiseParticles.main;
            disguiseParticlesMain.startColor = disguiseDetails[disguiseActive ? 1 : 0].color;
            disguiseParticles.Play();
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
                    // explorationCamCinemachine.m_XAxis.Value = combatX;
                    // explorationCamCinemachine.m_YAxis.Value = combatY;
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
        
        public void OnFootstep()
        {
            if (isGrounded)
            {
                SoundManager.Instance.PlaySound("footstep", audioSource);
            }
        }
        
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            Vector3 sphereCastDirection = orientation.forward;
            float sphereCastRadius = freeFlowSphereCastRadius;
            float sphereCastDistance = freeFlowSphereCastDistance;

            Vector3 startSphereCenter = transform.position;
            Vector3 endSphereCenter = transform.position + sphereCastDirection * sphereCastDistance;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(startSphereCenter, endSphereCenter);
            Gizmos.DrawWireSphere(startSphereCenter, sphereCastRadius);
            Gizmos.DrawWireSphere(endSphereCenter, sphereCastRadius);
            
            for (float i = 0; i <= sphereCastDistance; i += sphereCastDistance / 5)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCastDirection * i, sphereCastRadius);
            }
        }
    }

    [System.Serializable]
    public class DisguiseDetails
    {
        public string name;
        public GameObject facePlate;
        public Material material;
        public Color color;
    }
}
