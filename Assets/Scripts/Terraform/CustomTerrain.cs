using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTerrain {
    int size;
    private GameObject terrainGameObject;
    private Material material;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh mesh;
    private CustomTerrainData terrainData;
    private float[,] heightMap;

    // Getters
    public GameObject TerrainObject { get => terrainGameObject; }
    public float[,] TerrainHeightMap { get => heightMap; }
    public Mesh TerrainMesh { get => mesh; }

    public CustomTerrain(int size, Material terrainMaterial, CustomTerrainData terrainData) {
        this.material = terrainMaterial;
        this.size = size;
        this.terrainData = terrainData;
    }

    public GameObject CreateTerrainObject() {
        GameObject go = new GameObject(Constants.TerrainName);
        go.transform.position = Vector3.zero;

        meshFilter = go.AddComponent<MeshFilter>();
        meshRenderer = go.AddComponent<MeshRenderer>();
        meshCollider = go.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        heightMap = GenerateHeightMap();
        mesh = GenerateMesh();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        go.tag = Constants.TerrainTag;
        go.layer = LayerMask.NameToLayer(Constants.TerrainTag);
        terrainGameObject = go;

        return go;
    }

    public void SetData(CustomTerrainData terrainData) {
        this.terrainData = terrainData;
    }

    private float[,] GenerateHeightMap() {
        if (terrainData != null) {
            return Noise.GenerateNoiseMap(size, terrainData.seed, terrainData.scale, terrainData.octaves, terrainData.persistance, terrainData.lacunarity, terrainData.offset);
        }
        return new float[0,0];
    }

    private Mesh GenerateMesh() {
        if (heightMap != null && terrainData != null) {
            return MeshGenerator.GenerateTerrainMesh(heightMap, terrainData.heightMultiplier, terrainData.heightCurve).CreateMesh();
        }
        return new Mesh();
    }

    public Vector3 SurfacePointToWorldPos(Vector2 point) {
        float maxHeight = mesh.bounds.max.y + 10;
        float dst = Mathf.Abs(mesh.bounds.max.y - mesh.bounds.min.y) + 20;
        Vector3 offset = new Vector3(-mesh.bounds.size.x, 0, -mesh.bounds.size.z) * 0.5f;
        Vector3 projectPoint = new Vector3(point.x, maxHeight, point.y) + offset;
        RaycastHit hit;
        int mask = 1 << LayerMask.NameToLayer(Constants.TerrainTag);

        if (Physics.Raycast(projectPoint, Vector3.down, out hit, dst, mask)) {
            return hit.point;
        }
        // hmmmm...
        return hit.point;
    }
}
