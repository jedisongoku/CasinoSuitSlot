// examples https://developers.facebook.com/docs/unity/examples

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;

using Facebook.Unity;
using Facebook.MiniJSON;

namespace Mkey
{
    public enum FBState { Free, Login, LoadData }
    public class FBholder : MonoBehaviour
    {
        public static FBholder Instance;
        public static FBState fbState = FBState.Free;
        public bool debugLogin = true;

        public static Action<bool, string> LoginEvent;
        public static Action<bool, bool, string> LoginPublishEvent;
        public static Action LogoutEvent;

		public string playerID;
		public string playerFirstName;
		public string playerLastName;
		public Sprite playerPhoto;

        // saves last player status, can be used to automatically log in (if (LastSessionLogined) FBlogin())
        public static bool LastSessionLogined
        {
            get
            {
                if (!PlayerPrefs.HasKey("_fblastlogined_"))
                {
                    PlayerPrefs.SetInt("_fblastlogined_", 0);
                }
                return PlayerPrefs.GetInt("_fblastlogined_") > 0;
            }
            set
            {
                PlayerPrefs.SetInt("_fblastlogined_", (value) ? 1 : 0);
            }
        }

        private void Awake()
        {
            if (Instance) Destroy(gameObject);
            else Instance = this;
            Initialize();
        }

        private void Start()
        {
           // if (LastSessionLogined) FBlogin();
        }

        #region init
        public void Initialize()
        {
            Debug.Log("FB Initialize");
            if (!FB.IsInitialized)
            {
                FB.Init(() =>
                {
                    if (FB.IsInitialized)
                    {
                        Debug.Log("Facebook SDK is initialized");
                        FB.ActivateApp(); //Signal an app activation App Event
                    }
                    else
                    {
                        Debug.Log("Failed to Initialize Facebook SDK");
                    }

                }, (isUnityShown) =>
                {
                    if (!isUnityShown)
                    {
                        Time.timeScale = 0;// Pause the game - we will need to hide
                    }
                    else
                    {
                        Time.timeScale = 1;// Resume the game - we're getting focus again
                    }

                });
            }
            else
                FB.ActivateApp(); // Already initialized, signal an app activation App Event
        }

        #endregion init

        #region login
        List<string> permissions;
        int loginTryCount = 10;
        public void FBlogin()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("Error. Check internet connection!");
                return;
            }
            fbState = FBState.Login;
            permissions = new List<string>();
            permissions.Add("public_profile");
            permissions.Add("email");
            permissions.Add("user_friends");
            FB.LogInWithReadPermissions(permissions, (result) =>
            {
                fbState = FBState.Free;
                if (FB.IsLoggedIn)
                {
                    playerID = null;
                    playerFirstName = null;
                    playerLastName = null;
                    playerPhoto = null;

                    if(debugLogin) Debug.Log("facebook is logged in, app token :" + AccessToken.CurrentAccessToken.TokenString);
                    LastSessionLogined = true;
                    LoadAllFBData();
                }
                else
                {
                    Debug.Log("facebook is not logged in, loginTryCount : " + loginTryCount);
                    if (result.Error != null)
                    {
                        Debug.Log(result.Error);
                    }
                    if (loginTryCount-- > 0)
                    {
                        FBlogin(); // try next login
                }
                    else
                    {
                        loginTryCount = 10;
                    }
                }

                if (LoginEvent != null) LoginEvent(IsLogined, result.Error);
            });
        }

        public void FBloginWithPublish()
        {
            FBloginWithPublish(null);
        }

        public void FBloginWithPublish(Action logInCallBack)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("Error. Check internet connection!");
                return;
            }
            fbState = FBState.Login;
            permissions = new List<string>();
            permissions.Add("publish_actions");
            FB.LogInWithPublishPermissions(permissions, (result) =>
            {
                fbState = FBState.Free;
                if (result.Error == null)
                {
                    if (HavePublishActions)
                    {
                        Debug.Log("Logined with publish permissions");

                    }
                    else
                    {
                        Debug.Log("Publish permissions no granted.");
                    }
                }
                else
                {
                    Debug.Log("ERROR. Login with publish permissions, error: " + result.Error);
                }
                if (LoginPublishEvent != null) LoginPublishEvent(IsLogined, HavePublishActions, result.Error);
                if (logInCallBack != null) logInCallBack();
            });
        }

        public void FBlogOut()
        {
            FB.LogOut();
            LastSessionLogined = false;
            if (LogoutEvent != null) LogoutEvent();
            StartCoroutine(WaitLogOut(() =>
            {
                Debug.Log("IsLogined: " + IsLogined);
            }));
        }

        public void FBlogOut(Action logOutCallBack)
        {
            FB.LogOut();
            LastSessionLogined = false;
            StartCoroutine(WaitLogOut(() =>
            {
                Debug.Log("IsLogined: " + IsLogined);
            }));

        }

        IEnumerator WaitLogOut(Action logOutCallBack)
        {
            while (IsLogined)
                yield return null;
            if (logOutCallBack != null) logOutCallBack();
        }

        public static bool IsLogined
        {
            get { return FB.IsLoggedIn; }
        }

        /// <summary>
        /// Run sequence to load user profile, apprequest, friends profiles, invitable friends profiles 
        /// </summary>
        private void LoadAllFBData()
        {
            fbState = FBState.LoadData;
            TweenSeq tS = new TweenSeq();

            tS.Add((callBack) =>
            {
                GetPlayerTextInfo(callBack);
            });

            tS.Add((callBack) =>
            {
                GetPlayerPhoto(callBack);
            });

            tS.Add((callBack) =>
            {
                fbState = FBState.Free;
                if (callBack != null) callBack();
            });

            tS.Start();
        }

        #endregion login

        #region player info
        int getPlayerInfoTryCount = 10;

        /// <summary>
        /// Fetch player first name, last name and id, with try count = getPlayerInfoTryCount 
        /// </summary>
        public void GetPlayerTextInfo(Action completeCallBack)
        {
            TweenSeq tS = new TweenSeq();
            for (int i = 0; i < getPlayerInfoTryCount; i++)
            {
                tS.Add((callBack) =>
                {
                    TryGetPlayerTextInfo(callBack);
                });
            }

            tS.Add((callBack) =>
            {
                if (completeCallBack != null) completeCallBack();
            });
            tS.Start();
        }

        /// <summary>
        /// Fetch player first name, id and photo
        /// </summary>
        public void TryGetPlayerTextInfo(Action completeCallBack)
        {
			if (string.IsNullOrEmpty(playerID))
            {
                if (debugLogin) Debug.Log("Try to get player text info...");
                FB.API("/me?fields=first_name,last_name,id,email", HttpMethod.GET,
                    (result) =>
                    {
                        if (result.Error != null)
                        {
                            Debug.Log(result.Error);
                        }
                        else
                        {
                            playerFirstName = (string)result.ResultDictionary["first_name"];
                            playerLastName = (string)result.ResultDictionary["last_name"];
                            playerID = (string)result.ResultDictionary["id"];
                            if (debugLogin)  Debug.Log("Player text info received. PlayerName: " + playerFirstName + " " + playerLastName + " ; playerID: " + playerID);
                       
						}
                        if (completeCallBack != null) completeCallBack();
                    });
            }
            else
            {
                if (completeCallBack != null) completeCallBack();
            }
        }

        /// <summary>
        /// Fetch player first name, id and photo
        /// </summary>
        public void GetPlayerPhoto(Action completeCallBack)
        {
			if (string.IsNullOrEmpty(playerID))
            {
                if (completeCallBack != null) completeCallBack();
                return;
            }

            TweenSeq tS = new TweenSeq();
            for (int i = 0; i < getPlayerInfoTryCount; i++)
            {
                tS.Add((callBack) =>
                {
                    TryGetPlayerPhoto(callBack);
                });
            }

            tS.Add((callBack) =>
            {
                if (completeCallBack != null) completeCallBack();
            });
            tS.Start();
        }

        /// <summary>
        /// Fetch player first name, id and photo
        /// </summary>
        public void TryGetPlayerPhoto(Action completeCallBack)
        {
			if (playerPhoto==null)
            {
                if (debugLogin) Debug.Log("Try to get player photo...");
                FB.API("/me/picture?type=square&height=128&width=128", HttpMethod.GET, (result) =>
                {
                    if (result.Texture != null)
                    {
                        if (debugLogin) Debug.Log("Player photo received..");
							playerPhoto = Sprite.Create(result.Texture, new Rect(0,0, result.Texture.width, result.Texture.height), new Vector2(0.5f,0.5f));
                    }
                    else
                    {
                        Debug.Log("NO player photo, new query: ....");
                    }
                    if (completeCallBack != null) completeCallBack();
                });
            }
            else
            {
                if (completeCallBack != null) completeCallBack();
            }
        }

        /// <summary>
        /// Post player score using FB scores api with publish_actions
        /// </summary>
        public void PostScore(int score)
        {
            if (HavePublishActions)
            {
                var scoreData = new Dictionary<string, string>();
                scoreData["score"] = score.ToString();
                FB.API("/me/scores", HttpMethod.POST, delegate (IGraphResult result)
                {
                    Debug.Log("Score submit result: " + result.RawResult);
                }, scoreData);
            }

            else
            {
                if (IsLogined)
                {
                    Debug.Log("Try to get publish permission...");
                    FBloginWithPublish(() => { PostScore(score); });
                }
                else
                {
                    Debug.Log("Login to FaceBook to post your score");
                   // GUIController.Instance.sh("Not logined!!!", "Login to FaceBook to post your score on Facebook", 3, null);
                }
            }

        }
        #endregion player info

        #region app link
        public void GetAppLink()
        {
            if (Constants.IsMobile)
            {
                FB.Mobile.FetchDeferredAppLinkData(this.GetAppLinkCallBack);
                return;
            }
            FB.GetAppLink(this.GetAppLinkCallBack);
        }

        protected void GetAppLinkCallBack(IResult result)
        {
            string LastResponse = string.Empty;
            string Status = string.Empty;
            if (result == null)
            {
                LastResponse = "Null Response\n";
                Debug.Log(LastResponse);
                return;
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                Status = "Error - Check log for details";
                LastResponse = "Error Response:\n" + result.Error;
            }
            else if (result.Cancelled)
            {
                Status = "Cancelled - Check log for details";
                LastResponse = "Cancelled Response:\n" + result.RawResult;
            }
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
                Status = "Success - Check log for details";
                LastResponse = "Success Response:\n" + result.RawResult;
            }
            else
            {
                LastResponse = "Empty Response\n";
            }

            Debug.Log(result.ToString());
        }
        #endregion app link

        #region share
        /// <summary>
        /// Create screen shot and post to FB with publish actions
        /// Take and publish a screenshot (requires publish_actions), requires version 4.3.3 or later:
        /// Publish a user's score in your game (requires the publish_actions permission):
        /// https://developers.facebook.com/docs/unity/reference/current/FB.API
        /// </summary>
        /// <param name="shareScreenCallBack"></param>
        public void ShareScreenShot(Action<bool> resultCallBack)
        {
            if (!HavePublishActions)
            {
                if (IsLogined)
                {
                    GuiController.Instance.ShowMessage("Message", "Try to publish screen.", 3, null);
                    FBloginWithPublish(() =>
                    {
                        StartCoroutine(TryPostScreenshot(resultCallBack));
                        if (!HavePublishActions)
                        {
                            GuiController.Instance.ShowMessage("Sorry!!!", "Not have a publish permissions to post your screen.", 3, null);
                        }
                    });
                }
                else
                {
                    GuiController.Instance.ShowMessage("Not logined!!!", "Login to FaceBook to post your screen.", 3, null);
                }
            }
            else
            {
                StartCoroutine(TryPostScreenshot(resultCallBack));
            }
        }

        /// <summary>
        /// Make screen shot and send to facebook
        /// Work!!! (addit refs: https://answers.unity.com/questions/1001100/post-screenshot-photo-on-facebook.html, http://www.thegamecontriver.com/2015/09/unity-share-post-image-to-facebook.html, https://stackoverflow.com/questions/43908776/unity-sharing-screenshot-with-text-to-facebook)
        /// </summary>
        /// <returns></returns>
        public IEnumerator TryPostScreenshot(Action<bool> resultCallBack)
        {
            yield return new WaitForEndOfFrame();
            var width = Screen.width;
            var height = Screen.height;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            TryPostPhoto("Try Snow Game !", tex, resultCallBack);
        }

        private void TryPostPhoto(string message, Texture2D photo, Action<bool> resultCallBack)
        {
            byte[] image = photo.EncodeToPNG();
            var wwwForm = new WWWForm();
            wwwForm.AddBinaryData("image", image, "screen.png");
            wwwForm.AddField("message", message);
            FB.API("me/photos", HttpMethod.POST, (result) =>
            {
                if (result.Error == null)
                {
                    Dictionary<string, object> reqResult = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
                    string id = reqResult["id"] as string;
                    string post_id = reqResult["post_id"] as string;
                }
                if (resultCallBack != null) resultCallBack(result.Error == null);
            }, wwwForm);
        }
        #endregion share

        /// <summary>
        /// Helper function to check whether the player has granted 'publish_actions'
        /// </summary>
        public static bool HavePublishActions
        {
            get
            {
                return (FB.IsLoggedIn &&
                       (AccessToken.CurrentAccessToken.Permissions as List<string>).Contains("publish_actions")) ? true : false;
            }
        }
    }
}