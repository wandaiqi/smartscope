using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Location {
    public int left;
    public int top;
    public int width;
    public int height;
}

public struct Label {

    public string name; //标签名称
    public float score; //置信度
    public LabelColor color; //标签颜色
    public Location location; //目标主体的位置

}

public struct LabelColor {
    public string colorName;
    public Color colorValue;

    public LabelColor(string name, Color color){
        this.colorName = name;
        this.colorValue = color;
    }

    public static readonly LabelColor red = new LabelColor("red", Color.red);
    public static readonly LabelColor yellow = new LabelColor("yellow", Color.yellow);
    public static readonly LabelColor green = new LabelColor("green", Color.green);
    public static readonly LabelColor blue = new LabelColor("blue", Color.blue);
    public static readonly LabelColor gray = new LabelColor("gray", Color.gray);

    public static LabelColor[] PredefinedColors = new LabelColor[] {
        red,yellow,green,blue,gray
    };

}

public class RecognitionResults {
    private List<string> labelNameList = new List<string>();
    private Dictionary<string, List<Label>> labelsDictionary = new Dictionary<string, List<Label>>();
    private Dictionary<string, LabelColor> labelColorMap = new Dictionary<string, LabelColor>();

    private LabelColor[] colorsPool = LabelColor.PredefinedColors;
    private int colorCursor = 0;

    public void add(Label label)
    {
        if(!labelNameList.Contains(label.name)){
            labelNameList.Add(label.name);
            labelsDictionary.Add(label.name, new List<Label>());
        }
        
        if(!labelColorMap.ContainsKey(label.name)){
            labelColorMap.Add(label.name, getColor());
        }

        label.color = labelColorMap[label.name];
        labelsDictionary[label.name].Add(label);
    }

    public string[] allLabelNames()
    {
       return labelNameList.ToArray();
    }

    public List<Label> getLableByName(string labelName)
    {
        return labelsDictionary[labelName];
    }

    public List<Label> getAllLabels()
    {
        List<Label> all = new List<Label>();
        foreach (var item in labelsDictionary)
        {
            all.AddRange(item.Value);
        }
        return all;
    }

    public int countOfLabel(string labelName)
    {
        var list = labelsDictionary[labelName];
        if(null == list){
            Debug.LogError("*** getCountOfLabel(), NULL label list!");
            return 0;
        }

        return list.Count;
    }

    public int countOfAll()
    {
        int count = 0;
        foreach (var item in labelsDictionary)
        {
            count += item.Value.Count;
        }
        return count;
    }

    public LabelColor colorOfLabel(string labelName)
    {
        return labelColorMap[labelName];
    }
    
    private LabelColor getColor()
    {
        if(colorCursor >= colorsPool.Length){
            colorCursor = 0;
        }

        LabelColor color = colorsPool[colorCursor];
        colorCursor++;
        return color;
    }

}


public class DataModel { 

    // 注意标签名称全部改为小写，比较时可以忽略大小写
    public static Dictionary<string, string> LabelNameAliasMap = new Dictionary<string, string>() {
        {"wbc","白细胞"},
        {"rbc","红细胞"},
        {"platelets","血小板"}
    };
}