using System;
using System.IO;
using Microsoft.Research.Science.Data;
using UnityEngine;

public static class NetCDF
{
    public static float[,,] Load(string ncFilePath)
    {
        if (string.IsNullOrEmpty(ncFilePath))
            throw new ArgumentException(nameof(ncFilePath), "File path is invalid");

        if (!File.Exists(ncFilePath))
            throw new FileNotFoundException();

        using (DataSet ds = DataSet.Open(ncFilePath))
        { 
            return LoadData(ds);
            }
    }

    public static float CelsiusToKelvin(float celsius)
    {
        return celsius + 273.15f;
    }

    public static float KelvinToCelsius(float kelvin)
    {
        return kelvin - 273.15f;
    }

    private static float[,,] LoadData(DataSet ds)
    {
        Variable lon = ds["lon"];
        Variable lat = ds["lat"];
        Variable t2 = ds["t2"];

        int[] shape = lon.GetShape();

        MultipleDataResponse res = ds.GetMultipleData(
            DataRequest.GetData(t2, null, null));

        return (float[,,])res[t2.ID].Data;
    }
}
