using System;
using System.IO;
using System.Diagnostics;
using static Mission;
using static Georeferencing;

public class GeoTagger  {

    private readonly CameraDefinition cameraDefinition;
    private readonly Georeferencing georeferencing;

    public GeoTagger(CameraDefinition cameraDefinition, Georeferencing georeferencing) {
        this.cameraDefinition = cameraDefinition;
        this.georeferencing = georeferencing;
    }

    public void TagImage(Waypoint waypoint, string imagePath) {
        UTMCoordinate utmCoordinate = georeferencing.MapUnityToUTM(waypoint.dronePosition);
        LatLngUTMConverter.LatLng latlng = LatLngUTMConverter.ConvertUtmToLatLng(utmCoordinate.Easting, utmCoordinate.Northing, georeferencing.utmZone, georeferencing.hemisphere.Value);
        string latitudeRef = georeferencing.hemisphere.Value;
        string longitudeRef = georeferencing.utmZone <= 30 ? "W" : "E";
        LongLatCoordidnates longLatCoord = new LongLatCoordidnates(latlng.Lng, latlng.Lat, utmCoordinate.Altitude, latitudeRef, longitudeRef);
        GeoTagImage(longLatCoord, imagePath);
    }

    private void GeoTagImage(LongLatCoordidnates coordinate, string imagePath) {
        using (Process process = new Process()) {
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "../../ExifTool/Image-ExifTool-12.42/exiftool");
            process.StartInfo.Arguments = string.Format("-overwrite_original -GPSLatitude={0} -GPSLatitudeRef={1} -GPSLongitude={2} -GPSLongitudeRef={3} -GPSAltitude={4} -GPSAltitudeRef=\"Above Sea Level\" -GPSDOP={5} -FocalLength={6} \"{7}\""
                , Math.Abs(coordinate.latitude), coordinate.latitudeRef, Math.Abs(coordinate.longitude), coordinate.longitudeRef, coordinate.altitude, georeferencing.dilutionOfPrecision, cameraDefinition.focalLength, imagePath);
            process.Start();
        }
    }

    struct LongLatCoordidnates {

        internal LongLatCoordidnates(double longitude, double latitude, double altitude, string latitudeRef, string longitudeRef) {
            this.longitude = longitude;
            this.latitude = latitude;
            this.altitude = altitude;
            this.latitudeRef = latitudeRef;
            this.longitudeRef = longitudeRef;
        }

        internal readonly double longitude, latitude, altitude;
        internal readonly string latitudeRef, longitudeRef;
    }

}
