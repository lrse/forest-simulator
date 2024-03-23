using UnityEngine;
using System.Collections;

[System.Serializable]
public class ConfigurableSurveyArea {

    [Tooltip("Object that we want to take images of. It will be centered in the flight plan")]
    public GameObject objectOfInterest;

    [Tooltip("Width of the area that we want to cover around the point of interest (meters)")]
    public int areaWidth = 300;

    [Tooltip("Distance of the area that we want to cover around the point of interest (meters)")]
    public int areaDistance = 200;

    [Tooltip("Determine if you want to map the center of the object of interest, or the 0,0")]
    public bool useCoordinateOrigin = false;

    [Tooltip("Percentage of overlap between images in the same pass (percentage)")]
    [Range(0, 100)]
    public int frontalOverlap = 85;

    [Tooltip("Percentage of overlap between images of consecutive pass(percentage)")]
    [Range(0, 100)]
    public int sideOverlap = 80;

    public SurveyArea BuildDomain() {
        return new SurveyArea(CalculateAreaToCover(), frontalOverlap, sideOverlap);
    }

    private Bounds CalculateAreaToCover() {
        // Try to find the center of the object of interest 
        Bounds objectsBounds;
        if (objectOfInterest.GetComponent<Collider>() != null) {
            objectsBounds = objectOfInterest.GetComponent<Collider>().bounds;
        } else if (objectOfInterest.GetComponent<Renderer>() != null) {
            objectsBounds = objectOfInterest.GetComponent<Renderer>().bounds;
        } else {
            objectsBounds = new Bounds(objectOfInterest.transform.position, objectOfInterest.transform.localScale);
        }

        Vector3 center = useCoordinateOrigin ? new Vector3(0, objectsBounds.center.y, 0) : objectsBounds.center;
        return new Bounds(center, new Vector3(areaWidth, objectsBounds.size.y, areaDistance));
    }
}
