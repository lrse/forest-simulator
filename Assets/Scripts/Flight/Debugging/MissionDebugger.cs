using UnityEngine;
using static Mission;

public class MissionDebugger {

    private static readonly Color COMPLETED = Color.yellow;
    private static readonly Color CURRENT = Color.green;
    private static readonly Color AREA = Color.blue;

    private readonly CameraDefinition cameraDefinition;
    private readonly SurveyArea surveyArea;
    private readonly FlightPlan flightPlan;
    private readonly Transform droneTransform;
    
    public MissionDebugger(CameraDefinition cameraDefinition, SurveyArea surveyArea, Mission mission, Transform droneTransform) {
        this.cameraDefinition = cameraDefinition;
        this.surveyArea = surveyArea;
        this.flightPlan = mission.CalculateFlightPlan();
        this.droneTransform = droneTransform;
    }

    public void DrawDebug(int pointInPlan) { 
        for (int i = 0; i < Mathf.Min(pointInPlan, flightPlan.waypoints.Count); i++) {
            DrawCompleted(flightPlan.waypoints[i]);
        }
        if (pointInPlan > 0) {
            DrawCurrent();
        }
        DrawArea();
    }

    /** Draw the area that should be covered by the drone flight */
    private void DrawArea() {
        DrawBox(CalculateBoxFromBounds(surveyArea.areaToCover), AREA);
    }

    /** Draw the current area being covered by the drone */
    private void DrawCurrent() {
        (Vector3, Vector3, Vector3, Vector3) box = CalculateBoxFromPositionAndRotation(droneTransform.position, droneTransform.rotation);
        DrawBox(box, CURRENT);
    }

    /** Draw the given flight steps as completed */
    private void DrawCompleted(Waypoint waypoint) {
        (Vector3, Vector3, Vector3, Vector3) box = CalculateBoxFromPositionAndRotation(waypoint.dronePosition, waypoint.droneRotation);
        DrawBox(box, COMPLETED);
        Debug.DrawLine(box.Item1, box.Item3, COMPLETED);
        Debug.DrawLine(box.Item2, box.Item4, COMPLETED);
    }

    /** Calculate the box being covered from the given position and rotation */
    private (Vector3, Vector3, Vector3, Vector3) CalculateBoxFromPositionAndRotation(Vector3 position, Quaternion rotation) {
        float relativeAltitude = position.y - surveyArea.areaToCover.center.y + surveyArea.areaToCover.extents.y;
        float imageWidth = relativeAltitude * cameraDefinition.sensorSizeX / cameraDefinition.focalLength;
        float imageHeight = relativeAltitude * cameraDefinition.sensorSizeY / cameraDefinition.focalLength;
        Bounds imageBounds = new Bounds(Vector3.zero, new Vector3(imageWidth, 0, imageHeight));
        (Vector3, Vector3, Vector3, Vector3) box = CalculateBoxFromBounds(imageBounds);
        Quaternion fixedRotation = Quaternion.Euler(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
        Vector3 fixedPosition = new Vector3(position.x, surveyArea.areaToCover.center.y, position.z);
        Matrix4x4 matrix = Matrix4x4.TRS(fixedPosition, fixedRotation, Vector3.one);

        return (matrix.MultiplyPoint(box.Item1), matrix.MultiplyPoint(box.Item2), matrix.MultiplyPoint(box.Item3), matrix.MultiplyPoint(box.Item4));
    }

    /** Calculate the box from the given bounds */
    private (Vector3, Vector3, Vector3, Vector3) CalculateBoxFromBounds(Bounds bounds) {
        // Set the altitude of the grid to the point between the drone and the actual bounds
        bounds = new Bounds(new Vector3(bounds.center.x, (bounds.center.y - bounds.extents.y + droneTransform.position.y) / 2, bounds.center.z), 
            new Vector3(bounds.size.x, 0, bounds.size.z));

        Vector3 bottomLeft = bounds.center - bounds.extents;
        Vector3 topRight = bounds.center + bounds.extents;
        Vector3 topLeft = new Vector3(bottomLeft.x, bottomLeft.y, topRight.z);
        Vector3 bottomRight = new Vector3(topRight.x, bottomLeft.y, bottomLeft.z);
        return (bottomLeft, topLeft, topRight, bottomRight);
    }

    /** Draw the given box with the given color*/
    private void DrawBox((Vector3, Vector3, Vector3, Vector3) box, Color color) {
        Debug.DrawLine(box.Item1, box.Item2, color);
        Debug.DrawLine(box.Item2, box.Item3, color);
        Debug.DrawLine(box.Item3, box.Item4, color);
        Debug.DrawLine(box.Item4, box.Item1, color);
    }
}
