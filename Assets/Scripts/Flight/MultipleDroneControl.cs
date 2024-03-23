using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MultipleDroneControl : MonoBehaviour {
    // This class simulates a drone flight taking cameras of a terrain.
    // We assume that the terrain is on the floor, facing up.

    #region External Configuration

    public ConfigurableSurveyArea surveyAreaConfiguration;
    public ConfigurableCameraDefinition cameraDefinitionConfiguration;
    public List<ConfigurableFlightSpecs> flightSpecsConfigurations;
    public ConfigurableGeoreferencing georeferencingConfiguration;

    [Tooltip("The path to the folder where every run will be saved.")]
    public string folderPath = "Images";

    #endregion

    private CameraDefinition cameraDefinition;
    private SurveyArea surveyArea;
    private Georeferencing georeferencing;
    private MissionExecution missionExecution;

    private int currentMission = 0;

    void Start() {
        // Assert an object was selected
        if (surveyAreaConfiguration.objectOfInterest == null) {
            throw new System.Exception("You must set an object of interest!");
        }

        // Build necessary entities
        cameraDefinition = cameraDefinitionConfiguration.BuildDomain();
        surveyArea = surveyAreaConfiguration.BuildDomain();
        georeferencing = georeferencingConfiguration.BuildDomain(surveyArea);

        // Set the camera
        Camera camera = GetComponent<Camera>();
        camera.usePhysicalProperties = true;
        camera.focalLength = cameraDefinition.focalLength;
        camera.sensorSize = new Vector2(cameraDefinition.sensorSizeX, cameraDefinition.sensorSizeY);

        // Prepare the execution
        PrepareMission(flightSpecsConfigurations[0]);

        // Add event listeners
        MissionExecution.OnMissionFinished += MissionFinished;
    }

    // Update is called once per frame
    void Update() {
        missionExecution.MoveDrone();
    }

    private void PrepareMission(ConfigurableFlightSpecs flightSpecsConfiguration) {
        missionExecution = new MissionExecution(cameraDefinition,
           surveyArea,
           georeferencing,
           flightSpecsConfiguration.BuildMission(surveyArea, cameraDefinition),
           transform,
           flightSpecsConfiguration.speed,
           Path.Combine(folderPath, GetFolderNameForMission(flightSpecsConfiguration)));
    }

    private void MissionFinished() {
        currentMission++;

        if (currentMission >= flightSpecsConfigurations.Count) {
            Quit();
        } else {
            PrepareMission(flightSpecsConfigurations[currentMission]);
        }
    }

    private string GetFolderNameForMission(ConfigurableFlightSpecs flightSpecsConfiguration) {
        if (flightSpecsConfiguration.missionType == ConfigurableFlightSpecs.MissionType.Grid) {
            return string.Format("Grid_{0}m_{1}", flightSpecsConfiguration.altitude1, flightSpecsConfiguration.cameraAngle1);
        } else {
            return string.Format("Double_Grid_{0}m_{1}_{2}m_{3}", flightSpecsConfiguration.altitude1, flightSpecsConfiguration.cameraAngle1, flightSpecsConfiguration.altitude2, flightSpecsConfiguration.cameraAngle2);
        }
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
