using UnityEngine;
using System;
namespace Mkey
{

    public class SlotSymbol : MonoBehaviour
    {
        public int iconID;
        [SerializeField]
        private int position;
        private GameObject particles;
        private Sprite winLightSprite;
        private GameObject winLightGO;
        private SpriteRenderer sR;


        public int Position
        {
            get { return position; }
            set { position = value; }
        }

        internal void SetIcon(SlotIcon icon, int iconID)
        {
            this.iconID = iconID;
            sR = GetComponent<SpriteRenderer>();
            sR.sprite = icon.iconSprite;
            winLightSprite = icon.addIconSprite;
        }

        internal void ShowParticles(bool activity, GameObject particlesPrefab)
        {
            if (activity)
            {
                if (particlesPrefab)
                {
                    if (particles == null)
                    {
                        particles = (GameObject)Instantiate(particlesPrefab, transform.position, transform.rotation);
                        particles.transform.parent = transform.parent;
                        particles.transform.localScale = transform.localScale;
                    }
                }
            }
            else
            {
                if (particles) { Destroy(particles); }
            }
        }

        GameObject tweenClone;
        internal void WinJump(Action completeCallBack, Transform firstPos, Transform secPos, float delay)
        {
            TweenSeq tS = new TweenSeq();
            // 0 create clone
            GameObject tweenClone = CreateJumpClone();

            // 1 scale clone
            tS.Add((callBack) =>
            {
                SimpleTween.Value(tweenClone, transform.localScale.x, transform.localScale.x * 2f, 0.2f).SetOnUpdate((float val) =>
                {
                    if (!tweenClone.activeSelf)
                    {
                        tweenClone.SetActive(true);
                    }
                    tweenClone.transform.localScale = new Vector3(val, val, val);
                }).AddCompleteCallBack(() => { callBack(); }).SetDelay(delay);
                //   
            });

            // 2 jump to first position  
            tS.Add((callBack) =>
            {
                SimpleTween.Move(tweenClone, tweenClone.transform.position, firstPos.position, 0.5f).AddCompleteCallBack(() => { callBack(); }).SetEase(EaseAnim.EaseOutBounce);
            });

            //3 jump to second position 
            tS.Add((callBack) =>
            {
                SimpleTween.Move(tweenClone, tweenClone.transform.position, secPos.position, 0.5f).SetEase(EaseAnim.EaseInCirc).AddCompleteCallBack(() => { callBack(); });

                SimpleTween.Value(tweenClone, tweenClone.transform.localScale.x, 0, 0.25f).SetOnUpdate((float val) => { tweenClone.transform.localScale = new Vector3(val, val, val); }).SetDelay(0.26f).
                AddCompleteCallBack(() => { Destroy(tweenClone); if (completeCallBack != null) completeCallBack(); });
            });

            tS.Start();
        }

        TweenSeq blinkTS;
        Vector3 localScale;
        internal void WinZoom(Action completeCallBack, float delay, int count)
        {
            blinkTS = new TweenSeq();
            localScale = transform.localScale;

            for (int i = 0; i < count; i++)
            {
                // 1 scale  out
                blinkTS.Add((callBack) =>
                {
                    SimpleTween.Value(gameObject, localScale.x, localScale.x * 1.1f, 0.15f).SetOnUpdate((float val) =>
                    {
                        transform.localScale = new Vector3(val, val, 1);
                    }).AddCompleteCallBack(() => { callBack(); }).SetDelay(delay).SetEase(EaseAnim.EaseInSine);
                });

                // 2 scale  in
                blinkTS.Add((callBack) =>
                {
                    SimpleTween.Value(gameObject, localScale.x * 1.1f, localScale.x, 0.15f).SetOnUpdate((float val) =>
                    {
                        transform.localScale = new Vector3(val, val, 1);
                    }).AddCompleteCallBack(() => { callBack(); }).SetDelay(delay).SetEase(EaseAnim.EaseInSine);
                });
            }

            //3 
            blinkTS.Add((callBack) =>
            {
                transform.localScale = localScale;
                if (completeCallBack != null) completeCallBack();
                callBack();
            });

            blinkTS.Start();
        }

        TweenSeq lightTS;
        internal void WinLight(Action completeCallBack, float delay, int count)
        {
            lightTS = new TweenSeq();
            localScale = transform.localScale;
            SpriteRenderer srL = Creator.CreateSpriteAtPosition(transform.parent, winLightSprite, transform.position, sR.sortingLayerID ,sR.sortingOrder+1 );
            srL.transform.rotation = transform.rotation;
            srL.transform.localScale = transform.localScale;
            Color c = srL.color;

            winLightGO = srL.gameObject;

            for (int i = 0; i < count; i++)
            {
                // 1 scale  out
                lightTS.Add((callBack) =>
                {
                    SimpleTween.Value(gameObject, 1.0f, 0, 0.25f).SetOnUpdate((float val) =>
                    {
                        srL.color = new Color(c.r, c.g, c.b, val);
                    }).AddCompleteCallBack(() => { callBack(); }).SetDelay(delay).SetEase(EaseAnim.EaseInSine);
                });

                // 2 scale  in
                lightTS.Add((callBack) =>
                {
                    SimpleTween.Value(gameObject, 0, 1f, 0.25f).SetOnUpdate((float val) =>
                    {
                        srL.color = new Color(c.r, c.g, c.b, val);
                    }).AddCompleteCallBack(() => { callBack(); }).SetDelay(delay).SetEase(EaseAnim.EaseInSine);
                });
            }

            //3 
            lightTS.Add((callBack) =>
            {
                srL.color = c;
                Destroy(winLightGO);
                if (completeCallBack != null) completeCallBack();
                callBack();
            });

            lightTS.Start();
        }

        internal void WinZoomCancel()
        {
            Debug.Log("Blink cancel : " + iconID);
            if (blinkTS!=null) blinkTS.Break();
            SimpleTween.Cancel(gameObject, false, 1f );
        }

        internal void WinLightCancel()
        {
            Debug.Log("Light cancel : " + iconID);
            if (lightTS != null) lightTS.Break();
            SimpleTween.Cancel(gameObject, false);
            Destroy(winLightGO);
        }

        private GameObject CreateJumpClone()
        {
            SpriteRenderer sR = GetComponent<SpriteRenderer>();
            return Creator.CreateSpriteAtPosition(transform.parent, sR.sprite, transform.position, sR.sortingLayerID, sR.sortingOrder + 1).gameObject;
        }
    }
}

