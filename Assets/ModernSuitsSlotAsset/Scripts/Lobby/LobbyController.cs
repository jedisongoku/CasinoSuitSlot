using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey {
    public class LobbyController : MonoBehaviour {


        public void SceneLoad(int scene)
        {
            SceneLoader.Instance.LoadScene(scene);
        }

        public void Slider_Click()
        {
            GuiController.Instance.ShowShop();
        }

    }
}