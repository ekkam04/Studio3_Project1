using System.Threading.Tasks;
using QFSW.QC;
using UnityEngine;

namespace Ekkam
{
    public class GuideBot : MonoBehaviour
    {
        public GameObject mainRig;
        [SerializeField] Vector3 offset;
        private Player player;
        public Animator anim;
        private float speed = 1.5f;
        private float rotationSpeed = 5f;
        
        void Start()
        {
            player = Player.Instance;
            anim = GetComponent<Animator>();
            // offset = transform.position - player.transform.position;
            print("GuideBot offset: " + offset);
        }
        
        void Update()
        {
            Vector3 directionToTarget = (player.transform.position + new Vector3(0, 0.5f, 0)) - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            Quaternion targetRotationLocal = Quaternion.Inverse(transform.parent.rotation) * targetRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationLocal, Time.deltaTime * rotationSpeed);
        }
        
        void FixedUpdate()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.Slerp(transform.position, player.transform.position + player.transform.forward * offset.z + player.transform.right * offset.x + player.transform.up * offset.y, step);
        }
    }
}