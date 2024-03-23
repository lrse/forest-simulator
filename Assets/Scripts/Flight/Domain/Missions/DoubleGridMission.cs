using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class DoubleGridMission : Mission {

    private readonly float altitude1, altitude2;
    private readonly float cameraAngle1, cameraAngle2;

    public DoubleGridMission(SurveyArea surveyArea, CameraDefinition cameraDefinition, float relativeAltitude1, float cameraAngle1, float relativeAltitude2, float cameraAngle2)
        : base(MissionType.DoubleGrid, surveyArea, cameraDefinition) {
        this.altitude1 = surveyArea.areaToCover.center.y - surveyArea.areaToCover.extents.y  + relativeAltitude1;
        this.altitude2 = surveyArea.areaToCover.center.y - surveyArea.areaToCover.extents.y  + relativeAltitude2;
        this.cameraAngle1 = cameraAngle1;
        this.cameraAngle2 = cameraAngle2;
    }

    protected override List<Waypoint> CalculateWaypoints() {
        List<Waypoint> firstGridWaypoints = CalculateWaypoints(altitude1, cameraAngle1, CalculateFirstGridWaypoints);
        List<Waypoint> secondGridWaypoints = CalculateWaypoints(altitude2, cameraAngle2, CalculateSecondGridWaypoints);

        firstGridWaypoints.AddRange(secondGridWaypoints);

        return firstGridWaypoints;
    }

    private List<Waypoint> CalculateWaypoints(float altitude, float cameraAngle, Func<float, float, float, Vector2, Vector2, List<Vector3>> calculator) {
        float imageWidth = altitude * cameraDefinition.sensorSizeX / cameraDefinition.focalLength;
        float imageHeight = altitude * cameraDefinition.sensorSizeY / cameraDefinition.focalLength;
        float frontalStep = imageHeight * (100 - surveyArea.frontalOverlap) / 100;
        float sideStep = imageWidth * (100 - surveyArea.sideOverlap) / 100;
        Bounds areaToCover = surveyArea.areaToCover;

        Vector2 bottomLeft = new Vector2(areaToCover.center.x - areaToCover.extents.x,
            areaToCover.center.z - areaToCover.extents.z);
        Vector2 topRight = new Vector2(areaToCover.center.x + areaToCover.extents.x,
            areaToCover.center.z + areaToCover.extents.z);

        List<Vector3> waypoints = calculator(altitude, frontalStep, sideStep, bottomLeft, topRight);
        return ConverPositionsToWaypoints(waypoints, cameraAngle);
    }

    private List<Waypoint> ConverPositionsToWaypoints(List<Vector3> positions, float cameraAngle) {
        List<Waypoint> waypoints = new List<Waypoint>();
        for (int i = 0; i < positions.Count - 1; i++) {
            Vector3 current = positions[i];
            Vector3 next = positions[i + 1];
            waypoints.Add(new Waypoint(current, MissionHelper.CalculateRotationBetweenWaypoints(current, next, cameraAngle)));
        }

        waypoints.Add(new Waypoint(positions[positions.Count - 1], waypoints[waypoints.Count - 1].droneRotation));
        return waypoints;
    }

    private List<Vector3> CalculateFirstGridWaypoints(float altitude, float frontalStep, float sideStep, Vector2 bottomLeft, Vector2 topRight) {
        List<Vector3> dronePositions = new List<Vector3>();
        Vector2 pointInFlight = bottomLeft;
        int yDirection = 1;
        while (pointInFlight.x <= topRight.x) {
            while ((bottomLeft.y < pointInFlight.y || Mathf.Approximately(bottomLeft.y, pointInFlight.y)) &&
                (pointInFlight.y < topRight.y || Mathf.Approximately(pointInFlight.y, topRight.y))) {
                Vector3 dronePosition = new Vector3(pointInFlight.x, altitude1, pointInFlight.y);
                dronePositions.Add(dronePosition);
                pointInFlight.y += frontalStep * yDirection;
            }
            yDirection *= -1;
            pointInFlight.x += sideStep;
            pointInFlight.y += frontalStep * yDirection;
        }

        return dronePositions;
    }

    private List<Vector3> CalculateSecondGridWaypoints(float altitude, float frontalStep, float sideStep, Vector2 bottomLeft, Vector2 topRight) {
        List<Vector3> dronePositions = new List<Vector3>();
        Vector2 pointInFlight = topRight;
        int xDirection = -1;
        while(pointInFlight.y >= bottomLeft.y) {
            while ((bottomLeft.x < pointInFlight.x || Mathf.Approximately(bottomLeft.x, pointInFlight.x)) &&
                (pointInFlight.x < topRight.x || Mathf.Approximately(pointInFlight.x, topRight.x))) {
                Vector3 dronePosition = new Vector3(pointInFlight.x, altitude2, pointInFlight.y);
                dronePositions.Add(dronePosition);
                pointInFlight.x += frontalStep * xDirection;
            }
            xDirection *= -1;
            pointInFlight.x += frontalStep * xDirection;
            pointInFlight.y -= sideStep;
        }

        return dronePositions;
    }
}