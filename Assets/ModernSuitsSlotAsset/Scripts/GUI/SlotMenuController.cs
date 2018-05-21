using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class SlotMenuController : MonoBehaviour
    {
        public Image levelSlider;
        public Text LevelNumberText;
        public Text BalanceSumText;
        [Space(16, order = 0)]
        public Text LineBetSumText;
        public Text TotalBetSumText;
        public Text LinesCountText;
        public Text AutoSpinsCountText;
        public Text FreeSpinText;
        public Text FreeSpinCountText;
        public Text InfoText;
        public Button spinButton;
        int winCoins;

        public static SlotMenuController Instance;
        private Button[] buttons;
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

        private void Start()
        {
            // set start button delegates
            if (spinButton)
            {
                spinButton.GetComponent<StartButtonBehavior>().ClickDelegate = () =>
                {
                    SlotController.Instance.SpinPress();
                };

                spinButton.GetComponent<StartButtonBehavior>().ChangeStateDelegate = (auto) =>
                {
                    SlotController.Instance.SetAutoPlay(auto);
                };
            }
            buttons = GetComponentsInChildren<Button>();
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
        /// Set all buttons interactble = activity, but startButton = startButtonAcivity
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="startButtonAcivity"></param>
        public void SetControlActivity(bool activity, bool startButtonAcivity)
        {
            if (buttons != null)
            {
                foreach (Button b in buttons)
                {
                 if(b) b.interactable = activity;
                }
            }
           if(spinButton) spinButton.interactable = startButtonAcivity;
        }

        /// <summary>
        /// Refresh gui data : MoneyCount,  BetCount, freeSpin
        /// </summary>
        internal void Refresh()
        {
            #region header refresh
            RefreshLevel();
            RefreshBalance();
            #endregion header refresh

            #region footer refresh
            RefreshBetLines();
            RefreshSpins();
            RefreshInfo();
            #endregion footer refresh
        }

        public void SetWinInfo(int coins)
        {
            Debug.Log("win coins : " + coins);
            winCoins = coins;
            RefreshInfo();
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
            if (sP)
            {
                if (BalanceSumText) BalanceSumText.text = sP.Coins.ToString();
            }
        }

        /// <summary>
        /// Refresh gui lines, bet
        /// </summary>
        internal void RefreshBetLines()
        {
            if (sP)
            {
                if (LineBetSumText) LineBetSumText.text = sP.LineBet.ToString();
                if (TotalBetSumText) TotalBetSumText.text = sP.TotalBet.ToString();
                if (LinesCountText) LinesCountText.text = sP.SelectedLinesCount.ToString();
            }
        }

        /// <summary>
        /// Refresh gui spins
        /// </summary>
        internal void RefreshSpins()
        {
            if (sP)
            {
                if (AutoSpinsCountText) AutoSpinsCountText.text = sP.AutoSpinCount.ToString();
                if (FreeSpinText) FreeSpinText.gameObject.SetActive(sP.FreeSpins > 0);
                if (FreeSpinCountText) FreeSpinCountText.text = (sP.FreeSpins > 0) ? sP.FreeSpins.ToString() : "";
            }
        }

        internal void RefreshInfo()
        {
            if (sP)
            {
                if (winCoins == 0)
                {
                    if (InfoText)
                    {
                        InfoText.text = (sP.TotalBet > 0) ? "Click to SPIN to start!" : "Select any slot line!";
                    }
                }
                else InfoText.text = winCoins.ToString();
            }
        }

        /// <summary>
        /// Set play state to single rotation
        /// </summary>
        public void ResetAuto()
        {
            spinButton.GetComponent<StartButtonBehavior>().ResetAuto();
        }

        #region header menu
        public void MainMenu_Click()
        {
            GuiController.Instance.ShowMainMenu();
        }

        public void GameInfo_Click3x5()
        {
            GuiController.Instance.ShowGameInfo3x5();
        }

        public void GameInfo_Click3x3()
        {
            GuiController.Instance.ShowGameInfo3x3();
        }

        public void Lobby_Click()
        {
            SceneLoader.Instance.LoadScene(0);
        }

        public void Level_Click()
        {
            if (sP) GuiController.Instance.ShowLevelXP(sP.LevelProgress);
        }

        public void Shop_Click()
        {
            GuiController.Instance.ShowShop();
        }

        #endregion header menu


        #region footer menu
        public void LinesPlus_Click()
        {
            sP.IncSelectedLines();
        }

        public void LinesMinus_Click()
        {
            sP.DecSelectedLines();
        }

        public void LineBetPlus_Click()
        {
           sP.LineBet++;
        }

        public void LineBetMinus_Click()
        {
           sP.LineBet--;
        }

        public void AutoSpinPlus_Click()
        {
            sP.AutoSpinCount++;
        }

        public void AutoSpinMinus_Click()
        {
            sP.AutoSpinCount--;
        }

        public void MaxBet_Click()
        {
            sP.SetMaxBet();
        }
        #endregion footer menu

        private string GetMoneyName(int count)
        {
            if (count > 1) return "coins";
            else return "coin";
        }
    }
}