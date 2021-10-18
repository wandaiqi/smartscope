using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class EasyDLAccessToken 
{
    public string access_token;
    public int expires_in;
    public long expireDateTimeBinary;
}

public class EasyDLResponse 
{
    public string log_id;

    public Label[] results;

}

public class ImageRecognizer : MonoBehaviour
{
    /* 请在Resource目录下创建config.json文件（格式如下），并分别替换其中的api_key和app_secret为你自己在Baidu EasyDL 上的 api_key 和 app_secret
    {
        "api_key" : "cRDWykbfDzRD5fLqD90M4GuE",
        "app_secret" : "dcAL6D5qMX3LW6xFRQhQAXrUHDX2h5nE"
    }
    */
    private static string API_KEY = "";
    private static string APP_SECRET = "";
    
    private RecognitionResults recognitionResults = null;
    private EasyDLAccessToken token = null;

    void Awake()
    {
        API_KEY = Utility.GetConfigValue("api_key");
        APP_SECRET = Utility.GetConfigValue("app_secret");
    }

    public IEnumerator asyncRecognize(byte[] imageBytes, UnityAction<RecognitionResults> callback)
    {
        Debug.Log($"*** Start asyncRecognize() for image");
        recognitionResults = null;
        yield return StartCoroutine(asyncRecognize(imageBytes));
        
        if(null == recognitionResults){
            callback?.Invoke(null);
            Debug.Log($"*** End asyncRecognize(), the recognitionResults is null! Something must be wrong!!!");
        }else{
            Debug.Log($"*** End asyncRecognize(), {recognitionResults.countOfAll()} objects detected!");
            callback?.Invoke(recognitionResults);
        }
    }

    private IEnumerator asyncRecognize(byte[] imageBytes)
    {

        Debug.Log($"*** Converting to base64 encoding for image bytes");
        string base64 = System.Convert.ToBase64String(imageBytes);
        if(string.IsNullOrEmpty(base64)){
            Debug.LogError($"*** Error in base64 converting of image bytes)");
            // callback?.Invoke(null);
            yield break;
        }


        /** Fetch an access token of Baidu EasyDL **/
        // TODO:  Store the token to PlayerPrefs until expiration
        token = tryLoadAccessTokenFromCache();
        if(null == token || isTokenExpired(token)){
            Debug.Log($"*** Cached access token is expired, trying request new token from EasyDL...");
            yield return StartCoroutine(requestAccessToken());
        }
        /** Query EasyDL to recognize objects in the images **/
        if(null == token)
        {
            Debug.LogError("*** Error, token is null!");
            yield break;
        }

        EasyDLResponse response = null;
        string url = $"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/detection/ddbloodcell?access_token={token.access_token}";
        string bodyJson = "{\"image\":\"" + base64 + "\"}";
        // Debug.Log($"Post body content \n {bodyJson}");

        /* References about using json reqeuest in UnityWebRequest
        https://forum.unity.com/threads/unitywebrequest-post-url-jsondata-sending-broken-json.414708/
        https://forum.unity.com/threads/posting-raw-json-into-unitywebrequest.397871/
        */
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
            request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            
            Debug.Log("*** Status Code: " + request.responseCode);
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError($"*** Error in querying EasyDL!\n {request.error}");
            }
            else
            {
                response = JsonConvert.DeserializeObject<EasyDLResponse>(request.downloadHandler.text);
                Debug.Log($"*** Query result from EasyDL: {request.downloadHandler.text}");
            }
    
        }

        if(null == response){
            Debug.LogError($"*** Error in deserializing from EasyDL response JSON!\n");
            yield break;
        }

        recognitionResults = new RecognitionResults();
        foreach (var item in response.results)
        {
            recognitionResults.add(item);
        }
        
    }

    private IEnumerator requestAccessToken()
    {
        string tokenURL = "https://aip.baidubce.com/oauth/2.0/token";
        WWWForm tokenForm = new WWWForm();
        tokenForm.AddField("grant_type","client_credentials");
        tokenForm.AddField("client_id",API_KEY);
        tokenForm.AddField("client_secret",APP_SECRET);

        using (UnityWebRequest www = UnityWebRequest.Post(tokenURL, tokenForm))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError($"*** Error in getting access token of EasyDL!\n {www.error}");
                // callback?.Invoke(null);
                yield break;
            }
            else
            {
                token = JsonUtility.FromJson<EasyDLAccessToken>(www.downloadHandler.text);
                // TEST: long expirationDateBinary = System.DateTime.Now.AddSeconds(3).ToBinary();
                long expirationDateBinary = System.DateTime.Now.AddSeconds(token.expires_in).ToBinary();
                token.expireDateTimeBinary = expirationDateBinary;
                string expirationDate = System.DateTime.FromBinary(expirationDateBinary).ToString();
                Debug.Log($"*** Successfuly get an access token of EasyDL: {token.access_token},will expire at {expirationDate}");
                cacheAccessToken(token);
                
            }
        }
    }

    private void cacheAccessToken(EasyDLAccessToken token)
    {
        string tokenJson = JsonConvert.SerializeObject(token);
        Debug.Log($"*** cacheAccessToken(): Serializing EasyDL access token to Json string = '{tokenJson}'");
        PlayerPrefs.SetString("EasyDLAccessToken", tokenJson);
        PlayerPrefs.Save();
    }

    private EasyDLAccessToken tryLoadAccessTokenFromCache()
    {
        string tokenJson = PlayerPrefs.GetString("EasyDLAccessToken",null);
        Debug.Log($"*** Trying load access token from cache, tokenJson='{tokenJson}'");
        if(string.IsNullOrEmpty(tokenJson)){
            return null;
        }
        return JsonConvert.DeserializeObject<EasyDLAccessToken>(tokenJson);
    }

    private bool isTokenExpired(EasyDLAccessToken token)
    {
        if(null == token || token.expireDateTimeBinary == 0){
            return true;
        }
        
        // Debug.Log($"Long LL default value = {LL}");
        System.DateTime now = System.DateTime.Now;
        System.DateTime expiration = System.DateTime.FromBinary(token.expireDateTimeBinary);
        if(now >= expiration){
            return true;
        } else {
            return false;
        }
    }

    public RecognitionResults getExamples()
    {
        return Utility.getLabelExamples();
    }

    

}
