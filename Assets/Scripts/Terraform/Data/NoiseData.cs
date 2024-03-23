using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terraform/Data/Noise")]
public class NoiseData : ScriptableObject {
    public float scale;

    [MinAttribute(0)] public int octaves = 4;
    [Range(0, 1)] public float persistance = 0.25f;
    [MinAttribute(0)] public float lacunarity = 0.25f;
    public int seed;
    public Vector2 offset;
}
