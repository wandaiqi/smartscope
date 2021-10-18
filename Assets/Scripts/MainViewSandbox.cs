using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;



public class MainViewSandbox : MonoBehaviour
{
    [SerializeField] private RawImage m_RawImage;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadTestRawImage());   
    }

    private IEnumerator LoadTestRawImage()
    {
        string path = Path.Combine(Application.streamingAssetsPath,"BloodImage_00001_jpg.rf.d702f2b1212a2ed897b5607804109acf.jpg");
        Texture2D texture2d = new Texture2D(4, 3);
        texture2d.LoadImage(File.ReadAllBytes(path));
        Debug.Log($"*** width={texture2d.width} and height={texture2d.height}");
        // m_RawImage.texture = Resize(texture2d, 160, 120);
        
        Location objLoc = new Location();
        objLoc.width = 160;
        objLoc.height = 120;
        objLoc.left = 30;
        objLoc.top = 90;
        m_RawImage.texture = DrawObjectRectangle(texture2d,objLoc, Color.green);

        var panel = m_RawImage.GetComponentInParent<RectTransform>();
        var panelWidth = panel.sizeDelta.x;
        var panelHeight = panel.sizeDelta.y;
        Vector2 fixedSizeDelta = new Vector2(panelWidth, texture2d.height * (panelWidth/texture2d.width));
        Debug.Log($"*** fixed sizeDelta of image is {fixedSizeDelta}");
        m_RawImage.GetComponent<RectTransform>().sizeDelta = fixedSizeDelta;
        yield return null;
    }

    /*
    https://gamedev.stackexchange.com/questions/92285/unity3d-resize-texture-without-corruption
    */
    public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Point;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0,0);
        nTex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return nTex;
    }

    public static Texture2D DrawObjectRectangle(Texture2D texture2d, Location location, Color maskColor) {
        int WWidth = texture2d.width;
        int WHeight = texture2d.height;
        int beginX = location.left;
        int endX = beginX + location.width;
        int beginY = WHeight - location.top - location.height;
        int endY = beginY + location.height;
        Color32 originalColor;
        Color32 fillColor;

        for (int y = beginY; y <= endY; y++)
        {
            for (int x = beginX; x <= endX; x++)
            {
                originalColor = texture2d.GetPixel(x,y);
                fillColor = getMaskedColor(originalColor, maskColor);
                texture2d.SetPixel(x, y, fillColor);
            }
        }
        texture2d.Apply();
        return texture2d;
    }

    /*
    https://blog.csdn.net/itjobtxq/article/details/7065503
    */
    public static Color getMaskedColor(Color imageColor, Color maskColor){
        float alpha = 196;
        float mixed_r = maskColor.r + (imageColor.r - maskColor.r) * alpha / 255;
        float mixed_g = maskColor.g + (imageColor.g - maskColor.g) * alpha / 255;
        float mixed_b = maskColor.b + (imageColor.b - maskColor.b) * alpha / 255;
        return new Color(mixed_r,mixed_g, mixed_b, alpha);
    }
}
