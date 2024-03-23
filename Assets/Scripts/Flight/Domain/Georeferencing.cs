using UnityEngine;

public class Georeferencing {

    public class Hemisphere {
        private Hemisphere(string value) { Value = value; }

        public string Value { get; private set; }

        public static Hemisphere North { get { return new Hemisphere("N"); } }
        public static Hemisphere South { get { return new Hemisphere("S"); } }
    }

    public readonly double centerEasting, centerNorthing;
    public readonly int utmZone;
    public readonly Hemisphere hemisphere;
    public readonly float dilutionOfPrecision;
    private readonly Vector3 center;
    private readonly int noise;

    public Georeferencing(double centerEasting, double centerNorthing, int utmZone, Hemisphere hemisphere, float dilutionOfPrecision, int noiseRandomSeed, int maxNoise, SurveyArea surveyArea) {
        this.centerEasting = centerEasting;
        this.centerNorthing = centerNorthing;
        this.utmZone = utmZone;
        this.hemisphere = hemisphere;
        this.dilutionOfPrecision = dilutionOfPrecision;
        Random.InitState(noiseRandomSeed);
        this.noise = maxNoise;
        this.center = surveyArea.areaToCover.center;
    }

    public string GetDefinition() {
        return string.Format("WGS84 UTM {0}{1}", utmZone, hemisphere.Value);
    }

    public UTMCoordinate MapUnityToUTM(Vector3 position) {
        Vector3 positionWithNoise = position + Random.onUnitSphere * noise;
        double easting = centerEasting + (positionWithNoise.x - center.x);
        double northing = centerNorthing + (positionWithNoise.z - center.z);
        double altitude = positionWithNoise.y;
        return new UTMCoordinate(easting, northing, altitude);
    }

    public class UTMCoordinate {
        public double Easting { get; private set; }
        public double Northing { get; private set; }
        public double Altitude { get; private set; }

        internal UTMCoordinate(double easting, double northing, double altitude) {
            this.Easting = easting;
            this.Northing = northing;
            this.Altitude = altitude;
        }
    }

}
