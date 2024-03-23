public class CameraDefinition {

    public readonly float focalLength;
    public readonly float sensorSizeX;
    public readonly float sensorSizeY;
    public readonly int resolutionX;
    public readonly int resolutionY;

    public CameraDefinition(float focalLength, float sensorSizeX, float sensorSizeY, int resolutionX, int resolutionY) {
        this.focalLength = focalLength;
        this.sensorSizeX = sensorSizeX;
        this.sensorSizeY = sensorSizeY;
        this.resolutionX = resolutionX;
        this.resolutionY = resolutionY;
    }
}
