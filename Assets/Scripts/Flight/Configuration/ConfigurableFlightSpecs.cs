using UnityEngine;

[System.Serializable]
public class ConfigurableFlightSpecs {

    public enum MissionType {
        Grid,
        DoubleGrid
    };

    [Tooltip("Flight speed (meters / second)")]
    public float speed = 50F;

    public MissionType missionType;

    [Header("First grid")]
    [Tooltip("Angle of the camera (degrees). 90 means face down.")]
    [Range(0, 90)]
    public float cameraAngle1 = 90;

    [Tooltip("Altitude where the drone will fly (meters)")]
    public float altitude1 = 100;

    [Header("Second grid (used for double grid only)")]
    [Tooltip("Angle of the camera (degrees). 90 means face down.")]
    [Range(0, 90)]
    public float cameraAngle2 = 90;

    [Tooltip("Altitude where the drone will fly (meters)")]
    public float altitude2 = 100;


    public Mission BuildMission(SurveyArea surveyArea, CameraDefinition cameraDefinition) {
        switch(missionType) {
            case MissionType.Grid:
                return new GridMission(surveyArea, cameraDefinition, altitude1, cameraAngle1);
            case MissionType.DoubleGrid:
                return new DoubleGridMission(surveyArea, cameraDefinition, altitude1, cameraAngle1, altitude2, cameraAngle2);
            default:
                throw new System.Exception("Couldn't fin a mission for the selected mission type");
        }
    }

}
