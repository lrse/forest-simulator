using UnityEngine;
using System.Collections.Generic;

public abstract class Mission {

    protected readonly MissionType missionType;
    protected readonly SurveyArea surveyArea;
    protected readonly CameraDefinition cameraDefinition;

    protected Mission(MissionType missionType, SurveyArea surveyArea, CameraDefinition cameraDefinition) {
        this.missionType = missionType;
        this.surveyArea = surveyArea;
        this.cameraDefinition = cameraDefinition;
    }

    public FlightPlan CalculateFlightPlan() {
        return new FlightPlan(CalculateWaypoints(), missionType);
    }

    protected abstract List<Waypoint> CalculateWaypoints();

    public enum MissionType {
        Grid,
        DoubleGrid
    }

    public struct FlightPlan {

        public FlightPlan(List<Waypoint> waypoints, MissionType missionType) {
            this.waypoints = waypoints;
            this.missionType = missionType;
        }

        public readonly List<Waypoint> waypoints;
        public readonly MissionType missionType;

    }

    public struct Waypoint {

        public Waypoint(Vector3 dronePosition, Quaternion droneRotation) {
            this.dronePosition = dronePosition;
            this.droneRotation = droneRotation;
        }

        public readonly Vector3 dronePosition;
        public readonly Quaternion droneRotation;
    }

}