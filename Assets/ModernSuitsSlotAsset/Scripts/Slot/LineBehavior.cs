using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Mkey
{
    public class LineBehavior : MonoBehaviour
    {
        public RayCaster[] rayCasters;

        public WinData win;
        private bool winTweenComplete = true;

        public Transform[] keyDots;
        private MaterialPropertyBlock[] mpB;
        public List<SpriteRenderer> dotList;
        private Renderer[] rend;

        public Sprite dotSprite;
        public Material dotMaterial;
        private int dotSortingLayerID = 0;
        public int dotSortingOrder;

        public float dotDistance = 2f;
        public float burnTime =1f;
        public int burnSpeed = 4;

        private bool isSelected = false;
        public bool IsSelected
        {
            get {return isSelected;}
            private set {isSelected = value;}
        }

        public bool IsWinningLine
        {
            get { return win!=null; }
        }

        /// <summary>
        /// Get spins won
        /// </summary>
        public int WonSpins
        {
            get
            {
                return (win == null) ? 0 : win.FreeSpins;
            }
        }

        /// <summary>
        /// Get coins won
        /// </summary>
        internal int WonCoins
        {
            get
            {
                return (win == null) ? 0 : win.Pay;
            }
        }

        /// <summary>
        /// Return true if is won tween complete
        /// </summary>
        internal bool IsWinTweenComplete
        {
            get { return winTweenComplete; }
        }

        void Start()
        {
            //1)
            SpriteRenderer sr;
            foreach (var item in keyDots)
            {
                sr = item.GetComponent<SpriteRenderer>();
                if(sr)  sr.material = dotMaterial;
            }
           dotList = CreateDotLine(dotSprite, dotMaterial, dotSortingLayerID, dotSortingOrder, dotDistance, false);

            //2) cache data 
            if (dotList != null && dotList.Count > 0)
            {
                rend = new Renderer[dotList.Count];
                mpB = new MaterialPropertyBlock[dotList.Count];
                for (int i = 0; i < dotList.Count; i++)
                {
                    rend[i] = dotList[i];
                    MaterialPropertyBlock mP = new MaterialPropertyBlock();
                    mP.Clear();
                    rend[i].GetPropertyBlock(mP);
                    mpB[i] = mP;
                }
            }
            win = null;
        }

        /// <summary>
        /// Select line
        /// </summary>
        public void Select(bool setVisible, float burnDelay)
        {
            IsSelected = true;
            LineBurn(true, burnDelay, null);
        }

        /// <summary>
        /// Deselect line
        /// </summary>
        public void DeSelect()
        {
          //  SetLineVisible(false);
            IsSelected = false;
            LineBurn(false, 0, null);
        }

        #region dotline
        /// <summary>
        /// Create dotline for keyDots array
        /// </summary>
        private List<SpriteRenderer> CreateDotLine(Sprite sprite, Material material, int sortingLayerID, int sortingOrder, float distance, bool setActive)
        {
            if (keyDots == null || keyDots.Length < 2) return null;
            List<SpriteRenderer> dList = new List<SpriteRenderer>();
            for (int i = 0; i < keyDots.Length - 1; i++)
            {
                CreateDotLine(ref dList, sprite, material, keyDots[i], keyDots[i + 1],  sortingLayerID,  sortingOrder, distance, false, false, true);
            }
            if(dList!=null)
            dList.ForEach((r)=> {if(r!=null) r.gameObject.SetActive(setActive); });
            return dList;
        }

        /// <summary>
        /// Create dotLine tile between two points
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="dist"></param>
        /// <param name="createStartPoint"></param>
        /// <param name="createEndPoint"></param>
        private void CreateDotLine(ref List<SpriteRenderer> dList, Sprite sprite, Material material, Transform start, Transform end, int sortingLayerID, int sortingOrder, float dist, bool createStartPoint, bool createEndPoint, bool addExisting)
        {
            Vector3 dir = end.position - start.position;
            float seLength = dir.magnitude;
            SpriteRenderer srStart = start.GetComponent<SpriteRenderer>();
            SpriteRenderer srEnd = end.GetComponent<SpriteRenderer>();

            if (addExisting && srStart)
            {
                srStart.sortingLayerID = sortingLayerID;
                srStart.sortingOrder = sortingOrder;
             if(!dList.Contains(srStart))   dList.Add(srStart);
            }
            if (createStartPoint) dList.Add(Creator.CreateSpriteAtPosition(transform, sprite, material, start.position, sortingLayerID, sortingOrder));

            if (seLength == 0) return;

            Vector3 dirOne = dir / seLength;
            float countf = (dist < seLength) ? seLength / dist + 1f : 2f;
            float count = Mathf.RoundToInt(countf);

            for (int i = 1; i < count - 1; i++)
            {
                dList.Add(Creator.CreateSpriteAtPosition(transform, sprite, material, start.position + dirOne * ((float)i * seLength / (count - 1f)), sortingLayerID, sortingOrder));
            }

            if (createEndPoint)
            {
                dList.Add(Creator.CreateSpriteAtPosition(transform, sprite, material, end.position, sortingLayerID, sortingOrder));
            }

            if (addExisting && srEnd)
            {
                srEnd.sortingLayerID = sortingLayerID;
                srEnd.sortingOrder = sortingOrder;
                if(!dList.Contains(srEnd)) dList.Add(srEnd);
            }
        }
        #endregion dotline

        /// <summary>
        /// Enable or disable the flashing material
        /// </summary>
        internal void LineFlashing(bool flashing)
        {
            if (mpB == null || mpB.Length == 0) return;
            if (flashing)
            {
                for (int i = 0; i < mpB.Length; i++)
                {
                    mpB[i].SetFloat("_FadeEnable", 1);
                }

            }
            else
            {
                for (int i = 0; i < mpB.Length; i++)
                {
                    mpB[i].SetFloat("_FadeEnable", 0);
                }
            }

            for (int i = 0; i < mpB.Length; i++)
            {
                rend[i].SetPropertyBlock(mpB[i]);
            }
        }

        private IEnumerator LineBurnC(int dotCount, float burnDelay, Action completeCallBack)
        {
            if (IsSelected)
            {
                int p = 0;
                bool a;
                WaitForEndOfFrame wfef = new WaitForEndOfFrame();
                for (int c = 0; c < 2; c++)
                {
                    if (!IsSelected) break;
                    for (int i = 0; i < dotList.Count + dotCount; i += dotCount)
                    {
                        if (!IsSelected) break;
                        for (int j = 0; j < dotCount; j++)
                        {
                            if ((p = i + j) >= dotList.Count) break;
                            a = dotList[p].gameObject.activeSelf;
                            dotList[p].gameObject.SetActive(!a);
                        }
                        if (p >= dotList.Count) break;
                        yield return wfef;
                    }
                    yield return new WaitForSeconds(1.5f);
                }
                if (completeCallBack != null) completeCallBack();
            }
        }

        internal void LineBurn(bool burn, float burnDelay, Action completeCallBack)
        {
            StopCoroutine("LineBurnC");
            SetLineVisible(false);
            if(burn)
                StartCoroutine(LineBurnC(3, burnDelay, completeCallBack));
        }

        /// <summary>
        /// Enable or disable line elemnts.
        /// </summary>
        internal void SetLineVisible(bool visible)
        {
            if (dotList == null) return;
            foreach (var item in dotList)
                item.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Set Order for line spite rendrer.
        /// </summary>
        private void SetLineRenderOrder(int order)
        {
            foreach (var item in dotList)
                item.sortingOrder = order;
        }

        /// <summary>
        /// Find  and fill winning symbols list  from left to right, according pay lines
        /// </summary>
        internal void FindWin(List<PayLine> payTable)
        {
            win = null;
            foreach (var item in payTable)
            {
                win = GetPayLineWin(item);
                if (win!=null)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Check if line is wonn, according payline
        /// </summary>
        /// <param name="payLine"></param>
        /// <returns></returns>
        private WinData GetPayLineWin(PayLine payLine)
        {
            if(payLine == null || payLine.line.Length != rayCasters.Length) return null;
            List<SlotSymbol> winnSymbols = new List<SlotSymbol>();
            SlotSymbol s;
            for (int i = 0; i < rayCasters.Length; i++)
            {
                s = rayCasters[i].GetSymbol();
                if (payLine.line[i] >= 0 && s.iconID != payLine.line[i])
                {
                    return null;
                }
                else if (payLine.line[i] >= 0 && s.iconID == payLine.line[i])
                {
                    winnSymbols.Add(s);
                }
            }
            return new WinData(winnSymbols, payLine.freeSpins, payLine.pay);
        }

        /// <summary>
        /// Reset old winnig data 
        /// </summary>
        internal void ResetLineWinning()
        {
            win = null;
        }

        /// <summary>
        /// Instantiate particles for each winning symbol
        /// </summary>
        internal void ShowWinSymbolsParticles(bool activate)
        {
            if (IsWinningLine)
            {
                win.Symbols.ForEach((wS) => { wS.ShowParticles(activate, SlotController.Instance.particlesStars); });
            }
        }

        /// <summary>
        /// Instantiate jump clone for each symbol
        /// </summary>
        internal void LineWinJumps(float delay, Transform topJumpTarget, Transform bottomJumpTarget, Action<int, int> comleteCallBack )
        {
            winTweenComplete = false;
            if (win == null || win.Symbols == null)
            {
                if (comleteCallBack != null) comleteCallBack(0, 0);
                return;
            }

            ParallelTween pt = new ParallelTween();

            foreach (SlotSymbol s in win.Symbols)
            {
                pt.Add((callBack) =>
                {
                    s.WinJump(callBack, topJumpTarget, bottomJumpTarget, 0.0f);
                });
            }
            pt.Start(() =>
            {
                winTweenComplete = true;
                if (comleteCallBack != null) comleteCallBack(WonSpins, WonCoins);
            });
        }

        /// <summary>
        /// Instantiate jump clone for each symbol
        /// </summary>
        internal void LineWinZoom(float delay, int count, Action<int, int> comleteCallBack)
        {
            winTweenComplete = false;
            if(win==null || win.Symbols==null)
            {
                if (comleteCallBack != null) comleteCallBack(0, 0);
                return;
            }
            ParallelTween pt = new ParallelTween();

            foreach (SlotSymbol s in win.Symbols)
            {
                pt.Add((callBack) =>
                {
                    s.WinZoom(callBack, 0.0f, count);
                });
            }
            pt.Start(() =>
            {
                winTweenComplete = true;
                if (comleteCallBack != null) comleteCallBack(WonSpins, WonCoins);
            });
        }

        internal void LineWinZoomCancel()
        {
            if(win!=null && win.Symbols!=null)
            win.Symbols.ForEach((ws)=> { ws.WinZoomCancel(); });
        }

        internal void LineWinLightCancel()
        {
            if (win != null && win.Symbols != null)
                win.Symbols.ForEach((ws) => { ws.WinLightCancel(); });
        }

        /// <summary>
        /// Instantiate jump clone for each symbol
        /// </summary>
        internal void LineWinLight(float delay, Action<int, int> comleteCallBack)
        {
            winTweenComplete = false;
            ParallelTween pt = new ParallelTween();

            foreach (SlotSymbol s in win.Symbols)
            {
                pt.Add((callBack) =>
                {
                    s.WinLight(callBack, 0.0f, 5);
                });
            }
            pt.Start(() =>
            {
                winTweenComplete = true;
                if (comleteCallBack != null) comleteCallBack(WonSpins, WonCoins);
            });
        }

    }

    public class WinData
    {
        List<SlotSymbol> symbols;
        int freeSpins = 0;
        int pay = 0;
        public int Pay
        {
            get { return pay; }
        }

        public int FreeSpins
        {
            get { return freeSpins; }
        }

        public List<SlotSymbol> Symbols
        {
            get { return symbols; }
        }

        public WinData(List<SlotSymbol> symbols, int freeSpins, int pay)
        {
            this.symbols = symbols;
            this.freeSpins = freeSpins;
            this.pay = pay;
        }
    }
}
