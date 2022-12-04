using System;
using System.IO;
using Microsoft.Research.Science.Data;
using UnityEngine;

public static class NetCDF
{
    public static float[,,] Load2D(string ncFilePath, string dataVariable)
    {
        if (string.IsNullOrEmpty(ncFilePath))
            throw new ArgumentException(nameof(ncFilePath), "File path is invalid");

        if (!File.Exists(ncFilePath))
            throw new FileNotFoundException();

        using (DataSet ds = DataSet.Open(ncFilePath))
        {
            return LoadData2D(ds, dataVariable);
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

    private static float[,,] LoadData2D(DataSet ds, string dataVariable)
    {
        Variable data = ds[dataVariable];

        MultipleDataResponse res = ds.GetMultipleData(
            DataRequest.GetData(data));

        return (float[,,])res[data.ID].Data;
    }
}
