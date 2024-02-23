using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class Interactable : MonoBehaviour
    {
        Player player;
        Inventory inventory;
        public string interactText;
        public int timesInteracted = 0;

        void Start()
        {
            player = FindObjectOfType<Player>();
            inventory = FindObjectOfType<Inventory>();
        }

        void Update()
        {
            // check if player has the item in their inventory
            if (inventory.HasItem(GetComponent<Item>()) == false)
            {
                CheckForInteract();
            }
        }

        void CheckForInteract()
        {
            if (Vector3.Distance(player.transform.position, transform.position) < player.interactDistance)
            {
                print("Press E to interact with " + gameObject.name);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Interact();
                }
            }
        }

        public void Interact()
        {
            print("Interacting with " + gameObject.name);
            timesInteracted++;
            if (GetComponent<Item>())
            {
                inventory.AddItem(GetComponent<Item>());
                transform.SetParent(player.itemHolderRight.transform);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
    }
}
