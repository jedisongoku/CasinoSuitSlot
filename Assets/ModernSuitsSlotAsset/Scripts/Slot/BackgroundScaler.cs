using UnityEngine;

[ExecuteInEditMode]
public class BackgroundScaler : MonoBehaviour {
    [SerializeField]
    private float width;
    [SerializeField]
    private float height;
    [SerializeField]
    private float baseRatio = 0.75f;
    [SerializeField]
    private float currScrRatio;

    public bool useBaseScale = true;
   // [HideInInspector]
    [SerializeField]
    private bool useBaseScaleOld = true;
    void Start()
    {
        if (useBaseScale)
            BaseScaleBkg();
        else
            ScaleBkg();
    }

    void Update()
    {
        if (width != Screen.width || height != Screen.height || useBaseScale!=useBaseScaleOld)
        {
            useBaseScaleOld = useBaseScale;

            if(useBaseScale)
                BaseScaleBkg();
            else
                ScaleBkg();
        }
    }

    void BaseScaleBkg()
    {
        width = Screen.width; height = Screen.height;
        currScrRatio = height / width;
        float k = 1f;
        if (baseRatio > currScrRatio)
        {
            k = baseRatio / currScrRatio;
        }
        gameObject.transform.localScale = new Vector3(k, k, k);
    }

    void ScaleBkg()
    {
        width = Screen.width; height = Screen.height;
        currScrRatio = height / width;
        float k = 1f;
       k = (currScrRatio <= 0.75f) ? -0.8f * currScrRatio + 1.6f : -0.46f * currScrRatio + 1.26f; 
        gameObject.transform.localScale = new Vector3(k, k, k);
    }

}
