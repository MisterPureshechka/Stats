#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class InputsAxesList
{
    public static string[] Names 
    { 
        get 
        {
            var result = new List<string>();

            var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            var obj = new SerializedObject(inputManager);
            var axes = obj.FindProperty("m_Axes");
            for (int i = 0; i < axes.arraySize; i++)
            {
                var axis = axes.GetArrayElementAtIndex(i);
                result.Add(axis.FindPropertyRelative("m_Name").stringValue);
            }

            return result.Union(result).ToArray();
        } 
    }

    public static string DrawValues(string currentValue, Rect position, string label)
    {
        var currentInputIndex = 0;
        var axesNames = Names;
        for (var i = 0; i < axesNames.Length; i++)
            if (axesNames[i] == currentValue)
                currentInputIndex = i;
        return axesNames[EditorGUI.Popup(position, label, currentInputIndex, axesNames)];
    }
}
#endif