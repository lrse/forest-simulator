using UnityEngine;

[RequireComponent(typeof(ProceduralGrassRenderer))]
public class Terraformer : MonoBehaviour {

    // General properties
    public Material terrainMaterial;
    public int size = 256;
    public int seed = 0;
    public Pipeline[] pipelines;
    public bool useGeneralSeed = true;

    // Land properties
    public CustomTerrainData terrainData;
    private GameObject terrainObject;
    private ProceduralGrassRenderer grassRenderer;

    // Export
    public int exportTerrainPoints = 1000000;
    public bool exportTerrain;
    public bool exportGrass;
    public bool exportPipelines;

    // Private properties
    private CustomTerrain customTerrain;
    [HideInInspector] public bool terrainDataFoldout;

    public void GenerateTerrain() {
        terrainObject = terrainObject ? terrainObject : GameObject.Find(Constants.TerrainName);
        if (terrainObject != null) { 
            DestroyImmediate(terrainObject);
        }
        if (terrainMaterial == null) {
            terrainMaterial = Resources.Load<Material>(Constants.TerrainMaterial);
        }
        Debug.Assert(terrainData != null, "Terrain data is null");
        Debug.Assert(terrainMaterial != null, "Terrain material is null");
        if (useGeneralSeed) terrainData.seed = seed;
        customTerrain = new CustomTerrain(size, terrainMaterial, terrainData);
        terrainObject = customTerrain.CreateTerrainObject();
    }

    public void GenerateGrass() {
        grassRenderer = GetComponent<ProceduralGrassRenderer>();
        grassRenderer.Initialize();
        grassRenderer.Dispatch();
    }

    public void RunPipelines() {
        GenerateTerrain();
        foreach (Pipeline pipe in pipelines) {
            if (useGeneralSeed) pipe.SetSeed(seed);
            pipe.SetSize(size);
            SurfacePointToWorldPos fx = p => customTerrain.SurfacePointToWorldPos(p);
            pipe.Place(customTerrain, fx);
        }
    }

    public void ExportScene() {
        if (grassRenderer == null) grassRenderer = GetComponent<ProceduralGrassRenderer>();
        using (Exporter e = new Exporter()) {
            if (exportTerrain) e.WriteVertices(GeneratePointsOnTerrain(), "Terrain");
            if (exportPipelines) {
                foreach (Pipeline pipe in pipelines) {
                    e.WriteVertices(pipe.GetInstantiatedObjects());
                }
            }
            if (exportGrass) e.WriteVertices(
                positions: grassRenderer.GetGrassVertices(),
                tag: "Grass"
            );
        }
    }

    private Vector3[] GeneratePointsOnTerrain() {
        terrainObject = terrainObject ? terrainObject : GameObject.Find(Constants.TerrainName);
        if (terrainObject == null) {
            Debug.LogWarning("Terrain does not exist");
            return new Vector3[0];
        }

        Bounds bounds = terrainObject.GetComponent<MeshFilter>().sharedMesh.bounds;
        Vector2[] pointsOS = new Vector2[exportTerrainPoints];

        Random.State stateBefore = Random.state;
        Random.InitState(seed);
        for (int i = 0; i < exportTerrainPoints; i++) {
            float x = Random.Range(0, bounds.extents.x * 2);
            float z = Random.Range(0, bounds.extents.z * 2);
            pointsOS[i] = new Vector2(x, z);
        }
        Random.state = stateBefore;

        RaycastHit[] hits = ParallelRaycast.PointsToSurface(pointsOS, Constants.TerrainLayer, bounds);
        Vector3[] pointsWS = new Vector3[hits.Length];
        for (int i = 0; i < hits.Length; i++) {
            if (hits[i].collider != null) {
                pointsWS[i] = hits[i].point;
            }
        }
        return pointsWS;
    }
}

public delegate Vector3 SurfacePointToWorldPos(Vector2 pos);
