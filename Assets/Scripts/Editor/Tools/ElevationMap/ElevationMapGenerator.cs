using static CastingResult;

public class ElevationMapGenerator : Task<CastingResult, ElevationMap> {

    private CastingResult castingResult;
    private ElevationMap elevationMap;
    private int rowsMapped = 0;

    public override string GetDescription() {
        return "Creating elevation map based on the casting results...";
    }

    public override void TakeInput(CastingResult input) {
        this.castingResult = input;
        this.elevationMap = new ElevationMap(input.Width, input.Height);
    }

    public override float ContinueProcessingAndReportProgress() {
        for (int col = 0; col < elevationMap.Width; col++) {
            RaycastResult raycastResult = castingResult[rowsMapped, col];
            if (raycastResult != null) {
                elevationMap[rowsMapped, col] = raycastResult.hitPoint.y;
            }
        }
        rowsMapped++;
        return rowsMapped / (float) elevationMap.Width;
    }

    public override ElevationMap GetResult() {
        return elevationMap;
    }

}
