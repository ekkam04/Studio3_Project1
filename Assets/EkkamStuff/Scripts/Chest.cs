using System.Collections;
using UnityEngine;

namespace Ekkam
{
    public class Chest : Signalable
    {
        [SerializeField] private GameObject slidingPartL;
        [SerializeField] private GameObject slidingPartR;
        [SerializeField] private Transform tokenSpawnPoint;
        
        public override void Signal()
        {
            StartCoroutine(OpenChest());
        }
        
        IEnumerator OpenChest()
        {
            float t = 0;
            Vector3 startPosL = slidingPartL.transform.localPosition;
            Vector3 startPosR = slidingPartR.transform.localPosition;
            Vector3 endPosL = startPosL + new Vector3(0.5f, 0, 0);
            Vector3 endPosR = startPosR + new Vector3(-0.5f, 0, 0);
            while (t < 1)
            {
                slidingPartL.transform.localPosition = Vector3.Lerp(startPosL, endPosL, t);
                slidingPartR.transform.localPosition = Vector3.Lerp(startPosR, endPosR, t);
                t += Time.deltaTime;
                yield return null;
            }
            // yield return new WaitForSeconds(0.25f);
            GameObject token = Instantiate(GameManager.Instance.tokenPrefab, tokenSpawnPoint.position, Quaternion.identity);
        }
    }
}