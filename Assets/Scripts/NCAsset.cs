using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NCAsset : ScriptableObject
{
    public string variable;
    public int[] shape;
    public float minValue;
    public float maxValue; 
    public float[] data;

    public float Get(int x)
    {
        return data[x];
    }

    public float Get(int x, int y)
    {
        return data[x * shape[1] + y];
    }

    public float Get(int x, int y, int z)
    {
        return data[(x * shape[1] + y) * shape[2] + z];
    }

    public float Get(params int[] indices)
    {
        int flatIndex = indices[0];
        for (int i = 1; i < indices.Length; i++)
        {
            flatIndex *= shape[i];
            flatIndex += indices[i];
        }
        return data[flatIndex];
    }
}
