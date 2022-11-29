using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.IO;

public class CreateNCVolumeWindow : EditorWindow
{
    [MenuItem("NetCDF/Create NC Volume")]
    public static void Open()
    {
        CreateNCVolumeWindow window = GetWindow<CreateNCVolumeWindow>();
        window.titleContent = new GUIContent("Create NC Volume"); 
        window.Show();
    }

    public void CreateGUI()
    {
        ObjectField ncFileField = new ObjectField("NC file");
        ncFileField.objectType = typeof(Object);
        GradientField gradientField = new GradientField("Color Gradient");
        Vector2Field tempRangeField = new Vector2Field("Temperature Range");
        tempRangeField.value = new Vector2(0, 25);

        Button createButton = new Button(() => CreateVolumeTexture(
            Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(ncFileField.value)), 
            gradientField.value, 
            tempRangeField.value.x, 
            tempRangeField.value.y));
        createButton.text = "Create Texture";

        rootVisualElement.Add(ncFileField);
        rootVisualElement.Add(gradientField);
        rootVisualElement.Add(tempRangeField);

        rootVisualElement.Add(createButton);
    }

    public void CreateVolumeTexture(string ncFile, Gradient tempGradient, float minTemp, float maxTemp)
    {
        Debug.Log("Creating Volume Texture from: " + ncFile);

        float[,,] data = NetCDF.Load(ncFile);    

        // Configure the texture
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        int sizeX = data.GetLength(0);
        int sizeY = data.GetLength(1);
        int sizeZ = data.GetLength(2);

        // Create the texture and apply the configuration
        Texture3D texture = new Texture3D(sizeX, sizeY, sizeZ, format, false);
        texture.wrapMode = wrapMode;

        // Create a 3-dimensional array to store color data
        Color[] colors = new Color[sizeX * sizeY * sizeZ];

        // Populate the array
        for (int z = 0; z < sizeZ; z++)
        {
            int zOffset = z * sizeX * sizeY;
            for (int y = 0; y < sizeY; y++)
            {
                int yOffset = y * sizeX;
                for (int x = 0; x < sizeX; x++)
                {
                    float t2 = NetCDF.KelvinToCelsius(data[x, y, z]);
                    Color color = tempGradient.Evaluate(Mathf.InverseLerp(minTemp, maxTemp, t2));

                    colors[x + yOffset + zOffset] = color;
                }
            }
        }

        // Copy the color values to the texture
        texture.SetPixels(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();

        // Save the texture in the assets folder
        AssetDatabase.CreateAsset(texture, "Assets/NetCDFTexture.asset");
    }
}
