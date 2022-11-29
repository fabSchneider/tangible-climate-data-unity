using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Terrain))]
[RequireComponent(typeof(TerrainCollider))]
public class TerrainGenerator : MonoBehaviour
{
    [Tooltip("Path to the .nc file inside the StreamingAssets folder")]
    public string ncFile;

    [Tooltip("The maximum height of the terrain in meters")]
    public int maxHeight = 30;

    // The terrain data we will be manipulating
    private TerrainData terrainData;

    // The temperature data we get from the .nc file
    // x = month, y = longitude, z = latitude
    private float[,,] t2Data;

    // the month we want to set the heights for
    private int month = 0;

    void Start()
    {
        // get a reference to the terrain and
        // terrain collider component to update them later
        Terrain terrain = GetComponent<Terrain>();
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();

        // create a new terrain data object
        terrainData = new TerrainData();
        terrainData.name = "TerrainGen";

        // Load the netcdf data from the streaming assets folder
        string path = Path.Combine(Application.streamingAssetsPath, ncFile);
        t2Data = NetCDF.Load(path);

        // get the dimensions of the array in x and y direction
        int sizeX = t2Data.GetLength(1);
        int sizeY = t2Data.GetLength(2);

        // height map resolution needs to be power of two + 1
        // so we take the smallest of the 2 data dimensions
        // and get the closest power of to smaller than that value. 
        int size = FloorToNearestPowerOfTwo(Mathf.Min(sizeX, sizeY)) + 1;

        // set the resolution of the terrain data height map to our calculated size 
        terrainData.heightmapResolution = size;

        // the size of the terrain will be the same as our input
        // so each temperature sample will cover 1x1m
        terrainData.size = new Vector3(size, maxHeight, size);

        // we call update terrain to generate the terrain for the first time
        UpdateTerrainHeights();

        // lastly we set the terrain data in the terrain and terrain collider component
        // since we are setting a reference to the terrain data we are manipulating here
        // the terrain will automatically update as we update the terrain heights.
        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
    }

    /// <summary>
    /// Sets the month that should be displayed.
    /// </summary>
    /// <param name="month">The month (should be between 0 (January) and 11 (December)</param>
    public void SetMonth(float month)
    {     
        // make sure the month is in the range of 0 to 11
        int newMonth = Mathf.Clamp(Mathf.RoundToInt(month), 0, 11);
        if(this.month != newMonth)
        {
            // only update the terrain if the new month is different 
            // from the current month
            this.month = newMonth;
            UpdateTerrainHeights();
        }
    }

    /// <summary>
    /// (Re)generates the terrain geometry for the set month. 
    /// </summary>
    /// <param name="month"></param>
    private void UpdateTerrainHeights()
    {
        // get the size of the terrain height map
        int size = terrainData.heightmapResolution;

        // we want to get a "slice" of the 3d temperature data
        // a slice which represents the temperature at longitude and latitude (x, y)
        // for the specified specific month
        float[,] t2Slice = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // here we copy the actual temperature data
                float t2 = t2Data[month, x, y];

                // the temperature comes in Kelvin so we first convert it to Celsius
                t2 = NetCDF.KelvinToCelsius(t2);
                // because the terrain expects the height to be between 0 and 1
                // we divide the value with our max height
                t2Slice[x, y] = t2 / maxHeight;
            }
        }
        // finally we set the heights of the terrain with the data from our slice
        terrainData.SetHeights(0, 0, t2Slice);
    }

    /// <summary>
    /// Returns a power of 2 number closest but smaller to the input number. 
    /// E.g 20 => 16, 1200 => 1024
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private int FloorToNearestPowerOfTwo(int x)
    {
        return (int)Mathf.Pow(2, Mathf.FloorToInt(Mathf.Log(x) / Mathf.Log(2)));
    }
}
