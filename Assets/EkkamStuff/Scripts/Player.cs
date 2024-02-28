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

        [SerializeField] public GameObject itemHolderRight;
        [SerializeField] public GameObject swordHitbox;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            inventory = FindObjectOfType<Inventory>();

            cameraObj = GameObject.FindObjectOfType<Camera>().transform;
            cameraOffset = cameraObj.position - transform.position;

            gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
            initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;

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
                transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, Time.deltaTime * rotationSpeed);
                anim.SetBool("isMoving", true);
            }
            else
            {
                anim.SetBool("isMoving", false);
            }
            
            ControlSpeed();
            CheckForGround();

            // left click for use (temporary, will be changed to new input system)
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                inventory.UseItem();
            }
            
            swordTimer += Time.deltaTime;
            if (swordTimer >= swordResetCooldown)
            {
                anim.SetLayerWeight(1, Mathf.Lerp(anim.GetLayerWeight(1), 0.75f, Time.deltaTime * 10));
            }
            else
            {
                anim.SetLayerWeight(1, 0);
            }

            if (swordTimer >= swordResetCooldown - 0.25f)
            {
                allowMovement = true;
                if (verticalInput != 0 || horizontalInput != 0)
                {
                    anim.SetLayerWeight(1, 0.75f);
                }
            }

            freeWillSlider.value = freeWill;
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

            rb.AddForce(moveDirection * speed * 10f, ForceMode.Force);
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
            }
            else
            {
                isGrounded = false;
                rb.drag = 0;
            }

            // RaycastHit hit2;
            // if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out hit2, groundDistanceLandingOffset + 0.1f))
            // {
            //     if (!isGrounded && !isJumping)
            //     {
            //         anim.SetBool("isJumping", false);
            //     }
            // }
        }

        public async void SwingSword()
        {
            if (swordTimer < swordAttackCooldown || isGrounded == false) return;
            allowMovement = false;
            swordTimer = 0;
            anim.SetTrigger("swordAttack");
            anim.SetLayerWeight(1, 0);
            await Task.Delay(250);
            swordHitbox.SetActive(true);
            rb.AddForce(transform.forward * 3.5f, ForceMode.Impulse);
            await Task.Delay(50);
            swordHitbox.SetActive(false);
        }
    }
}
