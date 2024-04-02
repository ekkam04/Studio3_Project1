using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam {
    public class Inventory : MonoBehaviour
    {
        
        // [SerializeField] private Texture2D[] slotTexture; // 0 = unselected, 1 = selected
        // [SerializeField] private GameObject SlotsHolder;
        
        // [SerializeField] private int slotCount = 5;
        // [SerializeField] private float slotPositionXOffset = 150f;
        // [SerializeField] private float slotPositionYOffset = 50f;
        // [SerializeField] private float slotSize = 50f;
        // [SerializeField] private float slotTransparency = 0.5f;

        public List<GameObject> slots = new List<GameObject>();
        public List<Item> items = new List<Item>();
        public GameObject selectedSlot;

        // private float slotPositionX = 0f;
        Player player;
        // UIManager uiManager;

        void Start()
        {
            player = FindObjectOfType<Player>();
            
            foreach (Transform child in transform)
            {
                GameObject slot = child.gameObject;
                slots.Add(slot);
            }
            
            selectedSlot = slots[0];
            selectedSlot.GetComponentInChildren<Animator>().SetTrigger("Pressed");
            ShowEquippedItem();
        }
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetSelectedSlot(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetSelectedSlot(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetSelectedSlot(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetSelectedSlot(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SetSelectedSlot(4);
            }
        }

        public void CycleSlot(bool forward)
        {
            int index = slots.IndexOf(selectedSlot);
            if (forward)
            {
                index++;
                if (index >= slots.Count)
                {
                    index = 0;
                }
            }
            else
            {
                index--;
                if (index < 0)
                {
                    index = slots.Count - 1;
                }
            }
            SetSelectedSlot(index);
        }
        
        void SetSelectedSlot(int index)
        {
            foreach (GameObject slot in slots)
            {
                slot.GetComponentInChildren<Animator>().SetTrigger("Normal");
            }
            selectedSlot = slots[index];
            selectedSlot.GetComponentInChildren<Animator>().SetTrigger("Pressed");
            ShowEquippedItem();
        }

        public void AddItem(Item item)
        {
            // Add item to first empty slot
            foreach (GameObject slot in slots)
            {
                if (slot.GetComponentInChildren<RawImage>() == null)
                {
                    GameObject itemObj = new GameObject("Item");
                    itemObj.transform.SetParent(slot.transform.GetChild(0).transform);
                    itemObj.AddComponent<RawImage>();
                    itemObj.GetComponent<RawImage>().texture = item.itemTexture;

                    float scaleFactor = itemObj.GetComponentInParent<Canvas>().scaleFactor;
                    itemObj.GetComponent<RectTransform>().sizeDelta = new Vector2(item.itemSize * scaleFactor, item.itemSize * scaleFactor);

                    itemObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    items.Add(item);
                    break;
                }
            }
            ShowEquippedItem();
        }
        
        public void RemoveItem(Item item)
        {
            // Remove item from slot
            if (items.Contains(item))
            {
                int index = items.IndexOf(item);
                items.Remove(item);
                Destroy(slots[index].GetComponentInChildren<RawImage>().gameObject);
            }
            Invoke("ShowEquippedItem", 0.1f);
        }
        
        public Item GetSelectedItem()
        {
            if (selectedSlot.GetComponentInChildren<RawImage>() != null)
            {
                return items[slots.IndexOf(selectedSlot)];
            }
            return null;
        }

        public void ShowEquippedItem()
        {
            // hide all items
            foreach (Item item in items)
            {
                item.gameObject.SetActive(false);
            }
            
            // hide all item animations
            player.anim.SetBool("isHoldingSword", false);
            player.anim.SetBool("isHoldingBow", false);
            player.anim.SetBool("isHolding", false);
            player.bowRig.weight = 0;
            player.SwitchCameraStyle(Player.CameraStyle.Exploration);

            // show item in selected slot
            if (selectedSlot.GetComponentInChildren<RawImage>() != null)
            {
                Item item = items[slots.IndexOf(selectedSlot)];
                item.gameObject.SetActive(true);
                if (item.tag == "Sword" || item.tag == "Staff" || item.tag == "FireExtinguisher")
                {
                    player.anim.SetBool("isHoldingSword", true);
                    player.SwitchCameraStyle(Player.CameraStyle.Combat);
                }
                else if (item.tag == "Bow")
                {
                    player.anim.SetBool("isHoldingBow", true);
                    player.bowRig.weight = 1;
                    player.SwitchCameraStyle(Player.CameraStyle.Combat);
                }
                else
                {
                    player.anim.SetBool("isHoldingSword", false);
                }
                
                if (item.tag == "Battery")
                {
                    player.anim.SetBool("isHolding", true);
                }
                else
                {
                    player.anim.SetBool("isHolding", false);
                }
            }
        }

        public bool HasItem(Item item)
        {
            return items.Contains(item);
        }
    }
}
