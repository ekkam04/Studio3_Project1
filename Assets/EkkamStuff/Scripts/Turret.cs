using System;
using UnityEngine;

namespace Ekkam
{
    public class Turret : MonoBehaviour
    {
        public GameObject rotatingBase; // this will rotate on the y axis
        public GameObject rotatingGun; // this will rotate on the x axis
        private Player player;

        private void Start()
        {
            player = Player.Instance;
        }

        private void Update()
        {
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