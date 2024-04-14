using System.Threading.Tasks;
using QFSW.QC;
using UnityEngine;

namespace Ekkam
{
    public class GuideBot : MonoBehaviour
    {
        public enum GuideBotState
        {
            Following,
            Talking,
            Flashlight
        }
        public GuideBotState guideBotState;
        
        public GameObject mainRig;
        public GameObject mousePosition3D;
        
        [SerializeField] Vector3 followingOffset;
        [SerializeField] Vector3 talkingOffset;
        [SerializeField] Vector3 flashlightOffset;
        
        private Player player;
        public Animator anim;
        
        private float speed = 1.5f;
        private float rotationSpeed = 5f;
        
        void Start()
        {
            player = Player.Instance;
            anim = GetComponent<Animator>();
            // offset = transform.position - player.transform.position;
        }
        
        void Update()
        {
            switch (guideBotState)
            {
                case GuideBotState.Following:
                    Follow(followingOffset);
                    LookAt(player.transform.position);
                    break;
                case GuideBotState.Talking:
                    Follow(talkingOffset);
                    LookAt(player.transform.position);
                    break;
                case GuideBotState.Flashlight:
                    Follow(flashlightOffset);
                    LookAt(mousePosition3D.transform.position);
                    break;
            }
        }
        
        private void Follow(Vector3 offset)
        {
            transform.position = Vector3.Slerp(
                transform.position,
                player.transform.position + player.transform.forward * offset.z + player.transform.right * offset.x + player.transform.up * offset.y, 
                speed * Time.deltaTime
            );
        }
        
        private void LookAt(Vector3 target)
        {
            Vector3 directionToTarget = target - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            Quaternion targetRotationLocal = Quaternion.Inverse(transform.parent.rotation) * targetRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationLocal, Time.deltaTime * rotationSpeed);
        }
        
        public void SwitchToFollowing()
        {
            guideBotState = GuideBotState.Following;
        }
        
        public void SwitchToTalking()
        {
            guideBotState = GuideBotState.Talking;
        }
        
        public void SwitchToFlashlight()
        {
            guideBotState = GuideBotState.Flashlight;
        }
    }
}