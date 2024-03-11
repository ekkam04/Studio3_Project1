using System.Threading.Tasks;
using QFSW.QC;
using UnityEngine;

namespace Ekkam
{
    public class GuideBot : MonoBehaviour
    {
        [SerializeField] Vector3 offset;
        private Player player;
        public Animator anim;
        
        void Start()
        {
            player = Player.Instance;
            anim = GetComponent<Animator>();
            // offset = transform.position - player.transform.position;
            print("GuideBot offset: " + offset);
        }
        
        void Update()
        {
            transform.position = player.transform.position + offset;
        }
    }
}