using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static Georeferencing;
using static GroundControlPointLocator;

public class GroundControlPointManager {

    private const string FILE_NAME = "gcp_list.txt";
    private readonly CameraDefinition cameraDefinition;
    private readonly List<GroundControlPoint> gcps;
    private List<ImageAssociation> associations;

    public GroundControlPointManager(CameraDefinition cameraDefinition, SurveyArea surveyArea) {
        this.cameraDefinition = cameraDefinition;
        this.gcps = LocateGCPs(surveyArea);
        this.associations = new List<ImageAssociation>();
    }

    public void CheckForVisibleGCPs(Texture2D screenShot, Camera camera, string imageName) {
        foreach (GroundControlPoint gcp in gcps) {
            if (IsInSight(screenShot, camera, gcp)) {
                AddAssociation(imageName, gcp.position, camera);
            }
        }
    }

    public void WriteToFile(Georeferencing georeferencing, string folderPath) {
        if (associations.Count > 0) {
            using (StreamWriter file = new StreamWriter(Path.Combine(folderPath, FILE_NAME))) {
                file.WriteLine(georeferencing.GetDefinition());
                associations.Select(association => association.MapToUTMAndPrint(georeferencing))
                    .ToList()
                    .ForEach(file.WriteLine);
            }
        }
    }

    private bool IsInSight(Texture2D screenShot, Camera camera, GroundControlPoint gcp) {
        return IsGCPInCameraView(camera, gcp) && HasNothingInFront(screenShot, camera, gcp);
    }

    private bool HasNothingInFront(Texture2D screenShot, Camera camera, GroundControlPoint gcp) {
        // Here, we check if there is something between the camera and the gcp by comparing the color on the screenshow.
        // Another way could be to use Raycasting, but we would need to have good fitting colliders for trees in those cases, and it could be computationally expensive.
        Pixel pixel = WorldToPixel(camera, gcp.position);
        bool a = AreTheSameColor(gcp.color, screenShot.GetPixel(pixel.x, pixel.y));
        return a;
    }

    // This epsion was adjusted until the example worked. It might produce some false negatives.
    private const float epsilon = 0.008F;
    private bool AreTheSameColor(Color a, Color b) {
        Color.RGBToHSV(a, out float aHue, out _, out _);
        Color.RGBToHSV(b, out float bHue, out _, out _);
        // Check if they are close, and also check for the circular reds in HSV
        return Mathf.Abs(aHue - bHue) < epsilon ||
            (Mathf.Abs(aHue) < epsilon && Mathf.Abs(1 - bHue) < epsilon) ||
            (Mathf.Abs(bHue) < epsilon && Mathf.Abs(1 - aHue) < epsilon);
    }

    private bool IsGCPInCameraView(Camera camera, GroundControlPoint gcp) {
        // Here, we check if the gcp is actually in front of the camera
        Vector3 viewportPoint = camera.WorldToViewportPoint(gcp.position);
        return 0 <= viewportPoint.x && viewportPoint.x <= 1 && 0 <= viewportPoint.y && viewportPoint.y <= 1 && 0 <= viewportPoint.z;
    }

    private Pixel WorldToPixel(Camera camera, Vector3 position) {
        Vector3 screenPoint = camera.WorldToScreenPoint(position);
        return new Pixel(Mathf.RoundToInt(screenPoint.x), Mathf.RoundToInt(screenPoint.y));
    }

    private void AddAssociation(string imageName, Vector3 gcpPosition, Camera camera) {
        Pixel pixel = WorldToPixel(camera, gcpPosition);
        Pixel correctedPixel = pixel.InvertY(cameraDefinition.resolutionY);
        associations.Add(new ImageAssociation(imageName, gcpPosition, correctedPixel));
    }
    
    class ImageAssociation {

        private readonly string imageName;
        private readonly Vector3 gcp;
        private readonly Pixel pixel;

        internal ImageAssociation(string imageName, Vector3 gcp, Pixel pixel) {
            this.imageName = imageName;
            this.gcp = gcp;
            this.pixel = pixel;
        }

        public string MapToUTMAndPrint(Georeferencing georeferencing) {
            UTMCoordinate coordinate = georeferencing.MapUnityToUTM(gcp);
            return string.Format("{0} {1} {2} {3} {4} {5}", coordinate.Easting, coordinate.Northing, coordinate.Altitude, pixel.x, pixel.y, imageName);
        }
    }

    class Pixel {

        public readonly int x, y;

        public Pixel(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Pixel InvertY(int imageHeight) {
            return new Pixel(x, imageHeight - y);
        }
    }
}