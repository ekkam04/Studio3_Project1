using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Animations;

namespace Ekkam {
    public class Interactable : MonoBehaviour
    {
        Player player;
        Inventory inventory;
        UIManager uiManager;
        GameObject pickUpPrompt;
        
        public enum InteractionType
        {
            InteractKey,
            Damage
        }
        public InteractionType interactionType;
        
        public enum InteractionAction
        {
            Pickup,
            Signal
        }
        public InteractionAction interactionAction;

        [Header("Interactable Settings")]
        public string interactText;
        public int timesInteracted = 0;
        
        [Header("Item Settings")]
        public Vector3 rotationOffset;
        public bool heldInOffHand;
        
        [Header("Signal Settings")]
        public Signalable signalReceiver;

        void Start()
        {
            player = FindObjectOfType<Player>();
            inventory = FindObjectOfType<Inventory>();
            uiManager = FindObjectOfType<UIManager>();
            var mainCamera = Camera.main;
            
            pickUpPrompt = Instantiate(uiManager.pickUpPrompt, transform.position, Quaternion.identity, transform);
            pickUpPrompt.GetComponentInChildren<TextMeshProUGUI>().text = interactText;
            pickUpPrompt.GetComponentInChildren<RotationConstraint>().AddSource(new ConstraintSource
            {
                sourceTransform = mainCamera.transform,
                weight = 1
            });
            pickUpPrompt.SetActive(false);
        }

        void Update()
        {
            // check if player has the item in their inventory
            if (interactionType == InteractionType.InteractKey && inventory.HasItem(GetComponent<Item>()) == false)
            {
                CheckForInteract();
            }
        }

        void CheckForInteract()
        {
            if (Vector3.Distance(player.transform.position, transform.position) < player.interactDistance)
            {
                pickUpPrompt.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Interact();
                }
            }
            else
            {
                pickUpPrompt.SetActive(false);
            }
        }

        public void Interact()
        {
            print("Interacting with " + gameObject.name);
            timesInteracted++;
            if (interactionAction == InteractionAction.Pickup)
            {
                if (GetComponent<Item>())
                {
                    pickUpPrompt.SetActive(false);
                    inventory.AddItem(GetComponent<Item>());
                    if (heldInOffHand)
                    {
                        transform.SetParent(player.itemHolderLeft.transform);
                    }
                    else
                    {
                        transform.SetParent(player.itemHolderRight.transform);
                    }
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    transform.Rotate(rotationOffset);
                }
            }
            else if (interactionAction == InteractionAction.Signal)
            {
                print("Signaling " + signalReceiver.name);
                signalReceiver.Signal();
            }
        }
    }
}
