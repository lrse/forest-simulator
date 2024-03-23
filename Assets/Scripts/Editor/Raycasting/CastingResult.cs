using UnityEngine;

public class CastingResult : ITaskIO {

    public int Width { get; private set; }
    public int Height { get; private set; }
    public long Size { get; private set; }
    private RaycastResult[,] raycastResults;

    public CastingResult(int width, int height) {
        this.Width = width;
        this.Height = height;
        this.Size = width * height;
        raycastResults = new RaycastResult[height, width];
    }

    public void AddResult(int row, int col, RaycastHit raycastHit) {
        EntityType entityType = raycastHit.collider is TerrainCollider ? EntityType.TERRAIN : EntityType.TREE;
        raycastResults[row, col] = new RaycastResult(raycastHit.point, entityType, raycastHit.transform.name);
    }

    public RaycastResult this[int row, int col] {
        get {
            return raycastResults[row, col];
        }
    }

    public class RaycastResult {

        public readonly Vector3 hitPoint;
        public readonly EntityType entityType;
        public readonly string entityName;

        public RaycastResult(Vector3 hitPoint, EntityType entityType, string entityName) {
            this.hitPoint = hitPoint;
            this.entityType = entityType;
            this.entityName = entityName;
        }

    }

    public enum EntityType {
        TERRAIN,
        TREE
    }
}