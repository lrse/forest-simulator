using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terraform/Data/Grass")]
public class GrassData : ScriptableObject {
    [Tooltip("The jitter grid factor")]
    public float jitterFactor;
    [Tooltip("Density map")]
    public Pipeline densityPipeline;
    [Tooltip("The maximum number of grass segments. Note this is also bounded by the max value set in the compute shader")]
    public int maxSegments = 3;
    [Tooltip("The maximum bend of a blade of grass, as a multiplier to 90 degrees")]
    public float maxBendAngle = 0;
    [Tooltip("The blade curvature shape")]
    public float bladeCurvature = 1;
    [Tooltip("The base height of a blade")]
    public float bladeHeight = 1;
    [Tooltip("The height variance of a blade")]
    public float heightVariance = 0.1f;
    [Tooltip("The base width of a blade")]
    public float bladeWidth = 1;
    [Tooltip("The width variance of a blade")]
    public float widthVariance = 0.1f;
}
