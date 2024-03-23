using static CastingResult;

public class TreeCrownDetection : Task<CastingResult, DetectionResult> {

    private const int ROWS_PER_STEP = 4;
    private DetectionResult detectionResult;
    private CastingResult castingResult;

    private int rowsProcessed = 0;

    public override string GetDescription() {
        return "Detecting individual trees...";
    }

    public override void TakeInput(CastingResult input) {
        this.castingResult = input;
        this.detectionResult = new DetectionResult(input.Width, input.Height);
        this.rowsProcessed = 0;
    }

    public override float ContinueProcessingAndReportProgress() {
        for (int i = 0; i < ROWS_PER_STEP && rowsProcessed < detectionResult.Height; i++, rowsProcessed++) {
            ProcessRow(rowsProcessed);
        }

        return rowsProcessed / (float) detectionResult.Height;
    }

    public override DetectionResult GetResult() {
        return detectionResult;
    }

    private void ProcessRow(int row) {
        for (int col = 0; col < castingResult.Width; col++) {
            RaycastResult result = castingResult[row, col];
            if (result != null && result.entityType == EntityType.TREE) {
                detectionResult.AssignPixelToTree(result.entityName, row, col, result.hitPoint.y);
            }
        }
    }

}
