using UnityEngine;
using UnityEditor.AssetImporters;
using Microsoft.Research.Science.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// Scripted importer for .nc files
/// </summary>
[ScriptedImporter(1, "nc")]
public class NCImporter : ScriptedImporter
{
    [Serializable]
    public enum Conversion
    {
        None,
        KelvinToCelsius,
        Custom
    }

    [Tooltip("The conversion that should be applied to the data")]
    public Conversion conversion;

    public float customConversionBase;
    public float customConversionFactor;

    public string variableName;
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DataSet ds = DataSet.Open(ctx.assetPath, ResourceOpenMode.ReadOnly);
        NCAsset asset = ScriptableObject.CreateInstance<NCAsset>();
        asset.variables = ds.Variables.Select(v => new NCAsset.VariableInfo() { name = v.Name, shape = v.GetShape() }).ToList();

        if (!string.IsNullOrEmpty(variableName))
        {
            // import data if variable is in the list of variables
            NCAsset.VariableInfo varInfo = asset.variables.FirstOrDefault(v => v.name == variableName);
            if (varInfo != null)
            {
                Variable dataVar = ds[varInfo.name];

                MultipleDataResponse res = ds.GetMultipleData(
                    DataRequest.GetData(dataVar));

                int[] shape = dataVar.GetShape();
                Array dataArr = dataVar.GetData();

                int flatLength = shape.Aggregate((a, b) => a * b);

                Func<float, float> converter = null;
                switch (conversion)
                {
                    case Conversion.KelvinToCelsius:
                        converter = NetCDF.KelvinToCelsius;
                        break;
                    case Conversion.Custom:
                        converter = CustomConversion;
                        break;
                }

                float[] flatData = Flatten(dataArr, shape, converter, out float min, out float max);
                asset.shape = shape;
                asset.data = flatData;
                asset.minValue = min;
                asset.maxValue = max;
                Debug.Log($"Loaded Data Length: {flatData.Length}\nMin Value: {min}\nMax Value: {max}");
            }
        }

        ds.Dispose();
        ctx.AddObjectToAsset("Main Obj", asset);
        ctx.SetMainObject(asset);
    }

    private float CustomConversion(float val)
    {
        return val * customConversionFactor + customConversionBase;
    }

    private static float[] Flatten(Array data, int[] shape, Func<float, float> conversion, out float minVal, out float maxVal)
    {
        int flatLength = shape.Aggregate((a, b) => a * b);
        float[] arr = new float[flatLength];
        var stack = new Stack<IEnumerator>();
        int idx = 0;

        minVal = float.PositiveInfinity;
        maxVal = float.NegativeInfinity;

        stack.Push(data.GetEnumerator());
        do
        {
            for (var iterator = stack.Pop(); iterator.MoveNext();)
            {
                if (iterator.Current is Array)
                {
                    stack.Push(iterator);
                    iterator = (iterator.Current as IEnumerable).GetEnumerator();
                }
                else
                {
                    float val = (float)iterator.Current;
                    // apply chosen conversion
                    if(conversion != null)
                        val = conversion(val);

                    if(val < minVal)
                        minVal = val;
                    if(val > maxVal)
                        maxVal = val;

                    arr[idx++] = val;
                }
            }
        }
        while (stack.Count > 0);
        return arr;
    }
}