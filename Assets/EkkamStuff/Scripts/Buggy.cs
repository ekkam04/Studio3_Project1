using System;
using UnityEngine;

namespace Ekkam
{
    public class Buggy : MonoBehaviour
    {
        [SerializeField] WheelCollider frontLeftWheel;
        [SerializeField] WheelCollider frontRightWheel;
        [SerializeField] WheelCollider rearLeftWheel;
        [SerializeField] WheelCollider rearRightWheel;
        
        [SerializeField] Transform frontLeftWheelMesh;
        [SerializeField] Transform frontRightWheelMesh;
        [SerializeField] Transform rearLeftWheelMesh;
        [SerializeField] Transform rearRightWheelMesh;
        
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
            Vector3 pos;
            Quaternion rot;
            wheelCollider.GetWorldPose(out pos, out rot);
            wheelTransform.rotation = rot;
            wheelTransform.position = pos;
        }
        
        public void EnterVehicle()
        {
            Player.Instance.EnterVehicle(drivingPosition);
            isDriving = true;
        }
        
        public void ExitVehicle()
        {
            Player.Instance.ExitVehicle();
            isDriving = false;
            reenableDriveButton.Signal();
        }
    }
}