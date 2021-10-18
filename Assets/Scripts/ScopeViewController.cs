using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using NativeGalleryNamespace;
using NativeCameraNamespace;


public class ScopeViewController : MonoBehaviour
{
    [SerializeField] private RawImage m_RawImage;
    [SerializeField] private TextMeshProUGUI m_InfoText;
    [SerializeField] private Button m_CameraButton;
    [SerializeField] private Button m_GalleryButton;

    [SerializeField] private CanvasGroup m_InteractableGroup;

    private ImageRecognizer recognizer;

    private RectTransform scopeRect;
    private float scopeWidth;
    private float scopeHeight;
    private Texture defaultRawImageTexture;
    
    private static string CACHED_FILE 
    {
        set {}
        get { return Path.Combine(Application.persistentDataPath, "cachedImage.jpg"); }
    }
    
    private static readonly string CACHED_RECOGNITION_SUMMARY_KEY = "cachedRecognitionSummary";

    void Awake()
    {
        scopeRect = GetComponent<RectTransform>();
        recognizer = GetComponent<ImageRecognizer>();
        m_CameraButton.onClick.AddListener(OnCameraButtonClick);
        m_GalleryButton.onClick.AddListener(OnGalleryButtonClick);
        defaultRawImageTexture = m_RawImage.texture;
    }

    void Start() 
    {
        m_InfoText.text = "";
        StartCoroutine(initialize());
    }

    private IEnumerator initialize() 
    {
        // m_InfoText.text = "<color=green><b> 正在分析，请稍候 ... </b></color>";
        m_InfoText.text = "<color=green><b> 请拍摄或从相册选择一张显微图片... </b></color>";
        scopeWidth = scopeRect.rect.width;
        scopeHeight = scopeRect.rect.height;
        Debug.Log($"*** scopeWidth={scopeWidth}, scopeHeight={scopeHeight}");

        //load cached data
        bool cachedFileExist = File.Exists(CACHED_FILE);
        bool cachedSummaryExist = !string.IsNullOrEmpty(PlayerPrefs.GetString(CACHED_RECOGNITION_SUMMARY_KEY, null));
        if(cachedFileExist && cachedSummaryExist){
            yield return loadFromCache();
        } else {
            yield return captureNewImage();
        }
    }

    private IEnumerator captureNewImage()
    {
        // Debug.Log("*** No cached image or recognition result found, now captureNewImage()");
        // byte[] bytes = readExampleImage();
        // yield return doRecognize(bytes);
        
        yield return null;
        PickImage();
        // TakePicture();
    }

    private void resizeImageInScopeView(Texture2D texture)
    {
        m_RawImage.texture = texture;
        Vector2 fixedSizeDelta;
        if(texture.width < texture.height) {
            fixedSizeDelta = new Vector2(scopeWidth, texture.height * (scopeWidth/texture.width));
        } else {
            fixedSizeDelta = new Vector2(texture.width * (scopeHeight/texture.height), scopeHeight);
        }

        Debug.Log($"*** Caculating resize the image within ScopeView to = {fixedSizeDelta}");
        m_RawImage.GetComponent<RectTransform>().sizeDelta = fixedSizeDelta;
        // m_RawImage.GetComponent<TouchZoomMove>().Init();
    }

    private IEnumerator loadFromCache() 
    {   
        byte[] imageBytes = File.ReadAllBytes(CACHED_FILE);
        if(null == imageBytes){
            Debug.LogError("*** Null or illegal cached file casued error when loadCahce()");
            yield break;
        }
        Texture2D texture = new Texture2D(4, 3);
        texture.LoadImage(imageBytes);
        Debug.Log($"*** Cached image loaded, width={texture.width} and height={texture.height}");
        resizeImageInScopeView(texture);
        m_InfoText.text = PlayerPrefs.GetString(CACHED_RECOGNITION_SUMMARY_KEY, "");
        yield return null;  
    }

    private IEnumerator saveToCache(Texture2D texture, string recognitionSummary)
    {
        if(null == texture || string.IsNullOrEmpty(recognitionSummary)){
            Debug.Log("*** saveToCache() nothing to save. image or recognition results are null!");
            yield break;
        }
        string path = CACHED_FILE;
        byte[] bytes = texture.EncodeToJPG();
        yield return null;
        File.WriteAllBytes(path,bytes);
        Debug.Log($"*** Recognized image has been saved to {path}");
        yield return null;
        PlayerPrefs.SetString(CACHED_RECOGNITION_SUMMARY_KEY, recognitionSummary);
    }

    private byte[] readExampleImage() 
    {
        string path = Path.Combine(Application.streamingAssetsPath,"BloodImage_00001_jpg.rf.d702f2b1212a2ed897b5607804109acf.jpg");
        return File.ReadAllBytes(path);
    }

    private IEnumerator doRecognize(byte[] imageBytes)
    {
        m_InfoText.text = "<color=green><b> 正在分析，请稍候 ... </b></color>";
        m_RawImage.texture = defaultRawImageTexture;
        SetInteractable(false);
        string path = Path.Combine(Application.streamingAssetsPath,"BloodImage_00001_jpg.rf.d702f2b1212a2ed897b5607804109acf.jpg");
        yield return recognizer.asyncRecognize(imageBytes, (RecognitionResults recog)=>{

            if(null == recog){
                SetInteractable(true);
                return;
            }

            Texture2D texture = new Texture2D(4, 3);
            texture.LoadImage(imageBytes);
            resizeImageInScopeView(texture);
            StartCoroutine(drawRecognizedResults(texture,recog));
        });
    }

    private IEnumerator drawRecognizedResults(Texture2D texture, RecognitionResults recog){

        List<Label> allLabels = recog.getAllLabels();
        foreach (var label in allLabels)
        {
            texture = drawLableOutline(texture,label);
            yield return null;
            m_RawImage.texture = texture;
            yield return null;
        }
        m_InfoText.text = formatLabelStrings(recog);
        yield return saveToCache((Texture2D)m_RawImage.texture,  m_InfoText.text);
        SetInteractable(true);
    }

    private string formatLabelStrings(RecognitionResults recog) 
    {
        
        StringBuilder sb = new StringBuilder();
        int labelCount = 0;
        string colorName = "white";
        string displayName = "";

        sb.Append($"<color=green><b>共识别出 {recog.countOfAll()} 个目标：</b></color> \n");
        // sb.Append("=================================\n");
        string[] allLabelNames = recog.allLabelNames();
        foreach (var name in allLabelNames)
        {
            labelCount = recog.countOfLabel(name);
            colorName = recog.colorOfLabel(name).colorName;
            displayName = DataModel.LabelNameAliasMap.ContainsKey(name.ToLower()) ? DataModel.LabelNameAliasMap[name.ToLower()] : name; 
            sb.Append($"<color={colorName}>  *  {displayName} ({labelCount})\n");
        }
        return sb.ToString();
    }

    private Texture2D drawLableOutline(Texture2D texture2d, Label label, int borderWeight = 2) 
    {
        Location location = label.location;
        Color maskColor = label.color.colorValue;
        int WWidth = texture2d.width;
        int WHeight = texture2d.height;
        int beginX = location.left;
        int endX = beginX + location.width;
        int beginY = WHeight - location.top - location.height;
        int endY = beginY + location.height;

        int borderX1 = beginX + borderWeight;
        int borderX2 = endX - borderWeight;
        int borderY1 = beginY + borderWeight;
        int borderY2 = endY - borderWeight;

        Color32 originalColor;
        Color32 fillColor;

        for (int y = beginY; y <= endY; y++)
        {
            for (int x = beginX; x <= endX; x++)
            {
                originalColor = texture2d.GetPixel(x,y);
                if (((x>=beginX && x<borderX1) || (x>borderX2 && x<=endX)) || 
                    ((y>=beginY && y<borderY1) || (y>borderY2 && y<=endY))) 
                {
                    fillColor = label.color.colorValue;
                } else {
                    fillColor = getMaskedColor(originalColor, maskColor);
                }
                texture2d.SetPixel(x, y, fillColor);
            }
        }
        texture2d.Apply();
        return texture2d;
    }

    /*
    https://blog.csdn.net/itjobtxq/article/details/7065503
    */
    private Color getMaskedColor(Color imageColor, Color maskColor)
    {
        float alpha = 196;
        float mixed_r = maskColor.r + (imageColor.r - maskColor.r) * alpha / 255;
        float mixed_g = maskColor.g + (imageColor.g - maskColor.g) * alpha / 255;
        float mixed_b = maskColor.b + (imageColor.b - maskColor.b) * alpha / 255;
        return new Color(mixed_r,mixed_g, mixed_b, alpha);
    }

    private void PickImage(int maxSize = 512)
    {
        if(NativeGallery.IsMediaPickerBusy()){
            return;
        }

        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path)=>
        {
            Debug.Log($"*** PickImage() at path = {path}");
            if(string.IsNullOrEmpty(path)){
                return;
            }
            Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize, false);
            if(null == texture){
                Debug.Log($"*** Couldn't load texture from {path}");
                return;
            }
            StartCoroutine(doRecognize(texture.EncodeToJPG()));

        },"Choose a picture");

        Debug.Log($"*** PickImage() from gallery, permission result {permission}");
    }

    private void TakePicture(int maxSize=512)
    {
        if(NativeCamera.IsCameraBusy()){
            return;
        }

        NativeCamera.Permission permission = NativeCamera.TakePicture( (path)=>
        {
            Debug.Log($"*** TakePicture() at path = {path}");
            if(string.IsNullOrEmpty(path)){
                return;
            }

            Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize, false);
            if(null == texture){
                Debug.Log($"*** Couldn't load texture from {path}");
                return;
            }
            StartCoroutine(doRecognize(texture.EncodeToJPG()));
        }, maxSize );

        Debug.Log($"*** TakePicture() from camera, permission result {permission}");
    }

    private void SetInteractable(bool interactable)
    {
        m_InteractableGroup.interactable = interactable;
    }

    #region Actions

    void OnCameraButtonClick()
    {
        Debug.Log($"*** Camera button clicked to capture a image...");
        TakePicture();
    }

    void OnGalleryButtonClick()
    {
        Debug.Log($"*** Gallery button clicked to choose a image from gallery");
        PickImage();
    }

    #endregion
    
}
