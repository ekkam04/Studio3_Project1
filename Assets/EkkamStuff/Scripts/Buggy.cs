using System;
using UnityEngine;

namespace Ekkam
{
    public class Buggy : MonoBehaviour
    {
        Rigidbody rb;
        
        [SerializeField] WheelCollider frontLeftWheel;
        [SerializeField] WheelCollider frontRightWheel;
        [SerializeField] WheelCollider rearLeftWheel;
        [SerializeField] WheelCollider rearRightWheel;
        
        [SerializeField] Transform frontLeftWheelMesh;
        [SerializeField] Transform frontRightWheelMesh;
        [SerializeField] Transform rearLeftWheelMesh;
        [SerializeField] Transform rearRightWheelMesh;
        
        [SerializeField] Interactable frontLeftWheelHub;
        [SerializeField] Interactable frontRightWheelHub;
        [SerializeField] Interactable rearLeftWheelHub;
        [SerializeField] Interactable rearRightWheelHub;
        
        public bool startWithWheels = true;
        private bool hasWheels;
        public GameObject repairSupport;
        public Signalable[] signalOnRepairComplete;
        
        public Transform drivingPosition;
        public Transform exitPosition;
        
        public float maxSteerAngle = 15;
        public float acceleration = 500f;
        public float brakingForce = 300f;
        
        public bool isDriving;
        
        public Action reenableDriveButton;
        
        private float currentAcceleration;
        private float currentBrakingForce;
        
        private float horizontalInput;
        private float verticalInput;
        
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            
            frontLeftWheel.ConfigureVehicleSubsteps(5, 12, 15);
            frontRightWheel.ConfigureVehicleSubsteps(5, 12, 15);
            rearLeftWheel.ConfigureVehicleSubsteps(5, 12, 15);
            rearRightWheel.ConfigureVehicleSubsteps(5, 12, 15);

            if (!startWithWheels)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
                Destroy(frontRightWheelHub.transform.GetChild(0).gameObject);
                frontRightWheel.enabled = false;
                Destroy(frontLeftWheelHub.transform.GetChild(0).gameObject);
                frontLeftWheel.enabled = false;
                Destroy(rearRightWheelHub.transform.GetChild(0).gameObject);
                rearRightWheel.enabled = false;
                Destroy(rearLeftWheelHub.transform.GetChild(0).gameObject);
                rearLeftWheel.enabled = false;
            }
            
            Invoke(nameof(CheckForWheels), 1f);
        }

        private void Update()
        {
            if (isDriving)
            {
                horizontalInput = Input.GetAxis("Horizontal");
                verticalInput = Input.GetAxis("Vertical");
                currentBrakingForce = 0;
            }
            else
            {
                horizontalInput = 0;
                verticalInput = 0;
                currentBrakingForce = brakingForce;
            }
            
            if (Input.GetKey(KeyCode.Escape) && isDriving)
            {
                ExitVehicle();
            }
        }

        private void FixedUpdate()
        {
            if (!hasWheels) return;
            
            frontLeftWheel.steerAngle = maxSteerAngle * horizontalInput;
            frontRightWheel.steerAngle = maxSteerAngle * horizontalInput;
            
            currentAcceleration = verticalInput * acceleration;
            
            frontLeftWheel.motorTorque = currentAcceleration;
            frontRightWheel.motorTorque = currentAcceleration;
            
            frontLeftWheel.brakeTorque = currentBrakingForce;
            frontRightWheel.brakeTorque = currentBrakingForce;
            rearLeftWheel.brakeTorque = currentBrakingForce;
            rearRightWheel.brakeTorque = currentBrakingForce;
            
            UpdateWheelMesh(frontLeftWheel, frontLeftWheelMesh);
            UpdateWheelMesh(frontRightWheel, frontRightWheelMesh);
            UpdateWheelMesh(rearLeftWheel, rearLeftWheelMesh);
            UpdateWheelMesh(rearRightWheel, rearRightWheelMesh);
        }
        
        private void UpdateWheelMesh(WheelCollider wheelCollider, Transform wheelTransform)
        {
            if (!wheelTransform) return;
            Vector3 pos;
            Quaternion rot;
            wheelCollider.GetWorldPose(out pos, out rot);
            wheelTransform.rotation = rot;
            wheelTransform.position = pos;
        }
        
        public void EnterVehicle()
        {
            Player.Instance.EnterVehicle(drivingPosition, this);
            isDriving = true;
        }
        
        public void ExitVehicle()
        {
            Player.Instance.ExitVehicle();
            isDriving = false;
            reenableDriveButton.Signal();
        }
        
        public void CheckForWheels()
        {
            // check if wheel hubs have a child object
            Transform frontLeftHubChild = frontLeftWheelHub.transform.childCount > 0 ? frontLeftWheelHub.transform.GetChild(0) : null;
            if (frontLeftHubChild && frontLeftWheelMesh != frontLeftHubChild)
            {
                frontLeftWheelMesh = frontLeftHubChild;
                frontLeftWheelHub.enabled = false;
                frontLeftWheel.enabled = true;
            }
            
            Transform frontRightHubChild = frontRightWheelHub.transform.childCount > 0 ? frontRightWheelHub.transform.GetChild(0) : null;
            if (frontRightHubChild && frontRightWheelMesh != frontRightHubChild)
            {
                frontRightWheelMesh = frontRightHubChild;
                frontRightWheelHub.enabled = false;
                frontRightWheel.enabled = true;
            }
            
            Transform rearLeftHubChild = rearLeftWheelHub.transform.childCount > 0 ? rearLeftWheelHub.transform.GetChild(0) : null;
            if (rearLeftHubChild && rearLeftWheelMesh != rearLeftHubChild)
            {
                rearLeftWheelMesh = rearLeftHubChild;
                rearLeftWheelHub.enabled = false;
                rearLeftWheel.enabled = true;
            }
            
            Transform rearRightHubChild = rearRightWheelHub.transform.childCount > 0 ? rearRightWheelHub.transform.GetChild(0) : null;
            if (rearRightHubChild && rearRightWheelMesh != rearRightHubChild)
            {
                rearRightWheelMesh = rearRightHubChild;
                rearRightWheelHub.enabled = false;
                rearRightWheel.enabled = true;
            }
            
            if (frontLeftWheelMesh && frontRightWheelMesh && rearLeftWheelMesh && rearRightWheelMesh)
            {
                print("All wheels are set!");
                if (repairSupport != null) repairSupport.SetActive(false);
                hasWheels = true;
                rb.isKinematic = false;
                rb.useGravity = true;
                reenableDriveButton.Signal();
                foreach (Signalable signalable in signalOnRepairComplete)
                {
                    signalable.Signal();
                }
            }
        }
    }
}