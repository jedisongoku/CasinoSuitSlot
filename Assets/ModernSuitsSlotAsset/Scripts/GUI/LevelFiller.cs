using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class LevelFiller : MonoBehaviour {

    Image image;
    RectTransform rt;
	void Start () {
        image = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
	}
	
	void Update () {
        if (image && rt)
        {
            float dx = image.fillAmount;
            rt.anchoredPosition =new Vector2((dx-1)*rt.rect.width , rt.anchoredPosition.y);
        }
	}
}
