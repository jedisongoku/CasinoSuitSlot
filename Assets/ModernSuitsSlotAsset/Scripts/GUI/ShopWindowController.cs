using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class ShopWindowController : PopUpsController
    {
        public GameObject shopThingPrefab;
        public RectTransform ThingsParent;
        private List<ShopThingHelper> shopThings;

        void Start()
        {
            CreateThingTab();
        }

        public override void RefreshWindow()
        {
            base.RefreshWindow();
        }

        public void Cancel_Click()
        {
            if (SoundMasterController.Instance) SoundMasterController.Instance.SoundPlayClick(0.0f, null);
            CloseButton_click();
        }

        private void CreateThingTab()
        {
            ShopThingHelper[] sT = ThingsParent.GetComponentsInChildren<ShopThingHelper>();
            foreach (var item in sT)
            {
                DestroyImmediate(item.gameObject);
            }

            Purchaser p = Purchaser.Instance;
            if (p == null) return;

            List<ShopThingData> products = new List<ShopThingData>();
            if (p.consumable != null && p.consumable.Length > 0) products.AddRange(p.consumable);
            if (p.nonConsumable != null && p.nonConsumable.Length > 0) products.AddRange(p.nonConsumable);
            if (p.subscriptions != null && p.subscriptions.Length > 0) products.AddRange(p.subscriptions);

            if (products.Count==0) return;

            shopThings = new List<ShopThingHelper>();
            for (int i = 0; i < products.Count; i++)
            {
              if(products[i]!=null)  shopThings.Add(CreateThing(shopThingPrefab, ThingsParent, products[i]));
            }
        }

        private ShopThingHelper CreateThing(GameObject prefab, RectTransform parent, ShopThingData shopThingData)
        {
            GameObject shopThing = Instantiate(shopThingPrefab);
            shopThing.transform.localScale = ThingsParent.transform.lossyScale;
            shopThing.transform.SetParent(ThingsParent.transform);
            ShopThingHelper sC = shopThing.GetComponent<ShopThingHelper>();
            sC.SetData(shopThingData);
            return sC;
        }

    }
}