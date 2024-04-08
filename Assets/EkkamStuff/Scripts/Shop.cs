using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Ekkam
{
    public class Shop : Signalable
    {
        public GameObject itemsHolder;
        public GameObject shopItemPrefab;
        public Sprite[] currencyIcons; // 0 = coins, 1 = tokens
        public List<ShopItem> shopItemPrefabs;
        public List<ShopItemStats> shopItems;
        
        public ShopItem selectedShopItem;
        public TMP_Text itemNameText;
        public TMP_Text itemDescriptionText;
        public TMP_Text itemPriceText;
        public RawImage itemIcon;
        public RawImage currencyIcon;
        
        public List<Dialog> buyDialogs;
        
        UIManager uiManager;
        
        void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            foreach (var item in shopItems)
            {
                var shopItem = Instantiate(shopItemPrefab, itemsHolder.transform);
                var shopItemScript = shopItem.GetComponent<ShopItem>();
                shopItemScript.itemKey = item.itemKey;
                shopItemScript.itemNameText.text = item.itemName;
                shopItemScript.itemDescription = item.itemDescription;
                if (item.itemPrice == 0)
                {
                    shopItemScript.itemPriceText.text = "FREE";
                }
                else
                {
                    shopItemScript.itemPriceText.text = item.itemPrice.ToString();
                }
                shopItemScript.itemStockText.text = item.itemStock.ToString();
                shopItemScript.itemIcon.texture = item.itemIcon.texture;
                if (item.isUpgrade)
                {
                    shopItemScript.currencyIcon.texture = currencyIcons[1].texture;
                }
                else
                {
                    shopItemScript.currencyIcon.texture = currencyIcons[0].texture;
                }
                shopItemPrefabs.Add(shopItemScript);
            }
            SetSelectedShopItem(shopItemPrefabs[0]);
        }
        
        public void SetSelectedShopItem(ShopItem item)
        {
            selectedShopItem = item;
            itemNameText.text = item.itemNameText.text;
            itemDescriptionText.text = item.itemDescription;
            itemPriceText.text = item.itemPriceText.text;
            itemIcon.texture = item.itemIcon.texture;
            currencyIcon.texture = item.currencyIcon.texture;
        }
        
        public void ShowConfirmationDialog(ShopItem item)
        {
            selectedShopItem = item;
            GameManager.Instance.dialogManager.dialogs = buyDialogs;
            GameManager.Instance.dialogManager.StartDialog(0);
        }

        public override void Signal()
        {
            if (selectedShopItem == null)
            {
                return;
            }
            
            int index = shopItems.FindIndex(x => x.itemKey == selectedShopItem.itemKey);
            shopItems[index].itemStock--;
            selectedShopItem.itemStockText.text = shopItems[index].itemStock.ToString();
            shopItemPrefabs[index].itemStockText.text = shopItems[index].itemStock.ToString();
            if (shopItems[index].itemStock == 0)
            {
                Destroy(shopItemPrefabs[index].gameObject);
                shopItemPrefabs.RemoveAt(index);
            }
            
            switch (selectedShopItem.itemKey)
            {
                case "free-health":
                    print("Bought free health pack");
                    Player.Instance.Heal(50);
                    break;
                case "health":
                    print("Bought health pack");
                    Player.Instance.Heal(50);
                    break;
            }
            
            Invoke("CloseShopUI", 0.5f);
        }

        void CloseShopUI()
        {
            uiManager.CloseShopUI();
        }
    }
    
    [System.Serializable]
    public class ShopItemStats
    {
        public string itemKey;
        public string itemName;
        public string itemDescription;
        public int itemPrice;
        public int itemStock;
        public bool isUpgrade;
        public Sprite itemIcon;
    }
}