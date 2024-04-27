using System;
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
        Collider collider;
        
        public static Color interactColor;
        
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
            Shop,
            DamageCrystal,
            Drive
        }
        public InteractionAction interactionAction;

        [Header("Interactable Settings")]
        public string interactText;
        public int timesInteracted = 0;
        public float extraInteractDistance;
        public bool singleUse;
        public string interactionActionKey;
        
        [Header("Item Settings")]
        public Vector3 rotationOffset;
        public Vector3 positionOffset;
        public bool heldInOffHand;
        
        [Header("Signal Settings")]
        public Signalable signalReceiver;
        public List<Signalable> extraSignalReceivers;
        
        [Header("Place Settings")]
        public Vector3 placeRotationOffset;
        public Vector3 placePositionOffset;
        public string tagToAccept;
        
        [Header("Dialog Settings")]
        public DialogManager dialogManager;
        
        [Header("Crystal Settings")]
        public bool isBroken;

        [Header("Drive Settings")]
        public Buggy buggy;
        
        public delegate void OnInteraction(string actionKey);
        public static event OnInteraction onInteraction;

        void Start()
        {
            player = FindObjectOfType<Player>();
            inventory = FindObjectOfType<Inventory>();
            uiManager = FindObjectOfType<UIManager>();
            collider = GetComponent<Collider>();
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
            
            if (interactionAction == InteractionAction.DamageCrystal)
            {
                if (timesInteracted > 2 && collider.enabled == true)
                {
                    collider.enabled = false;
                    if (signalReceiver != null)
                    {
                        signalReceiver.Signal();
                        foreach (var extraSignalReceiver in extraSignalReceivers)
                        {
                            extraSignalReceiver.Signal();
                        }
                    }
                    isBroken = true;
                }
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
            SoundManager.Instance.PlaySound("interact");
            if (interactionAction == InteractionAction.Pickup)
            {
                if (GetComponent<Item>())
                {
                    PickUp();
                }
            }
            else if (interactionAction == InteractionAction.Signal)
            {
                interactColor = Color.yellow;
                if (signalReceiver != null)
                {
                    print("Signaling " + signalReceiver.name);
                    signalReceiver.Signal();
                }
                foreach (var extraSignalReceiver in extraSignalReceivers)
                {
                    extraSignalReceiver.Signal();
                }
                StartCoroutine(PulsePickupPromptText(0.1f, 0.3f));
            }
            else if (interactionAction == InteractionAction.Place)
            {
                if (inventory.GetSelectedItem() != null && inventory.GetSelectedItem().tag == tagToAccept)
                {
                    print("Placing " + inventory.GetSelectedItem().name + " on " + gameObject.name);
                    SoundManager.Instance.PlaySound("battery-place");
                    GameObject objectToPlace = inventory.GetSelectedItem().gameObject;
                    objectToPlace.transform.SetParent(transform);
                    if (objectToPlace.GetComponent<Interactable>() != null) objectToPlace.GetComponent<Interactable>().enabled = false;

                    objectToPlace.transform.localPosition = Vector3.zero;
                    objectToPlace.transform.localPosition += placePositionOffset;
                    objectToPlace.transform.localRotation = Quaternion.identity;
                    objectToPlace.transform.Rotate(placeRotationOffset);
                    
                    await Task.Delay(100);
                    inventory.RemoveItem(inventory.GetSelectedItem(), false);
                    if (signalReceiver != null)
                    {
                        signalReceiver.Signal();
                        foreach (var extraSignalReceiver in extraSignalReceivers)
                        {
                            extraSignalReceiver.Signal();
                        }
                    }
                    
                    pickUpPrompt.SetActive(false);
                    this.enabled = false;
                }
                else
                {
                    interactColor = Color.red;
                    SoundManager.Instance.PlaySound("interact-failed");
                    StartCoroutine(PulsePickupPromptText( 0.1f, 0.3f));
                }
            }
            else if (interactionAction == InteractionAction.Talk)
            {
                if (signalReceiver != null)
                {
                    print("Signaling " + signalReceiver.name);
                    signalReceiver.Signal();
                }
                foreach (var extraSignalReceiver in extraSignalReceivers)
                {
                    extraSignalReceiver.Signal();
                }
                if (dialogManager == null)
                {
                    dialogManager = GetComponent<DialogManager>();
                }
                dialogManager.StartDialog(0);
                pickUpPrompt.SetActive(false);
                this.enabled = false;
            }
            else if (interactionAction == InteractionAction.Shop)
            {
                if (signalReceiver != null)
                {
                    print("Signaling " + signalReceiver.name);
                    signalReceiver.Signal();
                }
                foreach (var extraSignalReceiver in extraSignalReceivers)
                {
                    extraSignalReceiver.Signal();
                }
                uiManager.OpenShopUI();
                StartCoroutine(PulsePickupPromptText(0.1f, 0.3f));
            }
            else if (interactionAction == InteractionAction.DamageCrystal)
            {
                var crystals = GetComponentsInChildren<Crystal>();
                foreach (var crystal in crystals)
                {
                    crystal.DamageTile();
                }
            }
            else if (interactionAction == InteractionAction.Drive)
            {
                buggy.EnterVehicle();
            }

            if (onInteraction != null && interactionActionKey != "")
            {
                onInteraction.Invoke(interactionActionKey);
            }
            
            if (singleUse)
            {
                pickUpPrompt.SetActive(false);
                this.gameObject.SetActive(false);
            }
        }
        
        IEnumerator PulsePickupPromptText(float fadeInDuration, float fadeOutDuration)
        {
            for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
            {
                pickUpPrompt.GetComponentInChildren<TextMeshProUGUI>().color = Color.Lerp(Color.white, interactColor, t / fadeInDuration);
                yield return null;
            }
            for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
            {
                pickUpPrompt.GetComponentInChildren<TextMeshProUGUI>().color = Color.Lerp(interactColor, Color.white, t / fadeOutDuration);
                yield return null;
            }
        }

        public void PickUp()
        {
            if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
            if (inventory == null) inventory = FindObjectOfType<Inventory>();
            if (player == null) player = FindObjectOfType<Player>();
            
            uiManager.pickUpPrompt.SetActive(false);
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

        private void OnDisable()
        {
            Invoke("ResetTimesInteracted", 1f);
        }
        
        void ResetTimesInteracted()
        {
            timesInteracted = 0;
        }
    }
}
