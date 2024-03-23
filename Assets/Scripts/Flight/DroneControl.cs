using UnityEngine;

public class DroneControl : MonoBehaviour {
    // This class simulates a drone flight taking cameras of a terrain.
    // We assume that the terrain is on the floor, facing up.

    #region External Configuration

    public ConfigurableSurveyArea surveyAreaConfiguration;
    public ConfigurableCameraDefinition cameraDefinitionConfiguration;
    public ConfigurableFlightSpecs flightSpecsConfiguration;
    public ConfigurableGeoreferencing georeferencingConfiguration;

    [Tooltip("The path to the folder where the images will be saved. Everything inside the folder will be deleted before starting.")]
    public string folderPath = "Images";

    [Header("Debug")]
    [Tooltip("Draw the flight plan grid")]
    public bool drawGrid = false;
    #endregion

    private MissionDebugger missionDebugger;
    private MissionExecution missionExecution;

    private int currentWaypointInMission; // Starts with 1

    void Start() {
        // Assert an object was selected
        if (surveyAreaConfiguration.objectOfInterest == null) {
            throw new System.Exception("You must set an object of interest!");
        }

        // Build necessary entities
        CameraDefinition cameraDefinition = cameraDefinitionConfiguration.BuildDomain();
        SurveyArea surveyArea = surveyAreaConfiguration.BuildDomain();
        Georeferencing georeferencing = georeferencingConfiguration.BuildDomain(surveyArea);

        // Set the camera
        Camera camera = GetComponent<Camera>();
        camera.usePhysicalProperties = true;
        camera.focalLength = cameraDefinition.focalLength;
        camera.sensorSize = new Vector2(cameraDefinition.sensorSizeX, cameraDefinition.sensorSizeY);

        Mission mission = flightSpecsConfiguration.BuildMission(surveyArea, cameraDefinition);

        // Prepare the execution
        missionExecution = new MissionExecution(cameraDefinition, 
            surveyArea, 
            georeferencing,
            mission, 
            transform, 
            flightSpecsConfiguration.speed, 
            folderPath);

        missionDebugger = new MissionDebugger(cameraDefinition, surveyArea, mission, transform);

        // Add event listeners
        MissionExecution.OnWaypointReached += ReachedWaypoint;
        MissionExecution.OnMissionFinished += Quit;
    }

    // Update is called once per frame
    void Update() {
        missionExecution.MoveDrone();

        if (drawGrid) {
            missionDebugger.DrawDebug(currentWaypointInMission);
        }
    }

    private void ReachedWaypoint() {
        // Mark waypoint as completed
        currentWaypointInMission++;
    }

    private void Quit() {
        Debug.Log("All done!");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
