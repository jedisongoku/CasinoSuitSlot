using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mkey
{
    public class ShopThingHelper : MonoBehaviour
    {
        public Image thingImage;
        public Image thingLabelImage;
        public Text thingTextCount;
        public Text thingTextCountOld;
        public Text thingTextPrice;
        public Button thingBuyButton;

        public void SetData(ShopThingData shopThing)
        {
            thingImage.sprite = shopThing.thingImage;
            thingImage.SetNativeSize();
            thingTextCount.text = (shopThing.thingCount>0) ?  shopThing.thingCount.ToString() : "";
            thingTextCountOld.text = (shopThing.thingCountOld>0) ? shopThing.thingCountOld.ToString() : "";
            thingTextPrice.text =shopThing.currency + shopThing.thingPrice.ToString();

            if (shopThing.thingLabelImage)
            {
                thingLabelImage.sprite = shopThing.thingLabelImage;
                thingLabelImage.SetNativeSize();
            }
            else
            {
                thingLabelImage.gameObject.SetActive(false);
            }
            thingBuyButton.onClick.RemoveAllListeners();
            thingBuyButton.onClick = shopThing.clickEvent;
        }
    }

    [System.Serializable]
    public class ShopThingData
    {
        public string name;
        public Sprite thingImage;
        public Sprite thingLabelImage;
        public int thingCount;
        public int thingCountOld;
        public float thingPrice;
        public string currency;
        public string kProductID;
        public bool showInShop = true;

        [Space(8, order = 0)]
        [Header("Purchase Event: ", order = 1)]
        public UnityEvent PurchaseEvent;

        [HideInInspector]
        public Button.ButtonClickedEvent clickEvent;

        public ShopThingData(ShopThingData prod)
        {
            name = prod.name;
            thingImage = prod.thingImage;
            thingLabelImage = prod.thingLabelImage;
            thingCount = prod.thingCount;
            thingCountOld = prod.thingCountOld;
            thingPrice = prod.thingPrice;
            currency = prod.currency;
            kProductID = prod.kProductID;
            showInShop = prod.showInShop;
            PurchaseEvent = prod.PurchaseEvent;
            clickEvent = prod.clickEvent;
        }
    }
}