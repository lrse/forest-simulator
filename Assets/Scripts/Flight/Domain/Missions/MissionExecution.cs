using UnityEngine;
using static Mission;

public class MissionExecution {

    private WaypointProcessor waypointProcessor;
    private DroneMovement droneMovement;

    public delegate void MissionFinishedEvent();
    public static event MissionFinishedEvent OnMissionFinished;
    public delegate void WaypointReachedEvent();
    public static event WaypointReachedEvent OnWaypointReached;

    public MissionExecution(CameraDefinition cameraDefinition, SurveyArea surveyArea, Georeferencing georeferencing, Mission mission, Transform droneTransform, float flightSpeed,  string folderPath) {
        // Calculate the flight plan
        FlightPlan flightPlan = mission.CalculateFlightPlan();
        Debug.Log(string.Format("Mission calculated. Will take {0} images.", flightPlan.waypoints.Count));

        // Build helpers that will move the drone, take the images, and log everything to later geotag the images
        droneMovement = new DroneMovement(droneTransform, flightPlan, flightSpeed);
        waypointProcessor = new WaypointProcessor(cameraDefinition, surveyArea, georeferencing, flightPlan.waypoints.Count, folderPath);

        // Save all configuration as XML
        SaveXML(cameraDefinition, surveyArea, flightPlan, georeferencing, folderPath);

        // Add event listeners
        droneMovement.OnReachedWaypoint += ReachedWaypoint;
        waypointProcessor.OnProcessedAllWaypoints += () => OnMissionFinished?.Invoke();
    }

    public void MoveDrone() {
        droneMovement.MoveAndRotateDrone();
    }

    private void ReachedWaypoint(Waypoint waypoint) {
        // Add the current waypoint for processing (image capture, gcp calculation & image geotagging)
        waypointProcessor.AddWaypointForProcessing(waypoint);
        // Alert possible listeners
        OnWaypointReached?.Invoke();
    }

    private void SaveXML(CameraDefinition cameraDefinition, SurveyArea surveyArea, FlightPlan flightPlan, Georeferencing georeferencing, string folderPath) {
        Debug.Log("Writing an XML that details the mission");
        XMLWriter.WriteToXml(cameraDefinition, surveyArea, flightPlan, georeferencing, folderPath);
    }

}
