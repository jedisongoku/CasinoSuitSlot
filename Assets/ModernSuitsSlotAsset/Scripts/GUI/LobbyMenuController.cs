using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class LobbyMenuController : MonoBehaviour
    {
        [Space(8, order = 0)]
        [Header("Header menu objects: ", order = 1)]
        public Image levelSlider;
        public Text LevelNumberText;
        public Text BalanceSumText;
		public Text DealTimeText;


        [Space(8, order = 0)]
        [Header("Deal timer settings: ", order = 1)]
        public bool enableTimer;
        public bool createNewTimer;
        public int dealTimerDays = 0;
        public int dealTimerHours = 3;
        public int dealTimerMinutes = 0;
        public int dealTimerSeconds = 0;
        public string dealTimerName= "dealTimer";


		public static LobbyMenuController Instance;
        private Button[] buttons;
        private GlobalTimer gT;
        private SlotPlayer sP
        {
            get { return SlotPlayer.Instance; }
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            buttons = GetComponentsInChildren<Button>();
            gT = new GlobalTimer(dealTimerName, dealTimerDays, dealTimerHours, dealTimerMinutes,dealTimerSeconds, createNewTimer);
            gT.OnTickRestDaysHourMinSec = (d,h,m,s)=> {
              //  Debug.Log("rest: " + d + "d " + h + "h " + m + "m " + s + "s");
                if (DealTimeText) DealTimeText.text = String.Format("{0:00}:{1:00}:{2:00}", h, m, s);
            };
            Refresh();
        }

        void Update()
        {
            if (enableTimer)
            {
                //update timer
                gT.Update();
                if (gT.IsTimePassed)
                    gT.Restart();
            }
        }

        /// <summary>
        /// Set all buttons interactble = activity
        /// </summary>
        /// <param name="activity"></param>
        public void SetControlActivity(bool activity)
        {
            if (buttons == null) return;
            foreach (Button b in buttons)
            {
              if(b)  b.interactable = activity;
            }
        }

        /// <summary>
        /// Refresh gui data : MoneyCount,Level
        /// </summary>
        internal void Refresh()
        {
            RefreshLevel();
            RefreshBalance();
        }

        /// <summary>
        /// Refresh gui level
        /// </summary>
        internal void RefreshLevel()
        {
            if (sP)
            {
                if (levelSlider) levelSlider.fillAmount = sP.LevelProgress / 100f;
                if (LevelNumberText) LevelNumberText.text = sP.Level.ToString();
            }
        }

        /// <summary>
        /// Refresh gui balance
        /// </summary>
        internal void RefreshBalance()
        {
            if (BalanceSumText && sP) BalanceSumText.text = sP.Coins.ToString();
        }

        #region header menu
        public void MainMenu_Click()
        {
            GuiController.Instance.ShowMainMenu();
        }

        public void Shop_Click()
        {
            GuiController.Instance.ShowShop();
        }

        public void GameIcon_Click(int level)
        {
            
        }

        public void FaceBook_Click()
        {
            if (FBholder.Instance)
            {
                FBholder.Instance.FBlogin();
            }
        }

        public void Deal_Click(int level)
        {
            GuiController.Instance.ShowMessageBigDeal("10000", "", () => { GuiController.Instance.ShowShop(); }, () => { }, null);
        }

        public void Level_Click()
        {
          if(sP)  GuiController.Instance.ShowLevelXP(sP.LevelProgress);
        }

        
        #endregion header menu

    }
}