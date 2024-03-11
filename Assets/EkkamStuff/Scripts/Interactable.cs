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
            Press,
            Damage
        }
        public InteractionType interactionType;

        public string interactText;
        public int timesInteracted = 0;
        public Vector3 rotationOffset;
        public bool heldInOffHand;

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
            if (interactionType == InteractionType.Press && inventory.HasItem(GetComponent<Item>()) == false)
            {
                CheckForInteract();
            }
            else if (interactionType == InteractionType.Damage)
            {
                
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
                    pickUpPrompt.SetActive(false);
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
            if (GetComponent<Item>())
            {
                inventory.AddItem(GetComponent<Item>());
                // if (tag == "Sword")
                // {
                //     transform.SetParent(player.itemHolderRight.transform);
                //     transform.localPosition = Vector3.zero;
                //     transform.localRotation = Quaternion.identity;
                // }
                // else if (tag == "Bow")
                // {
                //     transform.SetParent(player.itemHolderLeft.transform);
                //     transform.localPosition = Vector3.zero;
                //     transform.localRotation = Quaternion.identity;
                //     var rotationAmount = new Vector3(0, 90, 0);
                //     transform.Rotate(rotationOffset);
                // }
                // else if (tag == "Staff")
                // {
                //     transform.SetParent(player.itemHolderRight.transform);
                //     transform.localPosition = Vector3.zero;
                //     transform.localRotation = Quaternion.identity;
                // }
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
    }
}
