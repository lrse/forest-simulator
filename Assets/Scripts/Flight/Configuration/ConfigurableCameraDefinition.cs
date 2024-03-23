using UnityEngine;
using System.Collections;

[System.Serializable]
public class ConfigurableCameraDefinition {

    [Tooltip("Lens focal length (millimeters). Not 35mm equivalent.")]
    public float focalLength = 3.61F;

    [Tooltip("Horizontal size of the sensor (millimeters)")]
    public float sensorSizeX = 6.24F;

    [Tooltip("Verticall size of the sensor (millimeters)")]
    public float sensorSizeY = 4.68F;

    [Tooltip("The width of the images taken by the drone (pixels)")]
    public int resolutionX = 4000;

    [Tooltip("The height of the images taken by the drone (pixels)")]
    public int resolutionY = 3000;

    public CameraDefinition BuildDomain() {
        return new CameraDefinition(focalLength, sensorSizeX, sensorSizeY, resolutionX, resolutionY);
    }
}
