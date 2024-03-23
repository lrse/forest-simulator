using UnityEngine;
using static Mission;

public class DroneMovement {

    private readonly Transform drone;
    private readonly FlightPlan flightPlan;
    private readonly float speed;
    private int step = 0;

    public delegate void WaypointReachedEvent(Waypoint waypoint);
    public delegate void FinalWaypointReachedEvent();
    public event WaypointReachedEvent OnReachedWaypoint;
    public event FinalWaypointReachedEvent OnReachedFinalWaypoint;

    public DroneMovement(Transform drone, FlightPlan flightPlan, float speed) {
        this.drone = drone;
        this.flightPlan = flightPlan;
        this.speed = speed;
    }

    public void MoveAndRotateDrone() {
        if (step >= 0) {
            if (step < flightPlan.waypoints.Count) {
                Waypoint waypoint = flightPlan.waypoints[step];
                Vector3 targetPosition = waypoint.dronePosition;

                if (IsDroneAlreadyThere(waypoint.dronePosition)) {
                    Quaternion targetRotation = waypoint.droneRotation;
                    if (IsDroneDoneRotating(targetRotation)) {
                        ReachedWaypoint();
                    } else {
                        // Continue rotating
                        drone.rotation = Quaternion.RotateTowards(drone.rotation, targetRotation, speed * Time.deltaTime);
                    }
                } else {
                    if (step == 0) {
                        // Move faster if we are getting to where we want to be
                        drone.position = Vector3.MoveTowards(drone.position, targetPosition, 100 * Time.deltaTime);
                        drone.LookAt(targetPosition);
                    } else {
                        // Continue moving towards next waypoint
                        drone.position = Vector3.MoveTowards(drone.position, targetPosition, speed * Time.deltaTime);
                    }
                }
            } else {
                ReachedFinalWaypoint();
            }
        }
    }

    private void ReachedWaypoint() {
        Waypoint waypoint = flightPlan.waypoints[step];
        // Fix the position and rotation (there could be a very little deviation)
        drone.position = waypoint.dronePosition;
        drone.rotation = waypoint.droneRotation;
        OnReachedWaypoint?.Invoke(waypoint);
        // Set the next waypoint
        step++;
    }

    private void ReachedFinalWaypoint() {
        step = -1;
        OnReachedFinalWaypoint?.Invoke();
    }

    private bool IsDroneDoneRotating(Quaternion targetRotation) {
        return Quaternion.Angle(drone.rotation, targetRotation) < 1;
    }

    private bool IsDroneAlreadyThere(Vector3 position) {
        return Vector3.Distance(drone.position, position) <= 1;
    }
}
