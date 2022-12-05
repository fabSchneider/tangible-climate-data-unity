using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NCAsset))]
public class NCAssetEditor : Editor
{
    public static int dataPreviewCount = 20;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        NCAsset asset = (NCAsset)target;
       

        var dataProp = serializedObject.FindProperty("data");

        GUILayout.Label("Data:", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(8);
        GUILayout.BeginVertical("box");
        GUILayout.Label("Shape: " + string.Join('x', asset.shape));
        int arraySize = dataProp.arraySize;
        int previewCount = Mathf.Min(arraySize, dataPreviewCount);
        if (previewCount > 0)
        {
            if (previewCount != arraySize)
                GUILayout.Label($"Previewing the first {previewCount} entries out of {arraySize} total entries");

            for (int i = 0; i < previewCount; i++)
            {
                GUILayout.Label($"[{i}]  {dataProp.GetArrayElementAtIndex(i).floatValue}");
            }
            if (previewCount != arraySize)
                GUILayout.Label("...");
        }
        else
        {
            GUILayout.Label("No data");
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();




        serializedObject.ApplyModifiedProperties();
    }
}
