using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



public class Utility 
{
    
    public static RecognitionResults getLabelExamples () 
    {
        RecognitionResults exampleResults = new RecognitionResults();

        Label label1 = new Label();
        label1.name = "RBC";
        label1.score = 0.831f;
        // label1.color = LabelColor.red;
            Location loc1 = new Location();
            loc1.width = 160;
            loc1.height = 120;
            loc1.left = 30;
            loc1.top = 90;
        label1.location = loc1;
        exampleResults.add(label1);

        Label label2 = new Label();
        label2.name = "WBC";
        label2.score = 0.823f;
        label2.color = LabelColor.yellow;
            Location loc2 = new Location();
            loc2.width = 115;
            loc2.height = 120;
            loc2.left = 130;
            loc2.top = 200;
        label2.location = loc2;
        exampleResults.add(label2);

        Label label3 = new Label();
        label3.name = "Platelets";
        label3.score = 0.722f;
        label3.color = LabelColor.green;
            Location loc3 = new Location();
            loc3.width = 95;
            loc3.height = 65;
            loc3.left = 230;
            loc3.top = 300;
        label3.location = loc3;
        exampleResults.add(label3);

        // Dictionary<string, Label[]> result = new Dictionary<string, Label[]>();
        // result.Add(label1.name, new Label[] {label1});
        // result.Add(label2.name, new Label[] {label2});
        // result.Add(label3.name, new Label[] {label3});

        return exampleResults;
    }

    public static string GetConfigValue(string key)
    {
        TextAsset configAsset = Resources.Load<TextAsset>("config");
        if(null == configAsset || string.IsNullOrEmpty(configAsset.text)){
            Debug.LogError("*** Error in getConfigValue(), can't find config.json or it is empty!");
            return null;
        }
        string configJson = configAsset.text;
        JObject jo = JsonConvert.DeserializeObject<JObject>(configJson);

        if(null == jo)
        {
            Debug.LogError("*** Error in load config.json, null JObject!");
            return null;
        }
        string value = jo[key].ToString();
        Debug.Log($"*** getConfigValue({key}) = {value}");
        return value;
        
    }
    
}
