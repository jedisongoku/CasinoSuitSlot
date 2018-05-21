using UnityEngine;
using System.Collections.Generic;

namespace Mkey
{
    public class PayTableController : PopUpsController
    {
        public GameObject[] tabs;
        private int currTabIndex = 0;
        void Start()
        {
            SetActiveTab(currTabIndex);
        }

        public void Cancel_Click()
        {
            if (SoundMasterController.Instance) SoundMasterController.Instance.SoundPlayClick(0.0f, null);
            CloseButton_click();
        }

        public override void RefreshWindow()
        {
            base.RefreshWindow();
        }

        private void GetChilds(GameObject g, ref List<GameObject> gList)
        {
            int childs = g.transform.childCount;
            if (childs > 0)//The condition that limites the method for calling itself
                for (int i = 0; i < childs; i++)
                {
                    Transform gT = g.transform.GetChild(i);
                    GameObject gC = gT.gameObject;
                    if (gC) gList.Add(gC);
                    GetChilds(gT.gameObject, ref gList);
                }
        }

        public void NextTab_Click()
        {
            currTabIndex =(int) Mathf.Repeat(++currTabIndex, tabs.Length);
            SetActiveTab(currTabIndex);
        }

        public void PrevTab_Click()
        {
            currTabIndex = (int)Mathf.Repeat(--currTabIndex, tabs.Length);
            SetActiveTab(currTabIndex);
        }

        private void SetActiveTab(int index)
        {
            if (tabs == null || tabs.Length == 0) return;
            if (index < 0) index = 0;
            if (index >= tabs.Length) index = tabs.Length - 1;
            for (int i = 0; i <tabs.Length; i++)
            {
              if(tabs[i]) tabs[i].SetActive(i==index);
            }
        }
    }
}