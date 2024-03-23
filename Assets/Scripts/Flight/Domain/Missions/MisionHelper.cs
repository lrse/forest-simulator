using UnityEngine;
using System.Collections;

internal class MissionHelper {

    internal static Quaternion CalculateRotationBetweenWaypoints(Vector3 from, Vector3 to, float cameraAngle) {
        Vector3 direction = to - from;
        return Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(cameraAngle, Vector3.right);
    }

}
