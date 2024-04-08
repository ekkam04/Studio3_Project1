using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Ekkam
{
    [System.Serializable]
    public class ShopItem : MonoBehaviour
    {
        public Button selectButton;
        public TMP_Text itemNameText;
        public TMP_Text itemPriceText;
        public TMP_Text itemStockText;
        public string itemDescription;
        public string itemKey;
        public RawImage itemIcon;
        public RawImage currencyIcon;
        
        private Shop shop;
        
        void Start()
        {
            shop = FindObjectOfType<Shop>();
        }
        
        public void SelectItem()
        {
            shop.SetSelectedShopItem(this);
        }
        
        public void BuyItem()
        {
            shop.ShowConfirmationDialog(this);
        }
    }
}