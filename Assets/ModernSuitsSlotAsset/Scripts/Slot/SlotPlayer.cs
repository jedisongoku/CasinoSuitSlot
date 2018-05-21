using UnityEngine;
using System.Collections.Generic;

namespace Mkey
{
    public class SlotPlayer : MonoBehaviour
    {
        [Space(10, order = 0)]
        [Header("Default data", order = 1)]
        public int defCoinsCount = 500; // default data
        public int defFreeSpin = 0;
        public int maxAutoSpins = 100;
        public bool saveData = false;

        #region keys
        private string saveCoinsKey = "mk_slot_coins"; // current coins
        private string saveLevelKey = "mk_slot_level"; // current level
        private string saveLevelProgressKey = "mk_slot_level_progress"; // progress to next level %
        private string saveAutoSpinsKey = "mk_slot_autospins"; // current auto spins
        #endregion keys

        private int coins;
        private int freeSpins;
        private int level = 0;
        private float levelProgress = 0;
        private int lineBet = 1;
        private int autoSpin = 1;
        private int playedSpins;

        private int selLinesCount = 0;
        public static SlotPlayer Instance;

        public int FreeSpins
        {
            get { return freeSpins; }
            set
            {
                freeSpins = value;
                if (freeSpins < 0) freeSpins = 0;
                RefReshGui(false, false, false, true);
            }
        }
        public bool HasFreeSpin
        {
            get { return FreeSpins > 0; }
        }
        public int TotalBet
        {
            get { return LineBet * SelectedLinesCount; }
        }
        public int LineBet
        {
            get { return lineBet; }
            set
            {
                lineBet = value;
                int maxLineBet = SlotController.Instance.maxLineBet;
                if (lineBet > maxLineBet) lineBet = maxLineBet;
                if (lineBet < 1) lineBet = 1;
                RefReshGui(false, false, true, false);
            }
        }
        public int AutoSpinCount
        {
            get
            {
                if (saveData)
                {
                    string key = saveAutoSpinsKey;
                    if (PlayerPrefs.HasKey(key))
                        autoSpin = PlayerPrefs.GetInt(key);
                }
                return autoSpin;
            }
            set
            {
                if (value < 1) value = 1;
                if (value > maxAutoSpins) value = maxAutoSpins;
                if (saveData)
                {
                    string key = saveAutoSpinsKey;
                    PlayerPrefs.SetInt(key, value);
                }
                autoSpin = value;
                RefReshGui(false, false, false, true);
            }
        }

        #region saved properties
        public int Coins
        {
            get
            {
                if (saveData)
                {
                    string key = saveCoinsKey;
                    if (PlayerPrefs.HasKey(key))
                        coins = PlayerPrefs.GetInt(key);
                    else
                    {
                        PlayerPrefs.SetInt(key, defCoinsCount);
                    }
                }
                return coins;
            }
            set
            {
                if (value < 0) value = 0;
                if (saveData)
                {
                    string key = saveCoinsKey;
                    PlayerPrefs.SetInt(key, value);
                }
                coins = value;
                RefReshGui(false, true, false, false);
            }
        }
        public int Level
        {
            get
            {
                if (saveData)
                {
                    string key = saveLevelKey;
                    if (PlayerPrefs.HasKey(key))
                        level = PlayerPrefs.GetInt(key);
                }
                return level;
            }
            set
            {
                if (saveData)
                {
                    string key = saveLevelKey;
                    PlayerPrefs.SetInt(key, value);
                }
                level = value;
                RefReshGui(true, false, false, false);
            }
        }
        public float LevelProgress
        {
            get
            {
                if (saveData)
                {
                    string key = saveLevelProgressKey;
                    if (PlayerPrefs.HasKey(key))
                        levelProgress = PlayerPrefs.GetFloat(key);
                }
                return levelProgress;
            }
            set
            {
                if (value >= 100f)
                {
                    Level++;
                    int levelUpReward = SlotController.Instance.levelUpReward;
                    if (levelUpReward > 0) GuiController.Instance.ShowMessageLevelUpCongratulation(levelUpReward.ToString(), Level.ToString(), () => { Coins += levelUpReward; }, null, null);
                    value = 0;
                }
                if (saveData)
                {
                    string key = saveLevelProgressKey;
                    PlayerPrefs.SetFloat(key, value);
                }
                levelProgress = value;
                RefReshGui(true, false, false, false);
            }
        }
        #endregion saved properties

  
        public int SelectedLinesCount
        {
            get { return selLinesCount; }
        }
        private List<LineButtonBehavior> lineButtons;

        internal bool AnyLineSelected
        {
            get { return selLinesCount > 0; }
        }
        internal bool HasMoneyForBet
        {
            get { return TotalBet <= Coins; }
        }

        private SlotMenuController sMC
        {
            get { return SlotMenuController.Instance; }
        }
        private LobbyMenuController lMC
        {
            get { return LobbyMenuController.Instance; }
        }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            if (!saveData) SetStartSettings();
            SetLineButtons();
            RefReshGui();
        }

        /// <summary>
        /// Set lines control to player
        /// </summary>
        public void SetLineButtons()
        {
            // add line buttons handlers
            lineButtons = new List<LineButtonBehavior>(FindObjectsOfType<LineButtonBehavior>());
            for (int i = 0; i < lineButtons.Count; i++)
            {
                lineButtons[i].PressButtonDelegate += () =>
                {
                    selLinesCount++;
                    RefReshGui();
                };

                lineButtons[i].UnPressButtonDelegate += () =>
                {
                    selLinesCount--;
                    RefReshGui();
                };
            }

            // sort buttons by button.number
            lineButtons.Sort((LineButtonBehavior a, LineButtonBehavior b) =>
            {
                if (a == null & b == null) return 0;
                else if (a == null) return -1;
                else if (b == null) return 1;
                else return a.number.CompareTo(b.number);
            });
        }

        public void ResetPrevSession()
        {
            selLinesCount = 0;
            freeSpins = 0;
        }

        /// <summary>
        /// Set default coins count, default free spins
        /// </summary>
        internal void SetStartSettings()
        {
            Coins = defCoinsCount;
            FreeSpins = defFreeSpin;
        }

        /// <summary>
        /// If has money for bet, dec money, and return true
        /// </summary>
        /// <returns></returns>
        internal bool ApplyBet()
        {
            if (HasMoneyForBet)
            {
                Coins -= TotalBet;
                RefReshGui();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// If has free spins, dec free spin and return true.
        /// </summary>
        /// <returns></returns>
        internal bool ApllyFreeSpin()
        {
            if (HasFreeSpin)
            {
                FreeSpins--;
                RefReshGui(false, false, false, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Add pay line
        /// </summary>
        internal void IncSelectedLines()
        {
            Debug.Log("inc");
            foreach (var item in lineButtons)
            {
                if (!item.Pressed)
                {
                    item.PointerDown(null);
                    break;
                }
            }
        }

        /// <summary>
        /// Remove pay line
        /// </summary>
        internal void DecSelectedLines()
        {
            for (int i = lineButtons.Count - 1; i >= 0; i--)
            {
                if (lineButtons[i].Pressed)
                {
                    lineButtons[i].PointerDown(null);
                    break;
                }
            }
        }

        internal void SetMaxBet()
        {
            RefReshGui();
            LineBet = SlotController.Instance.maxLineBet;
            SelectAllLines();
        }

        private void SelectAllLines()
        {
            foreach (var item in lineButtons)
            {
                if (!item.Pressed)
                {
                    item.PointerDown(null);
                }
            }
        }

        private void RefReshGui()
        {
            if (sMC)
            {
                sMC.Refresh();
            }
            if (lMC)
            {
                lMC.Refresh();
            }
        }

        private void RefReshGui(bool refreshLevel, bool refreshBalance, bool refreshBetLines, bool refreshSpins )
        {
            if (sMC)
            {
               if(refreshLevel) sMC.RefreshLevel();
               if (refreshBalance) sMC.RefreshBalance();
               if (refreshBetLines) sMC.RefreshBetLines();
               if (refreshSpins) sMC.RefreshSpins();
            }
            if (lMC)
            {
                lMC.Refresh();
            }
        }
    }
}