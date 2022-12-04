using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainDeform : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;
    private List<TerrainDeformer> deformers;

    private float[,] heightSamples;

    void Start()
    {
        terrain = GetComponent<Terrain>();

        // create a copy of the terrain data so that changes 
        // won't persist after exiting play mode
        terrainData = TerrainData.Instantiate(terrain.terrainData);
        terrain.terrainData = terrainData;
        heightSamples = new float[terrainData.heightmapTexture.width, terrainData.heightmapTexture.height];
        deformers = new List<TerrainDeformer>();
    }

    void Update()
    {
        UpdateTerrainFromDeformers();
    }

    private void UpdateTerrainFromDeformers()
    {
        int samplesX = heightSamples.GetLength(0);
        int samplesY = heightSamples.GetLength(1);
        float maxHeight = terrainData.size.y;

        // reset all heights to 0
        for (int x = 0; x < samplesX; x++)
        {
            for (int y = 0; y < samplesY; y++)
            {
                heightSamples[x, y] = 0f;
            }
        }

        GetComponentsInChildren(false, deformers);

        foreach (TerrainDeformer deformer in deformers)
        {
            float deformerRadius = deformer.Radius;

            Vector3 deformerPos = deformer.transform.localPosition;
            Vector2 deformerPosNormalized = new Vector2(deformerPos.z / terrainData.size.x, deformerPos.x / terrainData.size.z);
            float deformerHeightNormalized = deformerPos.y / terrainData.size.y;

            for (int x = 0; x < samplesX; x++)
            {
                for (int y = 0; y < samplesY; y++)
                {
                    Vector2 samplePos = new Vector2((float)x / samplesX, (float)y / samplesY);
                    float deformerDist = Vector2.Distance(samplePos, deformerPosNormalized);
                    float distNormalized = Mathf.Clamp01(deformerDist / deformerRadius);
                    float deform = deformer.falloffCurve.Evaluate(distNormalized);
                    deform *= deformerHeightNormalized;

                    switch (deformer.blendMode)
                    {
                        case TerrainDeformer.BlendMode.Additive:
                            {
                                heightSamples[x, y] += deform;
                                break;
                            }
                        case TerrainDeformer.BlendMode.Subtractive:
                            {
                                heightSamples[x, y] -= deform;
                                break;
                            }
                        case TerrainDeformer.BlendMode.Multiply:
                            {
                                heightSamples[x, y] *= deform;
                                break;
                            }
                        case TerrainDeformer.BlendMode.Maximum:
                            {
                                float height = heightSamples[x, y];
                                heightSamples[x, y] = Mathf.Max(deform, height);
                                break;
                            }
                        case TerrainDeformer.BlendMode.Minimum:
                            {
                                float height = heightSamples[x, y];
                                heightSamples[x, y] = Mathf.Min(deform, height);
                                break;
                            }

                        default:
                            break;
                    }


                }
            }
        }

        terrainData.SetHeights(0, 0, heightSamples);
    }
}
