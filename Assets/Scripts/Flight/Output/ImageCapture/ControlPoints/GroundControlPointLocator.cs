using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GroundControlPointLocator {
    
    public static List<GroundControlPoint> LocateGCPs(SurveyArea surveyArea) {
        // For now, we are looking for manually placed GCPs. In the future, it might make sense to locate them automatically. 
        // Maybe using Voronoi diagrams?

        return GameObject.FindGameObjectsWithTag("GCP")
            .Select(GroundControlPoint.FromGameObject)
            .Where(gcp => gcp.IsContainedByBound(surveyArea.areaToCover))
            .ToList();
    }

    public class GroundControlPoint {

        public readonly Vector3 position;
        public readonly Color color;

        private GroundControlPoint(Vector3 position, Color color) {
            this.position = position;
            this.color = color;
        }

        public bool IsContainedByBound(Bounds bounds) {
            return bounds.Contains(position);
        }

        public static GroundControlPoint FromGameObject(GameObject gcp) {
            return new GroundControlPoint(gcp.transform.position, gcp.GetComponent<Renderer>().material.color);
        }

    }
}