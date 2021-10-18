using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SafeAreaUIAdapterEditor),true)]
[CanEditMultipleObjects]
public class SafeAreaUIAdapterEditor : Editor
{
    SerializedProperty fixBottomMarginEnabled;
    SerializedProperty fixedBottomMargin;
    SerializedProperty resizeForBannerAd;

    void OnEnable()
    {
        fixBottomMarginEnabled = serializedObject.FindProperty("fixBottomMarginEnabled");
        fixedBottomMargin = serializedObject.FindProperty("fixedBottomMargin");
        resizeForBannerAd = serializedObject.FindProperty("resizeForBannerAd");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(fixBottomMarginEnabled,new GUIContent("Enable FixBottomMargin"));

        EditorGUI.BeginDisabledGroup(false == fixBottomMarginEnabled.boolValue);
        EditorGUILayout.PropertyField(fixedBottomMargin);
        if(resizeForBannerAd.boolValue){
           EditorGUILayout.HelpBox("FixedBottomMargin won't work when condition of showing banner ad is true",MessageType.Warning,false);
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.PropertyField(resizeForBannerAd);
        serializedObject.ApplyModifiedProperties();
        // EditorGUILayout.LabelField("(Above this object)");
        // EditorGUILayout.HelpBox("HHH2",MessageType.Info);
        
    }
}
