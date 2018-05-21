using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/*
  22.03.18
    Add cycled
    Add Restart
 */

namespace Mkey
{
    public enum WinShowType { JumpSymbols, Zoom, LightFlashing}
    public class SlotController : MonoBehaviour
    {
        public List<PayLine> payTable;

        public bool useLineBetMultiplier = true;
        public SlotIcon[] slotIcons;

        public int scatter_id; 
        public int wild_id; 
        public bool useAsWildMajor;
        public bool useAsScatterMajor;
        public int bonus_id;
        public int freespin_id;
        public bool useAsBonusMajor;
        public bool useAsFreeSpinMajor;
        public int heart_id;
        public int diamond_id;
        public bool useAsDiamondMajor;
        public bool useAsHeartMajor;


        public GameObject tilePrefab;
        public GameObject particlesStars;

        public SlotGroupBehavior[] slotGroupsBeh;

        public Transform bottomJumpTarget;
        public Transform topJumpTarget;

        public EaseAnim rotType = EaseAnim.EaseInCubic;
        public float rotateTime = 4f;
        public int rotateTimeRandomize = 10;
        public WinShowType winShowType = WinShowType.JumpSymbols;
        public bool winLineFlashing = true;
        public bool winSymbolParticles = true;
        public RNGType RandomGenerator = RNGType.Unity;
        public int levelUpReward = 3000;

        public int maxLineBet = 20;


        private int slotTilesCount = 30;
        public static SlotController Instance;
        private WinController winController;
        private List<PayLine> payTableFull; // extended  if useWild
        WaitForSeconds wfs1_0;
        WaitForSeconds wfs0_2;
        RNG rng; // random numbers generator
        private bool auto = false;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            wfs1_0 = new WaitForSeconds(1.0f);
            wfs0_2 = new WaitForSeconds(0.2f);
        }

        int spinCount = 0;
        void Start()
        {
            // create slots
            int slotsGrCount = slotGroupsBeh.Length;
            ReelData[] reelsData = new ReelData[slotsGrCount];
            ReelData reelData;
            int i = 0;
            foreach (SlotGroupBehavior sGB in slotGroupsBeh)
            {
                reelData = new ReelData(sGB.symbOrder);
                reelsData[i++] = reelData;
                sGB.CreateSlotCylinder(slotIcons, slotTilesCount, tilePrefab);
            }

            payTableFull = new List<PayLine>();
            for (int j = 0; j < payTable.Count; j++)
            {
               payTableFull.Add(payTable[j]);
               if (useAsWildMajor) payTableFull.AddRange(payTable[j].GetWildLines(this));
            }

            winController = new WinController(this, FindObjectsOfType<LineBehavior>());
            rng = new RNG(RNGType.Unity, reelsData);

            SlotPlayer.Instance.ResetPrevSession();
            SlotPlayer.Instance.SetLineButtons();
            SlotMenuController.Instance.Refresh();
            SetInputActivity(true);
        }

        // int fullI = 0;
        //  int[] proc; 

        void Update()
        {
            rng.Update();
        }

        internal void ReStartSettings()
        {
            winController.WinEffectsShow(false, false);
            winController.ResetLineWinning();
            LineButtonBehavior[] lbbs = FindObjectsOfType<LineButtonBehavior>();
            for (int i = 0; i < lbbs.Length; i++)
            {
                if (lbbs[i].Pressed)
                {
                    lbbs[i].PointerDown(null);
                }
            }

            foreach (SlotGroupBehavior sGB in slotGroupsBeh)
            {
                sGB.TilesGroup.localEulerAngles = Vector3.zero;
            }
        }

        private int autoCount = 1;
        /// <summary>
        /// Run slots when you press the button
        /// </summary>
        internal void SpinPress()
        {
            autoCount = SlotPlayer.Instance.AutoSpinCount;
            RunSlots();
        }

        bool slotsRunned = false;
        private void RunSlots()
        {
            if (slotsRunned) return;
            winController.WinEffectsShow(false, false);
            winController.WinZoomCancel();
            winController.WinLightCancel();

            winController.ResetLineWinning();

            if (!SlotPlayer.Instance.AnyLineSelected)
            {
                GuiController.Instance.ShowMessage(null, "Please select a any line.", 1.5f, null);
                SlotMenuController.Instance.ResetAuto();
                return;
            }
            if (!SlotPlayer.Instance.ApllyFreeSpin() && !SlotPlayer.Instance.ApplyBet())
            {
                GuiController.Instance.ShowMessage(null, "You have no money.", 1.5f, null);
                SlotMenuController.Instance.ResetAuto();
                return;
            }
            if (auto && autoCount <= 0)
            {
                SlotMenuController.Instance.ResetAuto();
                return;
            }

            StartCoroutine(RunSlotsAsync());
        }

        private IEnumerator RunSlotsAsync()
        {
            SlotMenuController.Instance.SetWinInfo(0);
            slotsRunned = true;
            autoCount--;
            Debug.Log("spins: " + spinCount++);
            //1 ---------------start preparation-------------------------------
            SetInputActivity(false);
            yield return wfs1_0;
            winController.HideAllLines();
            SoundMasterController.Instance.SoundPlayRotation(2f, true, null);

            //2 --------start rotating ----------------------------------------
            bool fullRotated = false;
            RotateSlots(() => { SoundMasterController.Instance.StopLoopClip(); fullRotated = true; });
            while (!fullRotated) yield return wfs0_2;  // wait 

            //3 --------check result-------------------------------------------
            winController.FindWinnSymbols();
            bool hasLineWin = false;
            bool hasScatterWin = false;
            if (winController.HasAnyWinn(ref hasLineWin, ref hasScatterWin))
            {
                SlotMenuController.Instance.SetWinInfo(winController.GetWinCoins());
                bool showEnd = false;
                winController.WinEffectsShow(winLineFlashing, winSymbolParticles);
                winController.WinSymbolShow(
                    winShowType,
                       (spins, coins) => //linewin
                       {
                           if (useLineBetMultiplier) coins *= SlotPlayer.Instance.LineBet;
                           SlotPlayer.Instance.FreeSpins += spins;
                           SlotPlayer.Instance.Coins += coins;
                           
                           SlotMenuController.Instance.Refresh();
                           SlotPlayer.Instance.LevelProgress += 2f;
                       },
                       () => //scatter wint
                       {
                           SlotPlayer.Instance.LevelProgress += 100f;
                       },
                       () => 
                       {
                           
                           showEnd = true;
                       }
                       );
                while (!showEnd) yield return wfs0_2;  // wait for show end
            }
            else
            {
                SoundMasterController.Instance.SoundPlaySlotLoose(0, false, null);
                SlotPlayer.Instance.LevelProgress += 0.5f;
            }
            SetInputActivity(true);
            slotsRunned = false;

            if (auto)
            {
                RunSlots();
            }
        }

        private void RotateSlots(Action rotCallBack)
        {
            ParallelTween pT = new ParallelTween();
            int[] rands = rng.GetRandSymbols();
            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                int n = i;
                int r = rands[i];
                pT.Add((callBack) =>
                {
                    slotGroupsBeh[n].NextRotateCylinder(rotType, rotateTime, r, rotateTimeRandomize / 100f, callBack);// slotGroupsBeh[n].RotateCylinder(rotType, rotateTime, n, rotateTimeRandomize / 100f, callBack);
                });
            }

            pT.Start(rotCallBack);
        }

        /// <summary>
        /// Set touch activity for game and gui elements of slot scene
        /// </summary>
        private void SetInputActivity(bool activity)
        {
            if (activity)
            {
                TouchManager.SetTouchActivity(activity);
                SlotMenuController.Instance.SetControlActivity(activity);
            }
            else
            {
                TouchManager.SetTouchActivity(activity);
                SlotMenuController.Instance.SetControlActivity(activity, auto); // spin button set active for auto spin
            }
        }

        /// <summary>
        /// Set auto play
        /// </summary>
        /// <param name="auto"></param>
        public void SetAutoPlay(bool auto)
        {
            this.auto = auto;
        }

        /// <summary>
        /// Calculate propabilities
        /// </summary>
        /// <returns></returns>
        public string[,] CreatePropabilityTable()
        {
            List<string> rowList = new List<string>();
            string[] iconNames = GetIconNames(false);
            int length = slotGroupsBeh.Length;
            string[,] table = new string[length + 1, iconNames.Length + 1];

            rowList.Add("reel / icon");
            rowList.AddRange(iconNames);
            SetRow(table, rowList, 0, 0);

            for (int i = 1; i <= length; i++)
            {
                table[i, 0] = "reel #" + i.ToString();
                SetRow(table, new List<float>(slotGroupsBeh[i - 1].GetReelSymbHitPropabilities(slotIcons)), 1, i);
            }
            return table;
        }

        /// <summary>
        /// Calculate propabilities
        /// </summary>
        /// <returns></returns>
        public string[,] CreatePayTable(out float sumPayOut)
        {
             
        List<string> row = new List<string>();
            List<float[]> reelSymbHitPropabilities = new List<float[]>();
            string[] iconNames = GetIconNames(false);
           
            sumPayOut = 0;
            payTableFull = new List<PayLine>();
            for (int j = 0; j < payTable.Count; j++)
            {
                payTableFull.Add(payTable[j]);
                if (useAsWildMajor) payTableFull.AddRange(payTable[j].GetWildLines(this));
            }
            int rCount = payTableFull.Count + 1;
            int cCount = slotGroupsBeh.Length + 3;
            string[,] table = new string[rCount, cCount];
            row.Add("PayLine / reel");
            for (int i = 0; i < slotGroupsBeh.Length; i++)
            {
                row.Add("reel #" + (i + 1).ToString());
            }
            row.Add("Payout");
            row.Add("Payout, %");
            SetRow(table, row, 0, 0);

            PayLine pL;
            for (int i = 0; i < payTableFull.Count; i++)
            {
                pL = payTableFull[i];
                table[i + 1, 0] = "Payline #" + (i + 1).ToString();
                table[i + 1, cCount - 2] = pL.pay.ToString();
                float pOut = pL.GetPayOutProb(this);
                sumPayOut += pOut;
                table[i + 1, cCount - 1] = pOut.ToString("F6");
                SetRow(table, new List<string>(pL.Names(slotIcons)), 1, i + 1);
            }

            Debug.Log("sum % = " + sumPayOut);
            return table;
        }

        private void SetRow<T>(string[,] table, List<T> row, int beginColumn, int rowNumber)
        {
            if (rowNumber >= table.GetLongLength(0)) return;

            for (int i = 0; i < row.Count; i++)
            {
                if (i + beginColumn < table.GetLongLength(1)) table[rowNumber, i + beginColumn] = row[i].ToString();
            }
        }

        public string[] GetIconNames(bool addAny)
        {
            if (slotIcons == null || slotIcons.Length == 0) return null;
            int length = (addAny) ? slotIcons.Length + 1 : slotIcons.Length;
            string[] sName = new string[length];
            if (addAny) sName[0] = "any";
            int addN = (addAny) ? 1 : 0;
            for (int i = addN; i < length; i++)
            {
                if (slotIcons[i - addN] != null && slotIcons[i - addN].iconSprite != null)
                {
                    sName[i] = slotIcons[i - addN].iconSprite.name;
                }
                else
                {
                    sName[i] = (i - addN).ToString();
                }
            }
            return sName;
        }

        public bool IsMajorSymbol(int symbolID)
        {
            if (useAsWildMajor && symbolID == wild_id) return true;
            if (useAsScatterMajor && symbolID == scatter_id) return true;
            if (useAsBonusMajor && symbolID == bonus_id) return true;
            if (useAsFreeSpinMajor && symbolID == freespin_id) return true;
            if (useAsHeartMajor && symbolID == heart_id) return true;
            if (useAsDiamondMajor && symbolID == diamond_id) return true;
            return false;
        }
    }


    [Serializable]
    public class SlotIcon
    {
        public Sprite iconSprite;
        public Sprite addIconSprite;

        public SlotIcon(Sprite iconSprite, Sprite addIconSprite)
        {
            this.iconSprite = iconSprite;
            this.addIconSprite = addIconSprite;
        }
    }

    // Helper for winning symbols check
    public class WinController
    {
        List<LineBehavior>lineBehL;
        List<PayLine> payTable;
        SlotGroupBehavior[] slotGroupsBeh;
        private List<SlotSymbol> scatterWinSymbols;
        int scatter_id;
        bool useScatter;
        private GameObject particlesPrefab;
        Transform topJumpTarget;
        Transform bottomJumpTarget;
        

        public WinController(SlotController sC, LineBehavior [] lineBeh)
        {
            lineBehL = new List<LineBehavior>(lineBeh);
            payTable = sC.payTable;
            slotGroupsBeh = sC.slotGroupsBeh;
            scatter_id = sC.scatter_id;
            useScatter = sC.useAsScatterMajor;
            particlesPrefab = sC.particlesStars;
            topJumpTarget = sC.topJumpTarget;
            bottomJumpTarget = sC.bottomJumpTarget;
        }

        /// <summary>
        /// Return true if slot has any winning
        /// </summary>
        internal bool HasAnyWinn(ref bool hasLineWin, ref bool hasScatterWin)
        {
            hasLineWin = false;
            hasScatterWin = false;

            foreach (LineBehavior lB in lineBehL)
            {
                if (lB.IsWinningLine)
                {
                    hasLineWin = true;
                }
            }
            if (useScatter && HasScatterWin(scatter_id))
            {
                hasScatterWin = true;
            }
 
            return (hasLineWin || hasScatterWin);
        }

        /// <summary>
        /// Find winning symbols 
        /// </summary>
        internal void FindWinnSymbols()
        {
            foreach (LineBehavior lB in lineBehL)
            {
                if (lB.IsSelected)
                {
                    lB.FindWin(payTable);
                }
            }
        }

        /// <summary>
        /// Show symbols particles and lines glowing
        /// </summary>
        internal void WinEffectsShow(bool flashingLines, bool showSymbolParticles)
        {
            HideAllLines();
            lineBehL.ForEach((lB) =>
            {
                if (lB.IsWinningLine)
                {
                    lB.SetLineVisible(flashingLines);
                    lB.LineFlashing(flashingLines);
                }
                lB.ShowWinSymbolsParticles(showSymbolParticles);
            });

            if (useScatter && scatterWinSymbols != null && scatterWinSymbols.Count>0)
            {
                foreach (var item in scatterWinSymbols)
                {
                    item.ShowParticles(showSymbolParticles, SlotController.Instance.particlesStars);
                }
            }
        }

        /// <summary>
        /// Show win symbols 
        /// </summary>
        internal void WinSymbolShow(WinShowType winShowType, Action<int, int> lineWinCallBack,Action scatterWinCallBack, Action completeCallBack)
        {
            switch (winShowType)
            {
                case WinShowType.JumpSymbols:
                    WinSymbolJumpsShow(lineWinCallBack, scatterWinCallBack, completeCallBack);
                    break;
                case WinShowType.Zoom:
                    WinSymbolZoomShowContinuous(lineWinCallBack,scatterWinCallBack, completeCallBack);
                    break;
                case WinShowType.LightFlashing:
                    WinSymbolLightShowCont(lineWinCallBack, scatterWinCallBack, completeCallBack);
                    break;
            }
        }

        /// <summary>
        /// Show win symbols jumps
        /// </summary>
        internal void WinSymbolJumpsShow(Action<int, int> lineWinCallBack, Action scatterWinCallBack, Action completeCallBack)
        {
            TweenSeq ts = new TweenSeq();

            //show linewins
            foreach (LineBehavior lB in lineBehL)
            {
                if (lB.IsWinningLine)

                    ts.Add((callBack) =>
                    {
                        lB.LineWinJumps(0, topJumpTarget, bottomJumpTarget,
                            (spins, coins) =>
                        {
                            if (lineWinCallBack != null) lineWinCallBack(spins, coins); callBack();
                        });
                    });
            }

            //show scatterwin
            if (useScatter && scatterWinSymbols != null && scatterWinSymbols.Count > 0)
            {
                ParallelTween pT;
                pT = new ParallelTween();
                foreach (var item in scatterWinSymbols)
                {
                    pT.Add((callBack) =>
                    {
                        item.WinJump(callBack, topJumpTarget, bottomJumpTarget, 0);
                    });
                }
                ts.Add((callBack) =>
                {
                    pT.Start(() =>
                    {
                        if (scatterWinCallBack != null) scatterWinCallBack();
                        if (callBack != null) callBack();
                    });
                });
            }
            ts.Add((callBack) => { if (completeCallBack != null) completeCallBack(); callBack(); });
            ts.Start();
        }

        int contID;
        TweenSeq contTS;

        /// <summary>
        /// Show win symbols jumps
        /// </summary>
        internal void WinSymbolZoomShow(Action<int, int> lineWinCallBack, Action scatterWinCallBack, Action completeCallBack)
        {
            TweenSeq ts = new TweenSeq();
          
            foreach (LineBehavior lB in lineBehL)
            {
                if (lB.IsWinningLine)

                    ts.Add((callBack) =>
                    {
                        lB.LineWinZoom(0, 5, 
                            (spins, coins) =>
                            {
                                if (lineWinCallBack != null) lineWinCallBack(spins, coins); callBack();
                            });
                    });
            }

            //show scatterwin
            if (useScatter && scatterWinSymbols != null && scatterWinSymbols.Count > 0)
            {
                ParallelTween pT;
                pT = new ParallelTween();
                foreach (var item in scatterWinSymbols)
                {
                    pT.Add((callBack) =>
                    {
                        item.WinZoom(callBack, 0, 5);
                    });
                }
                ts.Add((callBack) =>
                {
                    pT.Start(() =>
                    {
                        if (scatterWinCallBack != null) scatterWinCallBack();
                        if (callBack != null) callBack();
                    });
                });
            }

            ts.Add((callBack) => { if (completeCallBack != null) completeCallBack(); callBack(); });
            ts.Start();
        }

        /// <summary>
        /// Show win symbols blin continuous
        /// </summary>
        internal void WinSymbolZoomShowContinuous(Action<int, int> lineWinCallBack, Action scatterWinCallBack, Action completeCallBack)
        {
            contTS = new TweenSeq();
            int length = lineBehL.Count;
            contTS.Add((callBack) => {
                WinSymbolZoomShow(lineWinCallBack, scatterWinCallBack, 
                    () =>
                {
                    if (completeCallBack != null) completeCallBack();
                    callBack();
                });
            });

            contTS.Add((callBack) =>
            {
                SimpleTween.SimpleTweenObject cont = SimpleTween.Value(SlotController.Instance.gameObject, 0, 1, 10f).SetCycled().AddCompleteCallBack( // use as timer
                    () =>
                    {
                        foreach (LineBehavior lb in lineBehL)
                        {
                            lb.LineWinZoomCancel();
                        }
                        WinSymbolZoomShow(null, null, null);
                    });
                contID = cont.ID;
            });
            contTS.Start();
        }

        /// <summary>
        /// Show won symbols jumps
        /// </summary>
        internal void WinSymbolLightShow(Action<int, int> lineWinCallBack, Action scatterWinCallBack, Action completeCallBack)
        {
            TweenSeq ts = new TweenSeq();

            foreach (LineBehavior lB in lineBehL)
            {
                if (lB.IsWinningLine)

                    ts.Add((callBack) =>
                    {
                        lB.LineWinLight(0, (spins, coins) =>
                        {
                            if (lineWinCallBack != null) lineWinCallBack(spins, coins); callBack();
                        });
                    });
            }

            //show scatterwin
            if (useScatter && scatterWinSymbols != null && scatterWinSymbols.Count > 0)
            {
                ParallelTween pT;
                pT = new ParallelTween();
                foreach (var item in scatterWinSymbols)
                {
                    pT.Add((callBack) =>
                    {
                        item.WinLight(callBack, 0, 5);
                    });
                }
                ts.Add((callBack) =>
                {
                    pT.Start(() =>
                    {
                        if (scatterWinCallBack != null) scatterWinCallBack();
                        if (callBack != null) callBack();
                    });
                });
            }

            ts.Add((callBack) => { if (completeCallBack != null) completeCallBack(); callBack(); });
            ts.Start();
        }

        /// <summary>
        /// Show won symbols jumps
        /// </summary>
        internal void WinSymbolLightShowCont(Action<int, int> lineWinCallBack, Action scatterWinCallBack, Action completeCallBack)
        {
            contTS = new TweenSeq();
            int length = lineBehL.Count;
            contTS.Add((callBack) => {
                WinSymbolLightShow(lineWinCallBack, scatterWinCallBack, () =>
                {
                    if (completeCallBack != null) completeCallBack();
                    callBack();
                });
            });

            contTS.Add((callBack) =>
            {
                SimpleTween.SimpleTweenObject cont = SimpleTween.Value(SlotController.Instance.gameObject, 0, 1, 10f).SetCycled().AddCompleteCallBack( // use as timer
                    () =>
                    {
                        foreach (LineBehavior lb in lineBehL)
                        {
                            lb.LineWinLightCancel();
                        }
                        WinSymbolLightShow(null, null, null);
                    });
                contID = cont.ID;
            });
            contTS.Start();
        }

        /// <summary>
        /// Show selected lines with flashing or without
        /// </summary>
        internal void ShowSelectedLines(bool flashing)
        {
            lineBehL.ForEach((lB) =>
            {
                if (lB.IsSelected)
                {
                    lB.SetLineVisible(true);
                }
                lB.LineFlashing(flashing);
            });
        }

        /// <summary>
        /// Hide selected lines
        /// </summary>
        internal void HideAllLines()
        {
            lineBehL.ForEach((lB) =>
            {
                lB.LineFlashing(false);
                lB.SetLineVisible(false);
            });
        }

        /// <summary>
        /// Reset winning line data
        /// </summary>
        internal void ResetLineWinning()
        {
            foreach (LineBehavior lb in lineBehL)
            {
                lb.ResetLineWinning();
            }
        }

        internal void WinZoomCancel()
        {
            if (contTS != null) contTS.Break(); 
          
            foreach (LineBehavior lb in lineBehL)
            {
                lb.LineWinZoomCancel();
            }

            if(useScatter && scatterWinSymbols!=null)
            foreach (var item in scatterWinSymbols)
            {
                    item.WinZoomCancel();
            }
            SimpleTween.Cancel(contID, false);
        }

        internal void WinLightCancel()
        {
            if (contTS != null) contTS.Break();

            foreach (LineBehavior lb in lineBehL)
            {
                lb.LineWinLightCancel();
            }
            if (useScatter && scatterWinSymbols != null)
                foreach (var item in scatterWinSymbols)
                {
                    item.WinLightCancel();
                }

            SimpleTween.Cancel(contID, false);
        }

        private bool HasScatterWin(int scatter_id)
        {
            scatterWinSymbols = new List<SlotSymbol>();
            List<SlotSymbol> scatterSymbolsTemp = new List<SlotSymbol>();
            foreach (var item in slotGroupsBeh)
            {
                if (!item.HasSymbolInAnyRayCaster(scatter_id, ref scatterSymbolsTemp))
                {
                    scatterWinSymbols = new List<SlotSymbol>();
                    return false;
                }
                else
                {
                    scatterWinSymbols.AddRange(scatterSymbolsTemp);
                }
            }
            return true;
        }

        private void ShowScatterWin(Action completeCallBack)
        {
            if (scatterWinSymbols == null || scatterWinSymbols.Count == 0)
            {
                if (completeCallBack != null) completeCallBack();
            }
        }

        public int GetWinCoins()
        {
            int res = 0;
            foreach (LineBehavior lB in lineBehL)
            {
                if (lB.IsWinningLine)
                {
                    res += lB.win.Pay;
                }
            }
            return res;
        }
    }

    // GetInterface method for gameobject
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Returns all monobehaviours (casted to T)
        /// </summary>
        /// <typeparam name="T">interface type</typeparam>
        /// <param name="gObj"></param>
        /// <returns></returns>
        public static T[] GetInterfaces<T>(this GameObject gObj)
        {
            if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");
            //  var mObjs = gObj.GetComponents<MonoBehaviour>();
            var mObjs = MonoBehaviour.FindObjectsOfType<MonoBehaviour>();
            return (from a in mObjs where a.GetType().GetInterfaces().Any(k => k == typeof(T)) select (T)(object)a).ToArray();
        }

        /// <summary>
        /// Returns the first monobehaviour that is of the interface type (casted to T)
        /// </summary>
        /// <typeparam name="T">Interface type</typeparam>
        /// <param name="gObj"></param>
        /// <returns></returns>
        public static T GetInterface<T>(this GameObject gObj)
        {
            if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");
            return gObj.GetInterfaces<T>().FirstOrDefault();
        }

        /// <summary>
        /// Returns the first instance of the monobehaviour that is of the interface type T (casted to T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gObj"></param>
        /// <returns></returns>
        public static T GetInterfaceInChildren<T>(this GameObject gObj)
        {
            if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");
            return gObj.GetInterfacesInChildren<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all monobehaviours in children that implement the interface of type T (casted to T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gObj"></param>
        /// <returns></returns>
        public static T[] GetInterfacesInChildren<T>(this GameObject gObj)
        {
            if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");

            var mObjs = gObj.GetComponentsInChildren<MonoBehaviour>();

            return (from a in mObjs where a.GetType().GetInterfaces().Any(k => k == typeof(T)) select (T)(object)a).ToArray();
        }
    }

    public enum RNGType { Unity, MersenneTwister }
    public class RNG
    {
        private int[] randSymb;
        private RNGType rngType;
        private Action UpdateRNGAction;
        private ReelData[] reelsData;
        private RandomMT randomMT;

        public RNG(RNGType rngType, ReelData[] reelsData)
        {
            randSymb = new int[reelsData.Length];
            this.rngType = rngType;
            this.reelsData = reelsData;
            switch (rngType)
            {
                case RNGType.Unity:
                    UpdateRNGAction = UnityRNGUpdate;
                    break;
                case RNGType.MersenneTwister:
                    randomMT = new RandomMT();
                    UpdateRNGAction = MTRNGUpdate;
                    break;
                default:
                    UpdateRNGAction = UnityRNGUpdate;
                    break;
            }
        }

        public void Update()
        {
            UpdateRNGAction();
        }

        public int[] GetRandSymbols()
        {
            return randSymb;
        }

        int rand;
        private void UnityRNGUpdate()
        {
            for (int i = 0; i < randSymb.Length; i++)
            {
                rand = UnityEngine.Random.Range(0, reelsData[i].Length);
                randSymb[i] = rand;
            }
        }

        private void MTRNGUpdate()
        {
            for (int i = 0; i < randSymb.Length; i++)
            {
                rand = randomMT.RandomRange(0, reelsData[i].Length-1);
                randSymb[i] = rand;
            }
        }
    }

    [Serializable]
    public class ReelData
    {
        public List<int> symbOrder;
        public ReelData(List<int> symbOrder)
        {
            this.symbOrder = symbOrder;
        }
        public int Length
        {
            get { return (symbOrder == null) ? 0 : symbOrder.Count; }
        }
        public int GetSymbolAtPos(int position)
        {
            return (symbOrder == null || position >= symbOrder.Count) ? 0 : symbOrder.Count;
        }
    }

    /// <summary>
	/// Summary description for RandomMT.https://www.codeproject.com/Articles/5147/A-C-Mersenne-Twister-class
	/// </summary>
	public class RandomMT
    {
        private const int N = 624;
        private const int M = 397;
        private const uint K = 0x9908B0DFU;
        private const uint DEFAULT_SEED = 4357;

        private ulong[] state = new ulong[N + 1];
        private int next = 0;
        private ulong seedValue;


        public RandomMT()
        {
            SeedMT(DEFAULT_SEED);
        }
        public RandomMT(ulong _seed)
        {
            seedValue = _seed;
            SeedMT(seedValue);
        }

        public ulong RandomInt()
        {
            ulong y;

            if ((next + 1) > N)
                return (ReloadMT());

            y = state[next++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9D2C5680U;
            y ^= (y << 15) & 0xEFC60000U;
            return (y ^ (y >> 18));
        }

        private void SeedMT(ulong _seed)
        {
            ulong x = (_seed | 1U) & 0xFFFFFFFFU;
            int j = N;

            for (j = N; j >= 0; j--)
            {
                state[j] = (x *= 69069U) & 0xFFFFFFFFU;
            }
            next = 0;
        }

        public int RandomRange(int lo, int hi)
        {
            return (Math.Abs((int)RandomInt() % (hi - lo + 1)) + lo);
        }

        public int RollDice(int face, int number_of_dice)
        {
            int roll = 0;
            for (int loop = 0; loop < number_of_dice; loop++)
            {
                roll += (RandomRange(1, face));
            }
            return roll;
        }

        public int HeadsOrTails() { return ((int)(RandomInt()) % 2); }

        public int D6(int die_count) { return RollDice(6, die_count); }
        public int D8(int die_count) { return RollDice(8, die_count); }
        public int D10(int die_count) { return RollDice(10, die_count); }
        public int D12(int die_count) { return RollDice(12, die_count); }
        public int D20(int die_count) { return RollDice(20, die_count); }
        public int D25(int die_count) { return RollDice(25, die_count); }


        private ulong ReloadMT()
        {
            ulong[] p0 = state;
            int p0pos = 0;
            ulong[] p2 = state;
            int p2pos = 2;
            ulong[] pM = state;
            int pMpos = M;
            ulong s0;
            ulong s1;

            int j;

            if ((next + 1) > N)
                SeedMT(seedValue);

            for (s0 = state[0], s1 = state[1], j = N - M + 1; --j > 0; s0 = s1, s1 = p2[p2pos++])
                p0[p0pos++] = pM[pMpos++] ^ (mixBits(s0, s1) >> 1) ^ (loBit(s1) != 0 ? K : 0U);


            for (pM[0] = state[0], pMpos = 0, j = M; --j > 0; s0 = s1, s1 = p2[p2pos++])
                p0[p0pos++] = pM[pMpos++] ^ (mixBits(s0, s1) >> 1) ^ (loBit(s1) != 0 ? K : 0U);


            s1 = state[0];
            p0[p0pos] = pM[pMpos] ^ (mixBits(s0, s1) >> 1) ^ (loBit(s1) != 0 ? K : 0U);
            s1 ^= (s1 >> 11);
            s1 ^= (s1 << 7) & 0x9D2C5680U;
            s1 ^= (s1 << 15) & 0xEFC60000U;
            return (s1 ^ (s1 >> 18));
        }

        private ulong hiBit(ulong _u)
        {
            return ((_u) & 0x80000000U);
        }
        private ulong loBit(ulong _u)
        {
            return ((_u) & 0x00000001U);
        }
        private ulong loBits(ulong _u)
        {
            return ((_u) & 0x7FFFFFFFU);
        }
        private ulong mixBits(ulong _u, ulong _v)
        {
            return (hiBit(_u) | loBits(_v));

        }
    }

    [Serializable]
    //Helper class for creating pay table
    public class PayTable
    {
        public int reelsCount;
        public List<PayLine> payLines;
    }

    [Serializable]
    public class PayLine
    {
        public int[] line;
        public int pay;
        public int freeSpins;

        public PayLine()
        {
            line = new int[5];
            for (int i = 0; i < line.Length; i++)
            {
                line[i] = -1;
            }
        }

        public PayLine(int reelsCount)
        {
            line = new int[reelsCount];
            for (int i = 0; i < line.Length; i++)
            {
                line[i] = -1;
            }
        }

        public PayLine(PayLine pLine)
        {
            if (pLine.line != null)
            {
                line = new int[pLine.line.Length];
                for (int i = 0; i < line.Length; i++)
                {
                    line[i] = pLine.line[i];
                }
                pay = pLine.pay;
                freeSpins = pLine.freeSpins;
            }
            else
            {
                line = new int[5];
                for (int i = 0; i < line.Length; i++)
                {
                    line[i] = -1;
                }
            }
        }

        public PayLine(int [] line, int pay, int freeSpins)
        {
            if (line!= null)
            {
                this.line =line;
                this.pay = pay;
                this.freeSpins = freeSpins;
            }
        }

        public string ToString(Sprite[] sprites)
        {
            string res = "";
            if (line == null) return res;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] >= 0)
                    res += sprites[line[i]].name;
                else
                {
                    res += "any";
                }
                if (i < line.Length - 1) res += ";";
            }
            return res;
        }

        public string[] Names(SlotIcon[] sprites)
        {
            if (line == null) return null;
            string[] res = new string[line.Length];
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] >= 0)
                    res[i] = (sprites[line[i]] != null && sprites[line[i]].iconSprite != null) ? sprites[line[i]].iconSprite.name : "failed";
                else
                {
                    res[i] = "any";
                }
            }
            return res;
        }

        public float GetPayOutProb(SlotController sC)
        {
            float res = 0;
            if (!sC) return res;
            if (line == null || sC.slotGroupsBeh == null || sC.slotGroupsBeh.Length != line.Length) return res;
            float[] rP = sC.slotGroupsBeh[0].GetReelSymbHitPropabilities(sC.slotIcons);

            res = rP[line[0]];

            for (int i = 1; i < line.Length; i++)
            {
                if (line[i] >= 0)
                {
                    rP = sC.slotGroupsBeh[i].GetReelSymbHitPropabilities(sC.slotIcons);
                    res *= rP[line[i]];
                }
                else
                {
                    break;
                }
            }
           // return res * 100.0f;
            return res * (float)pay * 100.0f;
        }

        public bool IsMatch3()
        {
            if (line.Length == 3 && line[0] == line[1] && line[0] == line[2])
            {
                return true;
            }
            else if (line.Length > 3 && line[0] == line[1] && line[0] == line[2] && line[0] != line[3])
            {
                return true;
            }
            return false;
        }

        public bool IsMatch4()
        {
            if (line.Length == 4 && line[0] == line[1] && line[0] == line[2] && line[0] == line[3])
            {
                return true;
            }
            else if (line.Length > 4 && line[0] == line[1] && line[0] == line[2] && line[0] == line[3] && line[0] != line[4])
            {
                return true;
            }
            return false;
        }

        public bool IsMatch5()
        {
            if (line.Length == 5 && line[0] == line[1] && line[0] == line[2] && line[0] == line[3] && line[0] == line[4])
            {
                return true;
            }
            else if (line.Length > 5 && line[0] == line[1] && line[0] == line[2] && line[0] == line[3] && line[0] == line[4] && line[0] != line[5])
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Create and return additional lines for this line with wild symbol, for major symbols return empty list
        /// </summary>
        /// <returns></returns>
        public List<PayLine> GetWildLines(SlotController sC)
        {
            List<PayLine> res = new List<PayLine>();
            if (!sC) return res;
            if (!sC.useAsWildMajor) return res;

            int wild_id = sC.wild_id;
            int first = line[0];
            if(sC.IsMajorSymbol(first)) return res;

            if (IsMatch3())
            {
                //1)     F W W ? ?
                PayLine pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[2] = wild_id;
                res.Add(pl1);

                //2)     F F W ? ?
                pl1 = new PayLine(this);
                pl1.line[2] = wild_id;
                res.Add(pl1);

                //2)     F W F ? ?
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                res.Add(pl1);

            }
            else if (IsMatch4())
            {
                #region W 3 of 4
                //1)     F W W W ?
                PayLine pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[2] = wild_id;
                pl1.line[3] = wild_id;
                res.Add(pl1);
                #endregion W 3 of 4

                #region W 2 of 4
                //2)     F F W W ?
                pl1 = new PayLine(this);
                pl1.line[2] = wild_id;
                pl1.line[3] = wild_id;
                res.Add(pl1);

                //3)     F W F W ?
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[3] = wild_id;
                res.Add(pl1);

                //4)     F W W F ?
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[2] = wild_id;
                res.Add(pl1);
                #endregion W 1 of 4

                #region W 1 of 4
                //5)     F F F W ? 
                pl1 = new PayLine(this);
                pl1.line[3] = wild_id;
                res.Add(pl1);

                //6)     F F W F ? 
                pl1 = new PayLine(this);
                pl1.line[2] = wild_id;
                res.Add(pl1);

                //7)     F W F F ?
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                res.Add(pl1);
                #endregion W 1 of 4

            }
            else if (IsMatch5())
            {
                #region W 4 of 5
                //1)     F W W W W
                PayLine pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[2] = wild_id;
                pl1.line[3] = wild_id;
                pl1.line[4] = wild_id;
                res.Add(pl1);
                #endregion W 4 of 5

                #region W 3 of 5
                //2)     F F W W W
                pl1 = new PayLine(this);
                pl1.line[2] = wild_id;
                pl1.line[3] = wild_id;
                pl1.line[4] = wild_id;
                res.Add(pl1);

                //3)     F W F W W
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[3] = wild_id;
                pl1.line[4] = wild_id;
                res.Add(pl1);

                //4)     F W W F W
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[2] = wild_id;
                pl1.line[4] = wild_id;
                res.Add(pl1);

                //5)     F W W W F
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[2] = wild_id;
                pl1.line[3] = wild_id;
                res.Add(pl1);
                #endregion  W 3 of 5

                #region W 2 of 5
                //6)     F F F W W
                pl1 = new PayLine(this);
                pl1.line[3] = wild_id;
                pl1.line[4] = wild_id;
                res.Add(pl1);

                //7)     F F W F W
                pl1 = new PayLine(this);
                pl1.line[2] = wild_id;
                pl1.line[4] = wild_id;
                res.Add(pl1);

                //8)     F F W W F
                pl1 = new PayLine(this);
                pl1.line[2] = wild_id;
                pl1.line[3] = wild_id;
                res.Add(pl1);

                //9)     F W F W F
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[3] = wild_id;
                res.Add(pl1);

                //10)     F W W F F
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                pl1.line[2] = wild_id;
                res.Add(pl1);
                #endregion W 2 of 5

                #region W 1 of 5
                //10)     F F F F W
                pl1 = new PayLine(this);
                pl1.line[4] = wild_id;
                res.Add(pl1);

                //11)     F F F W F
                pl1 = new PayLine(this);
                pl1.line[3] = wild_id;
                res.Add(pl1);

                //12)     F F W F F
                pl1 = new PayLine(this);
                pl1.line[2] = wild_id;
                res.Add(pl1);

                //13)     F W F F F
                pl1 = new PayLine(this);
                pl1.line[1] = wild_id;
                res.Add(pl1);
                #endregion W 1 of 5
            }
            return res;
        }
    }

    static class ClassExt
    {
        public enum FieldAllign { Left, Right, Center}

        /// <summary>
        /// Return formatted string; (F2, N5, e, r, p, X, D12, C)
        /// </summary>
        /// <param name="fNumber"></param>
        /// <param name="format"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string ToString(this float fNumber, string format, int field)
        {
            string form = "{0," + field.ToString() +":"+ format + "}";
            string res = String.Format(form, fNumber);
            return res;
        }

        /// <summary>
        /// Return formatted string; (F2, N5, e, r, p, X, D12, C)
        /// </summary>
        /// <param name="fNumber"></param>
        /// <param name="format"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string ToString(this string s, int field)
        {
            string form = "{0," + field.ToString() +"}";
            string res = String.Format(form, s);
            return res;
        }

        /// <summary>
        /// Return formatted string; (F2, N5, e, r, p, X, D12, C)
        /// </summary>
        /// <param name="fNumber"></param>
        /// <param name="format"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string ToString(this string s, int field, FieldAllign fAllign)
        {
            int length = s.Length;
            if (length >= field)
            {
                string form = "{0," + field.ToString() + "}";
                return String.Format(form, s);
            }
            else
            {
                if (fAllign == FieldAllign.Center)
                {
                    int lCount = (field - length) / 2;
                    int rCount = field - length - lCount;
                    string lSp = new string('*', lCount);
                    string rSp = new string('*', rCount);
                    return (lSp + s + rSp);
                }
                else if (fAllign == FieldAllign.Left)
                {
                    int lCount = (field - length);
                    string lSp = new string('*', lCount);
                    return (s+lSp);
                }
                else
                {
                    string form = "{0," + field.ToString() + "}";
                    return  String.Format(form, s);
                }
            }
        }

        private static string ToStrings<T>(T[] a)
        {
            string res = "";
            for (int i = 0; i < a.Length; i++)
            {
                res += a[i].ToString();
                res += " ";
            }
            return res;
        }

        private static string ToStrings(float[] a, string format, int field)
        {
            string res = "";
            for (int i = 0; i < a.Length; i++)
            {
                res += a[i].ToString(format, field);
                res += " ";
            }
            return res;
        }

        private static string ToStrings(string[] a, int field, ClassExt.FieldAllign allign)
        {
            string res = "";
            for (int i = 0; i < a.Length; i++)
            {
                res += a[i].ToString(field, allign);
                res += " ";
            }
            return res;
        }

        private static float[] Mul(float[] a, float[] b)
        {
            if (a.Length != b.Length) return null;
            float[] res = new float[a.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = a[i] * b[i];
            }
            return res;
        }

    }

}