using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SafeAreaUIAdapter : MonoBehaviour
{ 

    [SerializeField] private bool fixBottomMarginEnabled = false;
    [SerializeField] private float fixedBottomMargin = 0f; //Recommended 48f for iPhoneX/Max

    private RectTransform mAdaptivePanel;
    private Rect mLastSafeArea = new Rect(0,0,0,0);

    private Vector3 mInitPosition;
    private Vector2 mInitAnchorMin;
    private Vector2 mInitAnchorMax;
    private Vector2 mInitOffsetMin;
    // private float mInitMaxBottomMargin;

    
    private bool mInitialized = false;
    
    //圆角矩形屏幕底部圆弧高度参考值，超过该参考值则需要适当延展SafeArea，否则下方空白太多。
    //参考：iPhone 11/XR上bottomMargin为68，iPhone XS/Max上bottomMargin为102，因此我们暂时取该参考值为48f(如果比这个值还小我们姑且认为可以不用调整)
    private const float ROUND_BOTTOM_MARGIN_THRESHOLD = 48f; 

    void Awake(){
        mAdaptivePanel = GetComponent<RectTransform>();
    }

    void Start()
    {   
        _Init();
        _ApplySafeArea();
    }

    void _Init()
    {
        mInitAnchorMin = mAdaptivePanel.anchorMin;
        mInitAnchorMax = mAdaptivePanel.anchorMax;
        mInitOffsetMin = mAdaptivePanel.offsetMin;
        mInitialized = true;
    }

    public void UpdateSafeArea()
    {
        if(!mInitialized || null == this.gameObject || !this.gameObject.activeInHierarchy){
            Debug.LogErrorFormat(this, $"Error: UpdateSafeArea() can not be called before intialized or attached gameObject '{this.gameObject.name}' is inactive");
            return;
        }
        mAdaptivePanel.anchorMin = mInitAnchorMin;
        mAdaptivePanel.anchorMax = mInitAnchorMax;
        mAdaptivePanel.offsetMin = mInitOffsetMin;
        _ApplySafeArea();
    }

    private void _ApplySafeArea()
    {
        _FixSizeForSafeArea();   
    }

    void _FixSizeForSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        // SafeArea Rect 距离屏幕顶部的距离
        float topMargin = Screen.height - safeArea.height - safeArea.y;
        // SafeArea Rect 距离屏幕底部的距离
        float bottomMargin = safeArea.y;

        // 如果屏幕底部是圆角，要适当延展SafeArea(缩小bottomMargin的高度)，否则道具栏下面空白太多
        Vector2 bottomOffset = Vector2.zero;
        if(fixBottomMarginEnabled && bottomMargin >= ROUND_BOTTOM_MARGIN_THRESHOLD){
            bottomOffset.y = bottomMargin - fixedBottomMargin;
        }   
        
        // “-offset" 向下延伸SafeAreaRect 底部，因此bottomOffset.y越大，向下延展越多
        anchorMin -= bottomOffset;
        // “+offset” 向上延伸SafeAreaRect 顶部，因此topOffset.y越大，向上延展约多

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;


        mAdaptivePanel.anchorMin = anchorMin;
        mAdaptivePanel.anchorMax = anchorMax;

        
    }

}

