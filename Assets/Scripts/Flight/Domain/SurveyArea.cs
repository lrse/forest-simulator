using UnityEngine;

public class SurveyArea {

    public readonly Bounds areaToCover;
    public readonly int frontalOverlap;
    public readonly int sideOverlap;

    public SurveyArea(Bounds areaToCover, int frontalOverlap, int sideOverlap) {
        this.areaToCover = areaToCover;
        this.frontalOverlap = frontalOverlap;
        this.sideOverlap = sideOverlap;
    }

}
