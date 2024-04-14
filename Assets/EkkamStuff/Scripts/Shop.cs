using System.Collections.Generic;
using System.Threading.Tasks;
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
                shopItemScript.itemPrice = item.itemPrice;
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
                shopItemScript.isUpgrade = item.isUpgrade;
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
            
            if (!selectedShopItem.isUpgrade && Player.Instance.coins < selectedShopItem.itemPrice)
            {
                print("Not enough coins");
                var dialog = new Dialog
                {
                    dialogText = "You don't have enough coins to buy this item.",
                    dialogOptions = new DialogOption[]
                    {
                        new DialogOption
                        {
                            optionText = "OK",
                            optionType = DialogOption.OptionType.End
                        }
                    }
                };
                GameManager.Instance.dialogManager.dialogs = new List<Dialog> { dialog };
                ShowDialogAfterDelay(200);
                return;
            }
            else if (selectedShopItem.isUpgrade && Player.Instance.tokens < selectedShopItem.itemPrice)
            {
                print("Not enough tokens");
                var dialog = new Dialog
                {
                    dialogText = "You don't have enough tokens to buy this item.",
                    dialogOptions = new DialogOption[]
                    {
                        new DialogOption
                        {
                            optionText = "OK",
                            optionType = DialogOption.OptionType.End
                        }
                    }
                };
                GameManager.Instance.dialogManager.dialogs = new List<Dialog> { dialog };
                ShowDialogAfterDelay(200);
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
            
            if (selectedShopItem.isUpgrade)
            {
                Player.Instance.tokens -= selectedShopItem.itemPrice;
            }
            else
            {
                Player.Instance.coins -= selectedShopItem.itemPrice;
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
        
        async void ShowDialogAfterDelay(int delay)
        {
            await Task.Delay(delay);
            GameManager.Instance.dialogManager.StartDialog(0);
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