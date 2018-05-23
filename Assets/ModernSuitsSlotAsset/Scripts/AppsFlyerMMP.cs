using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppsFlyerMMP : MonoBehaviour {

    void Start()
    {
        // For detailed logging
        //AppsFlyer.setIsDebug (true);
        AppsFlyer.setAppsFlyerKey("aTYJZVwsYCTz8BbnbrDbxL");
#if UNITY_IOS
        //Mandatory - set your AppsFlyer’s Developer key.
        
        //Mandatory - set your apple app ID
        //NOTE: You should enter the number only and not the "ID" prefix
        AppsFlyer.setAppID ("YOUR_APP_ID_HERE");
        AppsFlyer.trackAppLaunch ();
#elif UNITY_ANDROID
        //Mandatory - set your Android package name
        AppsFlyer.setAppID("com.Blast.Collider.Test");
        //Mandatory - set your AppsFlyer’s Developer key.
        AppsFlyer.init("aTYJZVwsYCTz8BbnbrDbxL", "AppsFlyerTrackerCallbacks");

        AppsFlyer.setCustomerUserID("659231");

        //For getting the conversion data in Android, you need to this listener.
        //AppsFlyer.loadConversionData("AppsFlyerTrackerCallbacks");

#endif
    }


    public static void ElementsCollected(int batch)
    {
        Dictionary<string, string> elementsCollected = new Dictionary<string, string>();
        elementsCollected.Add("quantity", batch.ToString());
        AppsFlyer.trackRichEvent("elements_collected", elementsCollected);
        //AppsFlyer.loadConversionData("AppsFlyerTrackerCallbacks");

        //To get the callbacks
        //AppsFlyer.createValidateInAppListener ("AppsFlyerTrackerCallbacks", "onInAppBillingSuccess", "onInAppBillingFailure");

    }

    public static void LevelCompleted()
    {

        Dictionary<string, string> levelCompleted = new Dictionary<string, string>();
        levelCompleted.Add("quantity", "1");
        AppsFlyer.trackRichEvent("level_completed", levelCompleted);
        //AppsFlyer.loadConversionData("AppsFlyerTrackerCallbacks");

        //To get the callbacks
        //AppsFlyer.createValidateInAppListener ("AppsFlyerTrackerCallbacks", "onInAppBillingSuccess", "onInAppBillingFailure");

    }
}
