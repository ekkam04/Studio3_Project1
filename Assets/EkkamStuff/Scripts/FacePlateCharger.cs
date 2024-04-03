using UnityEngine;

namespace Ekkam
{
    public class FacePlateCharger : MonoBehaviour
    {
        public float charge = 50;
        public float chargingRadius = 5;

        void Update()
        {
            if (Vector3.Distance(transform.position, Player.Instance.transform.position) < chargingRadius)
            {
                if (Player.Instance.disguiseBattery < Player.Instance.disguiseSlider.maxValue)
                {
                    Player.Instance.disguiseBattery += charge * Time.deltaTime;
                }
                else
                {
                    Player.Instance.disguiseBattery = Player.Instance.disguiseSlider.maxValue;
                }
            }
        }
    }
}