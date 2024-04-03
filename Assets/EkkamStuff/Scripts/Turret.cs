using System;
using UnityEngine;

namespace Ekkam
{
    public class Turret : MonoBehaviour
    {
        public GameObject rotatingBase; // this will rotate on the y axis
        public GameObject rotatingGun; // this will rotate on the x axis
        public Vector2 baseRotationLimits = new Vector2(-360, 360);
        public Vector2 gunRotationLimits = new Vector2(-360, 360);
        private Player player;

        private void Start()
        {
            player = Player.Instance;
        }

        private void Update()
        {
            // lock rotation limits
            if (rotatingBase.transform.rotation.eulerAngles.y > baseRotationLimits.y)
            {
                rotatingBase.transform.rotation = Quaternion.Euler(rotatingBase.transform.rotation.eulerAngles.x, baseRotationLimits.y, rotatingBase.transform.rotation.eulerAngles.z);
            }
            if (rotatingBase.transform.rotation.eulerAngles.y < baseRotationLimits.x)
            {
                rotatingBase.transform.rotation = Quaternion.Euler(rotatingBase.transform.rotation.eulerAngles.x, baseRotationLimits.x, rotatingBase.transform.rotation.eulerAngles.z);
            }
            if (rotatingGun.transform.rotation.eulerAngles.x > gunRotationLimits.y)
            {
                rotatingGun.transform.rotation = Quaternion.Euler(gunRotationLimits.y, rotatingGun.transform.rotation.eulerAngles.y, rotatingGun.transform.rotation.eulerAngles.z);
            }
            if (rotatingGun.transform.rotation.eulerAngles.x < gunRotationLimits.x)
            {
                rotatingGun.transform.rotation = Quaternion.Euler(gunRotationLimits.x, rotatingGun.transform.rotation.eulerAngles.y, rotatingGun.transform.rotation.eulerAngles.z);
            }
            
            // base rotation Y
            Vector3 directionToTarget = player.transform.position - transform.position;
            directionToTarget.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            rotatingBase.transform.rotation = Quaternion.Slerp(rotatingBase.transform.rotation, targetRotation, Time.deltaTime * 5f);
            
            // gun rotation X
            Vector3 directionToTargetGun = player.transform.position - rotatingGun.transform.position;
            Quaternion targetRotationGun = Quaternion.LookRotation(directionToTargetGun);
            rotatingGun.transform.rotation = Quaternion.Slerp(rotatingGun.transform.rotation, targetRotationGun, Time.deltaTime * 5f);
            
        }
    }
}