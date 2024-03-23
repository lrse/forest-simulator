using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Terraform/Data/Custom Terrain")]
public class CustomTerrainData : NoiseData {
    public float heightMultiplier;
    public AnimationCurve heightCurve = AnimationCurve.Constant(0, 1, 1);
}
