using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam {
    public class Inventory : MonoBehaviour
    {
        
        [SerializeField] private Texture2D[] slotTexture; // 0 = unselected, 1 = selected
        [SerializeField] private GameObject SlotsHolder;
        
        [SerializeField] private int slotCount = 5;
        // [SerializeField] private float slotPositionXOffset = 150f;
        // [SerializeField] private float slotPositionYOffset = 50f;
        [SerializeField] private float slotSize = 50f;
        [SerializeField] private float slotTransparency = 0.5f;

        public List<GameObject> slots = new List<GameObject>();
        public List<Item> items = new List<Item>();
        public GameObject selectedSlot;

        // private float slotPositionX = 0f;
        Player player;
        // UIManager uiManager;

        void Start()
        {
            player = FindObjectOfType<Player>();
            // uiManager = GameObject.FindObjectOfType<UIManager>();
            // Create slots as raw image with texture 0
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slot = new GameObject("Slot " + i);
                slot.transform.SetParent(SlotsHolder.transform);
                slot.AddComponent<RawImage>();
                slot.GetComponent<RawImage>().texture = slotTexture[0];

                Color tempColor = slot.GetComponent<RawImage>().color;
                tempColor.a = slotTransparency;
                slot.GetComponent<RawImage>().color = tempColor;
                
                float scaleFactor = slot.GetComponentInParent<Canvas>().scaleFactor;
                slot.GetComponent<RectTransform>().sizeDelta = new Vector2(slotSize * scaleFactor, slotSize * scaleFactor);

                // slot.GetComponent<RectTransform>().localPosition = new Vector3(slotPositionX, slotPositionYOffset, 0);
                // slotPositionX += slotPositionXOffset;
                slots.Add(slot);
            }

            // Center slots and take slot count into account
            // SlotsHolder.GetComponent<RectTransform>().localPosition = new Vector3(-slotPositionX / 2 + slotPositionXOffset / 2, 0, 0);

            // Set first slot to selected
            slots[0].GetComponent<RawImage>().texture = slotTexture[1];
            selectedSlot = slots[0];
        }

        public void CycleSlot(bool forward)
        {
            // Set all slots to unselected
            foreach (GameObject slot in slots)
            {
                slot.GetComponent<RawImage>().texture = slotTexture[0];
            }

            // Cycle slot
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
            selectedSlot = slots[index];
            selectedSlot.GetComponent<RawImage>().texture = slotTexture[1];
            ShowEquippedItem();
        }

        public void AddItem(Item item)
        {
            // Add item to first empty slot
            foreach (GameObject slot in slots)
            {
                if (slot.transform.childCount == 0)
                {
                    // create new raw image with item sprite inside slot as child
                    GameObject itemObj = new GameObject("Item");
                    itemObj.transform.SetParent(slot.transform);
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

        // public void UseItem()
        // {
        //     // Use item in selected slot
        //     if (selectedSlot.transform.childCount > 0)
        //     {
        //         Item item = items[slots.IndexOf(selectedSlot)];
        //         switch (item.tag)
        //         {
        //             case "Sword":
        //                 player.SwingSword();
        //                 break;
        //             case "Bow":
        //                 player.ShootArrow();
        //                 break;
        //             case "Staff":
        //                 player.ShootSpellBall();
        //                 break;
        //             default:
        //                 break;
        //         }
        //     }
        // }
        
        public Item GetSelectedItem()
        {
            if (selectedSlot.transform.childCount > 0)
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

            // uiManager.HideLighterUI();

            // show item in selected slot
            if (selectedSlot.transform.childCount > 0)
            {
                Item item = items[slots.IndexOf(selectedSlot)];
                item.gameObject.SetActive(true);
                // uiManager.ShowUsePrompt(item.useText);
                // if (item.tag == "Lighter")
                // {
                //     uiManager.ShowLighterUI();
                // }
                if (item.tag == "Sword" || item.tag == "Staff")
                {
                    player.anim.SetBool("isHoldingSword", true);
                }
                else
                {
                    player.anim.SetBool("isHoldingSword", false);
                }
            }
            else
            {
                player.anim.SetBool("isHoldingSword", false);
            }
        }

        public bool HasItem(Item item)
        {
            return items.Contains(item);
        }
    }
}
