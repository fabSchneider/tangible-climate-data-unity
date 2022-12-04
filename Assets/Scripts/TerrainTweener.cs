using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TerrainGenerator))]
public class TerrainTweener : MonoBehaviour
{
    [Tooltip("Speed at which the value of the terrain gen is changing in units/s")]
    public float tweenSpeed;
    public Slider slider;

    private TerrainGenerator terrainGenerator;


    void Start()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
    }

    void Update()
    {
        terrainGenerator.SetMonth(Time.time * tweenSpeed);
        if(slider)
            slider.SetValueWithoutNotify(terrainGenerator.Month);
    }
}
