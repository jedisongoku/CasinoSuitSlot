using System;
using System.Collections;
using UnityEngine;

namespace Mkey
{
    public class SessionTimer
    {
        private float initIime;
        private float lastTime = 0;
        private float passedTime = 0;
        private float passedTimeOld = 0;
        private bool pause = false;

        public Action <float> OnTickPassedSeconds;
        public Action<float> OnTickRestSeconds;
        public Action<int, int, int, float> OnTickPassedDaysHourMinSec;
        public Action<int ,int, int, float> OnTickRestDaysHourMinSec;

        public Action OnTimePassed;

        public bool IsTimePassed
        {
            get { return passedTime >= InitTime; }
        }

        public void PassedTime (out int days,  out int hours, out int minutes, out float seconds)
        {
            days = (int)(passedTimeOld / (86400f));
            float rest = passedTimeOld - days * 86400f;

            hours =(int) (rest / (3600f));
            rest = rest - hours * 3600f;

            minutes = (int) (rest / 60f);
            rest = rest - minutes * 60f;

            seconds = rest;
        }

        public void RestTime(out int days, out int hours, out int minutes, out float seconds)
        {
            float restTime = initIime - passedTimeOld;

            days = (int)(restTime / (86400f));
            float rest = restTime - days * 86400f;

            hours = (int)(rest / (3600f));
            rest = rest - hours * 3600f;

            minutes = (int)(rest / 60f);
            rest = rest - minutes * 60f;

            seconds = rest;
        }

        public float InitTime
        {
            get { return initIime; }
        }

        public SessionTimer(float secondsSpan)
        {
            initIime = secondsSpan;
            pause = true;
        }

        public SessionTimer(float minutesSpan, float secondsSpan)
        {
            initIime = minutesSpan*60f + secondsSpan;
            pause = true;
        }

        public SessionTimer(float hoursSpan,  float minutesSpan, float secondsSpan)
        {
            initIime = hoursSpan * 3600f  +  minutesSpan * 60f + secondsSpan;
            pause = true;
        }

        public SessionTimer(float daySpan, float hoursSpan, float minutesSpan, float secondsSpan)
        {
            initIime = daySpan * 24f * 3600f + hoursSpan * 3600f + minutesSpan * 60f + secondsSpan;
            pause = true;
        }

        public void Start()
        {
            if (IsTimePassed) return;
            pause = false;
        }

        public void Pause()
        {
            pause = true;
        }

        public void Restart()
        {
            passedTime = 0;
            passedTimeOld = 0;
            pause = false;
        }

        /// <summary>
        /// for timer update set Time.time param
        /// </summary>
        /// <param name="time"></param>
        public void Update(float gameTime)
        {
            float dTime = gameTime - lastTime;
            lastTime = gameTime;
            if (pause) return;

            passedTime += dTime;
            if (IsTimePassed && !pause)
            {
                pause = true;
                if (OnTimePassed != null) OnTimePassed();
            }


            if(passedTime - passedTimeOld >= 1.0f)
            {
                passedTimeOld = Mathf.Floor(passedTime);
                if (OnTickPassedSeconds != null)
                {
                    OnTickPassedSeconds(passedTimeOld);
                }
                if (OnTickRestSeconds != null)
                {
                    OnTickRestSeconds(initIime - passedTimeOld);
                }
                if (OnTickPassedDaysHourMinSec != null)
                {
                    int d =0;
                    int h = 0;
                    int m = 0;
                    float s = 0;
                    PassedTime(out d, out h, out m, out s);
                    OnTickPassedDaysHourMinSec(d, h,m, s);
                }
                if (OnTickRestDaysHourMinSec != null)
                {
                    int d = 0;
                    int h = 0;
                    int m = 0;
                    float s = 0;
                    RestTime(out d, out h, out m, out s);
                    OnTickRestDaysHourMinSec(d, h, m, s);
                }
            }
        }
    }

    public class GlobalTimer
    {
        private double initTime;
        private bool cancel = false;
        private string name = "timer_default";

        private DateTime startDT;
        private DateTime lastDT;
        private DateTime endDt ;
        private DateTime currentDT;

        string lastTickSaveKey;
        string startTickSaveKey;
        string initTimeSaveKey;

        public Action<double> OnTickPassedSeconds;
        public Action<double> OnTickRestSeconds;
        public Action<int, int, int, float> OnTickPassedDaysHourMinSec;
        public Action<int, int, int, float> OnTickRestDaysHourMinSec;
        public Action OnTimePassed;

        public bool IsTimePassed
        {
            get { return cancel; }
        }

        /// <summary>
        /// Returns the elapsed time from the beginning
        /// </summary>
        /// <param name="days"></param>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        public void PassedTime( out int days, out int hours, out int minutes, out float seconds)
        {
            TimeSpan passedTime = (!cancel) ? lastDT - startDT : endDt - startDT ;
            days = passedTime.Days;
            hours = passedTime.Hours;
            minutes = passedTime.Minutes;
            seconds =passedTime.Seconds + Mathf.RoundToInt(passedTime.Milliseconds * 0.001f);
        }

        /// <summary>
        /// Returns the remaining time to the end
        /// </summary>
        /// <param name="days"></param>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        public void RestTime(out int days, out int hours, out int minutes, out float seconds)
        {
            TimeSpan restTime = (!cancel) ? endDt - lastDT : endDt - endDt;
            days = restTime.Days;
            hours = restTime.Hours;
            minutes = restTime.Minutes;
            seconds =restTime.Seconds + Mathf.RoundToInt(restTime.Milliseconds*0.001f);
        }

        public double InitTime
        {
            get { return initTime; }
        }

        public GlobalTimer(string timerName, float daySpan, float hoursSpan, float minutesSpan, float secondsSpan, bool removeOld)
        {
            name = timerName;
            lastTickSaveKey = name + "_lastTick";
            startTickSaveKey = name + "_startTick";
            initTimeSaveKey = name + "_initTime";

            if (removeOld)
            {
                if (PlayerPrefs.HasKey(name))
                {
                    PlayerPrefs.DeleteKey(name);
                }
                if (PlayerPrefs.HasKey(lastTickSaveKey))
                {
                    PlayerPrefs.DeleteKey(lastTickSaveKey);
                }
                if (PlayerPrefs.HasKey(initTimeSaveKey))
                {
                    PlayerPrefs.DeleteKey(initTimeSaveKey);
                }
                if (PlayerPrefs.HasKey(startTickSaveKey))
                {
                    PlayerPrefs.DeleteKey(startTickSaveKey);
                }

                initTime = daySpan * 24.0 * 3600.0 + hoursSpan * 3600.0 + minutesSpan * 60.0 + secondsSpan;
                PlayerPrefs.SetString(initTimeSaveKey, initTime.ToString());
                PlayerPrefs.SetString(name, name);
                Debug.Log("-------------------------remove old " + name + "-------------------------------");
            }
            else // continue
            {
                if (PlayerPrefs.HasKey(name) && PlayerPrefs.HasKey(lastTickSaveKey) && PlayerPrefs.HasKey(initTimeSaveKey) && PlayerPrefs.HasKey(startTickSaveKey))
                {
                    startDT = DTFromSring(PlayerPrefs.GetString(startTickSaveKey));
                    lastDT = DTFromSring(PlayerPrefs.GetString(lastTickSaveKey));
                    initTime = daySpan * 24.0 * 3600.0 + hoursSpan * 3600.0 + minutesSpan * 60.0 + secondsSpan;
                    if (initTime == 0)
                        if (!double.TryParse(PlayerPrefs.GetString(initTimeSaveKey), out initTime))
                        {
                            Debug.Log("try parse error");
                            initTime = 0;
                        }
                    endDt = startDT.AddSeconds(initTime);
                    startTickCreated = true;
                    lastTickCreated = true;
                    Debug.Log("--------------------continue " +name+ " ------------------------");
                }
                else //if data lost
                {
                    initTime = daySpan * 24.0 * 3600.0 + hoursSpan * 3600.0 + minutesSpan * 60.0 + secondsSpan;
                    PlayerPrefs.SetString(initTimeSaveKey, initTime.ToString());
                    PlayerPrefs.SetString(name, name);
                    Debug.Log("--------------------old lost " + name + "------------------------");
                }
            }
            Debug.Log(initTime);
        }

        bool startTickCreated = false;
        bool lastTickCreated = false;
    

        /// <summary>
        /// Timer update.
        /// </summary>
        /// <param name="time"></param>
        public void Update()
        {
            if (cancel) return;
            currentDT = DateTime.Now;

            if (!startTickCreated)
            {
                startDT = currentDT;
                PlayerPrefs.SetString(startTickSaveKey, currentDT.ToString());
                startTickCreated = true;
                endDt = startDT.AddSeconds(initTime); // Debug.Log( "strtTick: "+startTick);
            }
            if (!lastTickCreated)
            {
                lastDT = currentDT;
                PlayerPrefs.SetString(lastTickSaveKey, currentDT.ToString());
                lastTickCreated = true;//  Debug.Log("lastTick: "+lastTick);
            }

            double dTime = (currentDT - startDT).TotalSeconds;
            double passedSeconds = dTime;
         
            if (dTime>=initTime && !cancel)
            {
                cancel = true;
                if (OnTimePassed != null) OnTimePassed();
                passedSeconds = initTime;
            }

            if ((currentDT-lastDT).TotalSeconds >= 1.0 || cancel)
            {
               // Debug.Log("dTime: " + dTime +" current: "+ currentDT.ToString() + " last: " + lastDT.ToString());
                lastDT = currentDT;
                PlayerPrefs.SetString(lastTickSaveKey, currentDT.ToString());

                if (OnTickPassedSeconds != null)
                {
                    OnTickPassedSeconds(passedSeconds);
                }
                if (OnTickRestSeconds != null)
                {
                    OnTickRestSeconds(initTime - passedSeconds);
                }
                if (OnTickPassedDaysHourMinSec != null)
                {
                    int d = 0;
                    int h = 0;
                    int m = 0;
                    float s = 0;
                    PassedTime(out d, out h, out m, out s);
                    OnTickPassedDaysHourMinSec(d, h, m, s);
                }
                if (OnTickRestDaysHourMinSec != null)
                {
                    int d = 0;
                    int h = 0;
                    int m = 0;
                    float s = 0;
                    RestTime(out d, out h, out m, out s);
                    OnTickRestDaysHourMinSec(d, h, m, s);
                }
            }
        }

        /// <summary>
        /// Restart new timer cycle
        /// </summary>
        public void Restart()
        {
            startTickCreated = false;
            lastTickCreated = false;
            cancel = false;
            Debug.Log("restart");
        }

        private DateTime DTFromSring(string dtString)
        {
            if (string.IsNullOrEmpty(dtString)) return DateTime.Now;
            return Convert.ToDateTime(dtString);
        }

        private double GetTimeSpanSeconds(DateTime dtStart, DateTime dtEnd)
        {
            return (dtEnd - dtStart).TotalSeconds;
        }

        private double GetTimeSpanSeconds(string dtStart, string dtEnd)
        {
            return (DTFromSring(dtEnd) - DTFromSring(dtStart)).TotalSeconds;
        }
    }

}

