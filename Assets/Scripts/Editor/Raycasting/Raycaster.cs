 using UnityEngine;
using static TreeInstantiator;

public class Raycaster : Task<TreeInstantiationResult, CastingResult> {
 
    private readonly float GSDInMeters;

    private TreeInstantiationResult treeInstantiationResult;
    private CastingResult result;
    private int rowsCalculated;

    public Raycaster(float GSD) {
        this.GSDInMeters = GSD / 100;
        this.rowsCalculated = 0;
    }

    public override string GetDescription() {
        return "Raycasting the terrain to find hit points...";
    }

    public override void TakeInput(TreeInstantiationResult input) {
        this.treeInstantiationResult = input;
        this.result = new CastingResult(Mathf.FloorToInt(input.bounds.size.x / GSDInMeters), Mathf.FloorToInt(input.bounds.size.z / GSDInMeters));
    }

    public override float ContinueProcessingAndReportProgress() {
        int width = Mathf.FloorToInt(treeInstantiationResult.bounds.size.x / GSDInMeters);
        int height = Mathf.FloorToInt(treeInstantiationResult.bounds.size.z / GSDInMeters);
        for (int col = 0; col < width; col++) {
            float z = treeInstantiationResult.bounds.center.z - treeInstantiationResult.bounds.extents.z + rowsCalculated * GSDInMeters;
            float x = treeInstantiationResult.bounds.center.x - treeInstantiationResult.bounds.extents.x + col * GSDInMeters;
            Vector3 rayOrigin = new Vector3(x, treeInstantiationResult.bounds.center.y + treeInstantiationResult.bounds.size.y + 100, z);
            bool wasHit = Physics.SphereCast(rayOrigin, GSDInMeters, Vector3.down, out RaycastHit hitInfo);
            if (wasHit) {
                result.AddResult(rowsCalculated, col, hitInfo);
            }
        }
        rowsCalculated++;
        if (rowsCalculated == height) {
            DestroyTreeContainer();
        }

        return rowsCalculated / (float) height;
    }

    public override CastingResult GetResult() {
        return result;
    }

    private void DestroyTreeContainer() {
        Object.DestroyImmediate(treeInstantiationResult.treeContainer);
    }
    
}
