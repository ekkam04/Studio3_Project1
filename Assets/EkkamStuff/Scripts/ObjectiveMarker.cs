using UnityEngine;
using TMPro;
using UnityEngine.Animations;

namespace Ekkam
{
    public class ObjectiveMarker : MonoBehaviour
    {
        public TMP_Text distanceText;
        private Player player;
        
        void Start()
        {
            player = Player.Instance;
            Camera mainCamera = Camera.main;
            RotationConstraint rotationConstraint = gameObject.GetComponent<RotationConstraint>();
            ConstraintSource constraintSource = new ConstraintSource
            {
                sourceTransform = mainCamera.transform,
                weight = 1
            };
            rotationConstraint.AddSource(constraintSource);
        }
        
        void Update()
        {
            if (player == null) return;
            
            var distance = Vector3.Distance(player.transform.position, transform.position);
            distanceText.text = $"{distance:0.0}m";
        }
    }
}