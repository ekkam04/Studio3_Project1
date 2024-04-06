using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.CSharp;
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
        TMP_Text pickUpText;
        
        public enum InteractionType
        {
            InteractKey,
            Damage
        }
        public InteractionType interactionType;
        
        public enum InteractionAction
        {
            Pickup,
            Signal,
            Place,
            Talk,
            Heal,
            DamageCrystal
        }
        public InteractionAction interactionAction;

        [Header("Interactable Settings")]
        public string interactText;
        public int timesInteracted = 0;
        public float extraInteractDistance;
        public bool singleUse;
        
        [Header("Item Settings")]
        public Vector3 rotationOffset;
        public Vector3 positionOffset;
        public bool heldInOffHand;
        
        [Header("Signal Settings")]
        public Signalable signalReceiver;
        
        [Header("Place Settings")]
        public Vector3 placeRotationOffset;
        public Vector3 placePositionOffset;
        public string tagToAccept;

        void Start()
        {
            player = FindObjectOfType<Player>();
            inventory = FindObjectOfType<Inventory>();
            uiManager = FindObjectOfType<UIManager>();
            var mainCamera = Camera.main;
            
            // pickUpPrompt = Instantiate(uiManager.pickUpPrompt, transform.position, Quaternion.identity);
            // pickUpPrompt.GetComponentInChildren<TextMeshProUGUI>().text = interactText;
            // pickUpPrompt.GetComponentInChildren<RotationConstraint>().AddSource(new ConstraintSource
            // {
            //     sourceTransform = mainCamera.transform,
            //     weight = 1
            // });
            // pickUpPrompt.SetActive(false);
            pickUpPrompt = uiManager.pickUpPrompt;
            pickUpText = pickUpPrompt.GetComponentInChildren<TMP_Text>();
        }

        void Update()
        {
            // check if player has the item in their inventory
            if (interactionType == InteractionType.InteractKey)
            {
                if (GetComponent<Item>() != null && inventory.HasItem(GetComponent<Item>()) == true)
                {
                    return;
                }
                CheckForInteract();
            }
        }

        void CheckForInteract()
        {
            if (Vector3.Distance(player.transform.position, transform.position) > player.interactDistance + extraInteractDistance + 0.1f)
            {
                return;
            }

            if (Vector3.Distance(player.transform.position, transform.position) < player.interactDistance + extraInteractDistance)
            {
                pickUpText.text = interactText;
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

        async public void Interact()
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
                    transform.localPosition += positionOffset;
                    transform.localRotation = Quaternion.identity;
                    transform.Rotate(rotationOffset);
                }
            }
            else if (interactionAction == InteractionAction.Signal)
            {
                print("Signaling " + signalReceiver.name);
                signalReceiver.Signal();
                StartCoroutine(PulsePickupPromptText(Color.yellow, 0.1f, 0.3f));
            }
            else if (interactionAction == InteractionAction.Place)
            {
                if (inventory.GetSelectedItem() != null && inventory.GetSelectedItem().tag == tagToAccept)
                {
                    print("Placing " + inventory.GetSelectedItem().name + " on " + gameObject.name);
                    GameObject objectToPlace = inventory.GetSelectedItem().gameObject;
                    objectToPlace.transform.SetParent(transform);
                    if (objectToPlace.GetComponent<Interactable>() != null) objectToPlace.GetComponent<Interactable>().enabled = false;

                    objectToPlace.transform.localPosition = Vector3.zero;
                    objectToPlace.transform.localPosition += placePositionOffset;
                    objectToPlace.transform.localRotation = Quaternion.identity;
                    objectToPlace.transform.Rotate(placeRotationOffset);
                    
                    await Task.Delay(100);
                    inventory.RemoveItem(inventory.GetSelectedItem());
                    if (signalReceiver != null) signalReceiver.Signal();
                    
                    pickUpPrompt.SetActive(false);
                    this.enabled = false;
                }
                else
                {
                    StartCoroutine(PulsePickupPromptText(Color.red, 0.1f, 0.3f));
                }
            }
            else if (interactionAction == InteractionAction.Talk)
            {
                DialogManager dialogManager = GetComponent<DialogManager>();
                dialogManager.StartDialog(0);
                pickUpPrompt.SetActive(false);
                this.enabled = false;
            }
            else if (interactionAction == InteractionAction.Heal)
            {
                player.Heal(50);
                StartCoroutine(PulsePickupPromptText(Color.green, 0.1f, 0.3f));
                if (singleUse)
                {
                    pickUpPrompt.SetActive(false);
                    this.enabled = false;
                }
            }
            else if (interactionAction == InteractionAction.DamageCrystal)
            {
                var crystals = GetComponentsInChildren<Crystal>();
                foreach (var crystal in crystals)
                {
                    crystal.DamageTile();
                }
            }
        }
        
        IEnumerator PulsePickupPromptText(Color color, float fadeInDuration, float fadeOutDuration)
        {
            for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
            {
                pickUpPrompt.GetComponentInChildren<TextMeshProUGUI>().color = Color.Lerp(Color.white, color, t / fadeInDuration);
                yield return null;
            }
            for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
            {
                pickUpPrompt.GetComponentInChildren<TextMeshProUGUI>().color = Color.Lerp(color, Color.white, t / fadeOutDuration);
                yield return null;
            }
        }
    }
}
