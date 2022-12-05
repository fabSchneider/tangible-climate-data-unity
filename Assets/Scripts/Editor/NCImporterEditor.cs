using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(NCImporter))]
public class NCImporterEditor : ScriptedImporterEditor
{
    private string[] variableNames;
    private List<NCImporter.VariableInfo> varInfos;

    public override void OnEnable()
    {
        base.OnEnable();

        NCImporter importer = ((NCImporter)target);
        varInfos = importer.availableVariables;
        variableNames = varInfos.Select(v => v.ToString()).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty conversionProp = serializedObject.FindProperty("conversion");
        EditorGUILayout.PropertyField(conversionProp);

        // if custom 
        if (conversionProp.enumValueIndex == 1)
        {
            SerializedProperty conversionBaseProp = serializedObject.FindProperty("customConversionBase");
            SerializedProperty conversionFactorProp = serializedObject.FindProperty("customConversionFactor");

            EditorGUILayout.PropertyField(conversionBaseProp);
            EditorGUILayout.PropertyField(conversionFactorProp);
        }

        SerializedProperty variableProp = serializedObject.FindProperty("selectedVariable");

        int selection = EditorGUILayout.Popup("Import Variable", variableProp.intValue, variableNames);

        SerializedProperty shapeRangeProp = serializedObject.FindProperty("shapeRange");

        if (selection != -1)
        {
            if (selection != variableProp.intValue)
            {
                variableProp.intValue = selection;
                NCImporter.VariableInfo varInfo = varInfos[selection];
                // populate shape range
                shapeRangeProp.arraySize = varInfo.shape.Length;
                for (int i = 0; i < shapeRangeProp.arraySize; i++)
                {
                    shapeRangeProp.GetArrayElementAtIndex(i).vector2IntValue = new Vector2Int(0, varInfo.shape[i]);
                }
            }

            variableProp.intValue = selection;
            for (int i = 0; i < shapeRangeProp.arraySize; i++)
            {
                EditorGUILayout.PropertyField(shapeRangeProp.GetArrayElementAtIndex(i));
            }
        }

        serializedObject.ApplyModifiedProperties();
        base.ApplyRevertGUI();
    }
}