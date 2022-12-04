using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Terrain))]
[RequireComponent(typeof(TerrainCollider))]
public class TerrainGenerator : MonoBehaviour
{
    [Tooltip("The nc asset")]
    public NCAsset ncAsset;

    public Material terrainMaterial;

    [Tooltip("The maximum height of the terrain in meters")]
    public float maxHeight = 30;

    private Terrain terrain;
    // The terrain data we will be manipulating
    private TerrainData terrainData;

    // the month we want to set the heights for
    private float month = 0f;
    public float Month => month;

    private bool updateTerrainFlag;

    private void OnValidate()
    {
        if(terrainMaterial)
            GetComponent<Terrain>().materialTemplate = terrainMaterial;
    }

    private void Start()
    {
        // get a reference to the terrain and
        // terrain collider component to update them later
        terrain = GetComponent<Terrain>();
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();

        // we create a copy of the terrain Material so that changes
        // won't be saved to the source material after finishing play mode
        terrainMaterial = new Material(terrainMaterial);
        terrainMaterial.name = terrainMaterial.name + "Inst";

        // we set the range properties of the terrain material instance
        // base on the min and max value of the nc data
        //terrainMaterialInst.SetFloat("_MaxHeight", maxHeight);
        terrainMaterial.SetFloat("_MaxHeight", maxHeight);

        // create a new terrain data object
        terrainData = new TerrainData();
        terrainData.name = "TerrainGen";

        // get the dimensions of the array in x and y direction
        int sizeX = ncAsset.shape[1];
        int sizeY = ncAsset.shape[2];

        // height map resolution needs to be power of two + 1
        // so we take the smallest of the 2 data dimensions
        // and get the closest power of to smaller than that value. 
        int size = FloorToNearestPowerOfTwo(Mathf.Min(sizeX, sizeY)) + 1;

        // set the resolution of the terrain data height map to our calculated size 
        terrainData.heightmapResolution = size;

        // the size of the terrain will be the same as our input
        // so each temperature sample will cover 1x1m
        terrainData.size = new Vector3(sizeX, maxHeight, sizeY);

        // we set the update flag to true so that the terrain will be
        // updated at the next call to Update()
        updateTerrainFlag = true;

        // lastly we set the terrain data in the terrain and terrain collider component
        // since we are setting a reference to the terrain data we are manipulating here
        // the terrain will automatically update as we update the terrain heights.
        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
        terrain.materialTemplate = terrainMaterial;

    }

    private void Update()
    {
        // we update the terrain if the update flag
        // is set to true. This way we avoid updating the 
        // terrain more than once per frame
        if (updateTerrainFlag)
        {
            UpdateTerrainHeights();
            updateTerrainFlag = false;
        }
    }

    /// <summary>
    /// Sets the month that should be displayed.
    /// </summary>
    /// <param name="month">The month (should be between 0 (January) and 11 (December)</param>
    public void SetMonth(float month)
    {
        // make sure the month is in the range of 0 to 12
        float newMonth = month % 12;
        if(this.month != newMonth)
        {
            // only update the terrain if the new month is different 
            // from the current month
            this.month = newMonth;
            updateTerrainFlag = true;
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
        // a slice which represents the data at longitude and latitude (x, y)
        // for the specified specific month
        float[,] dataSlice = new float[size, size];

        // get the base month and the fractional value 
        int baseMonth = Mathf.FloorToInt(month);
        float frac = month - baseMonth;
        // we make sure the month wraps back to 0 if it is 12 or larger
        baseMonth = baseMonth % 12;
        // the next month we will sample from to get the interpolated data
        int nextMonth = (baseMonth + 1) % 12;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // here we get the actual data for the base month and the month after
                float baseData = ncAsset.Get(baseMonth, x, y);
                float nextData = ncAsset.Get(nextMonth, x, y);

                // we interpolate between base and next
                // using the fractional value we extracted before
                float interpolatedData = Mathf.Lerp(baseData, nextData, frac);

                // because the terrain expects the height to be between 0 and 1
                // we remap the value from the assets min to max range to a normalized range between 0 and 1
                dataSlice[x, y] = Mathf.InverseLerp(ncAsset.minValue, ncAsset.maxValue, interpolatedData);
            }
        }
        // finally we set the heights of the terrain with the data from our slice
        terrainData.SetHeights(0, 0, dataSlice);
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
