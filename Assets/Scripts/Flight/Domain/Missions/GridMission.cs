using UnityEngine;
using System.Collections.Generic;

public class GridMission : Mission {

    private readonly float altitude;
    private readonly float cameraAngle;
    private readonly float imageWidth, imageHeight;

    public GridMission(SurveyArea surveyArea, CameraDefinition cameraDefinition, float relativeAltitude, float cameraAngle)
        : base(MissionType.Grid, surveyArea, cameraDefinition) {
        this.altitude = surveyArea.areaToCover.center.y - surveyArea.areaToCover.extents.y + relativeAltitude;
        this.cameraAngle = cameraAngle;
        this.imageWidth = altitude * cameraDefinition.sensorSizeX / cameraDefinition.focalLength;
        this.imageHeight = altitude * cameraDefinition.sensorSizeY / cameraDefinition.focalLength;
    }

    protected override List<Waypoint> CalculateWaypoints() {
        List<Vector3> positions = CalculatePositions();
        List<Waypoint> waypoints = new List<Waypoint>();
        for (int i = 0; i < positions.Count - 1; i++) {
            Vector3 current = positions[i];
            Vector3 next = positions[i + 1];
            waypoints.Add(new Waypoint(current, MissionHelper.CalculateRotationBetweenWaypoints(current, next, cameraAngle)));
        }

        if (waypoints.Count > 0) {
            waypoints.Add(new Waypoint(positions[positions.Count - 1], waypoints[waypoints.Count - 1].droneRotation));
        } else {
            waypoints.Add(new Waypoint(positions[0], Quaternion.AngleAxis(cameraAngle, Vector3.right)));
        }
        return waypoints;
    }

    private List<Vector3> CalculatePositions() {
        float frontalStep = imageHeight * (100 - surveyArea.frontalOverlap) / 100;
        float sideStep = imageWidth * (100 - surveyArea.sideOverlap) / 100;
        Bounds areaToCover = surveyArea.areaToCover;

        Vector2 bottomLeft = new Vector2(areaToCover.center.x - areaToCover.extents.x,
            areaToCover.center.z - areaToCover.extents.z);
        Vector2 topRight = new Vector2(areaToCover.center.x + areaToCover.extents.x,
            areaToCover.center.z + areaToCover.extents.z);

        List<Vector3> dronePositions = new List<Vector3>();
        Vector2 pointInFlight = bottomLeft;
        int yDirection = 1;
        while (pointInFlight.x <= topRight.x) {
            while ((bottomLeft.y < pointInFlight.y || Mathf.Approximately(bottomLeft.y, pointInFlight.y)) &&
                (pointInFlight.y < topRight.y || Mathf.Approximately(pointInFlight.y, topRight.y))) {
                Vector3 dronePosition = new Vector3(pointInFlight.x, altitude, pointInFlight.y);
                dronePositions.Add(dronePosition);
                pointInFlight.y += frontalStep * yDirection;
            }
            yDirection *= -1;
            pointInFlight.x += sideStep;
            pointInFlight.y += frontalStep * yDirection;
        }

        return dronePositions;
    }
}