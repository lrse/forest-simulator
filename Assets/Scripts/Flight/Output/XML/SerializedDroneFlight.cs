using UnityEngine;
using System.Xml.Serialization;
using System.Collections.Generic;
using System;
using System.Linq;

[XmlRoot("DroneFlight")]
[Serializable]
public class SerializedDroneFlight {

    public SerializedCameraDefinition cameraDefinition;
    public SerializedSurveyArea surveyArea;
    public SerializedGeoreferencing georeferencing;
    public SerializedFlightPlan flightPlan;

    public SerializedDroneFlight(CameraDefinition cameraDefinition, SurveyArea surveyArea, Mission.FlightPlan flightPlan, Georeferencing georeferencing) {
        this.cameraDefinition = new SerializedCameraDefinition(cameraDefinition);
        this.surveyArea = new SerializedSurveyArea(surveyArea);
        this.flightPlan = new SerializedFlightPlan(flightPlan);
        this.georeferencing = new SerializedGeoreferencing(georeferencing);
    }

    public SerializedDroneFlight() { }

    public class SerializedCameraDefinition {

        public float focalLength;
        public float sensorSizeX;
        public float sensorSizeY;
        public int resolutionX;
        public int resolutionY;

        public SerializedCameraDefinition(CameraDefinition cameraDefinition) {
            this.focalLength = cameraDefinition.focalLength;
            this.sensorSizeX = cameraDefinition.sensorSizeX;
            this.sensorSizeY = cameraDefinition.sensorSizeY;
            this.resolutionX = cameraDefinition.resolutionX;
            this.resolutionY = cameraDefinition.resolutionY;
        }

        public SerializedCameraDefinition() { }
    }

    public class SerializedSurveyArea {

        public float areaWidth;
        public float areaDistance;
        public int frontalOverlap;
        public int sideOverlap;

        public SerializedSurveyArea(SurveyArea surveyArea) {
            this.areaWidth = surveyArea.areaToCover.size.x;
            this.areaDistance = surveyArea.areaToCover.size.z;
            this.frontalOverlap = surveyArea.frontalOverlap;
            this.sideOverlap = surveyArea.sideOverlap;
        }

        public SerializedSurveyArea() { }
    }

    public class SerializedFlightPlan {

        public Mission.MissionType missionType;
        public string distanceCovered;
        public List<SerializedWaypoints> waypoints;

        public SerializedFlightPlan(Mission.FlightPlan flightPlan) {
            this.waypoints = flightPlan.waypoints.Select(waypoint => new SerializedWaypoints(waypoint)).ToList();
            this.missionType = flightPlan.missionType;

            double distanceCovered = 0;
            Vector3[] positions = flightPlan.waypoints.Select(waypoint => waypoint.dronePosition).ToArray();
            for (int i = 1; i < positions.Length; i++) {
                distanceCovered += Vector3.Distance(positions[i - 1], positions[i]);
            }
            this.distanceCovered = Math.Round(distanceCovered, 2) + "m";
        }

        public SerializedFlightPlan() { }
    }

    public class SerializedWaypoints {

        public Vector3 dronePosition;
        public Vector3 droneRotation;

        public SerializedWaypoints(Mission.Waypoint waypoint) {
            this.dronePosition = waypoint.dronePosition;
            this.droneRotation = waypoint.droneRotation.eulerAngles;
        }

        public SerializedWaypoints() { }
    }


    public class SerializedGeoreferencing {

        public double eastingCenter, northingCenter;
        public int utmZone;
        public string utmHemisphere;

        public SerializedGeoreferencing(Georeferencing georeferencing) {
            this.eastingCenter = georeferencing.centerEasting;
            this.northingCenter = georeferencing.centerNorthing;
            this.utmZone = georeferencing.utmZone;
            this.utmHemisphere = georeferencing.hemisphere.Value;
        }

        public SerializedGeoreferencing() { }
    }
}