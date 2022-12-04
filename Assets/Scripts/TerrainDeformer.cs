using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDeformer : MonoBehaviour
{
    public enum BlendMode
    {
        Additive, 
        Subtractive,
        Multiply,
        Maximum,
        Minimum
    }

    public AnimationCurve falloffCurve;
    public BlendMode blendMode = BlendMode.Maximum;
    public float Radius => transform.localScale.x / 10f; 
}
