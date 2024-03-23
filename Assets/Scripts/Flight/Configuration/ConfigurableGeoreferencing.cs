using UnityEngine;

[System.Serializable]
public class ConfigurableGeoreferencing {

    public enum Hemisphere {
        North,
        South
    }

    [Tooltip("The geometric point (in UTM) where the images will be tagged against.")]
    public double centerEasting = 277524.5597564102;
    [Tooltip("The geometric point (in UTM) where the images will be tagged against.")]
    public double centerNorthing = 5485401.149023914;

    [Tooltip("The UTM zone where the point will is.")]
    public int utmZone = 19;

    [Tooltip("The hemisphere where the point is.")]
    public Hemisphere utmHemisphere = Hemisphere.South;

    [Tooltip("Specifies how acurrante the GPS coordinates embedded in the images are. 1 implies highest acurracy possible and 20 implies discardable")]
    [Range(1, 20)]
    public float dilutionOfPrecision = 10;

    [Tooltip("Specify amount of noise to add to the GPS coordinates. A random vector of the size 'noise' will be added to the real coordinates. Set to 0 to avoid adding noise.")]
    public int noise = 0;

    [Tooltip("The seed to be used when generating the noise.")]
    public int noiseRandomSeed = 0;


    public Georeferencing BuildDomain(SurveyArea surveyArea) {
        return new Georeferencing(centerEasting, centerNorthing, utmZone, utmHemisphere == Hemisphere.North ? Georeferencing.Hemisphere.North : Georeferencing.Hemisphere.South, dilutionOfPrecision, noiseRandomSeed, noise, surveyArea);
    }
}