using UnityEngine;
using static TreeInstantiator;

public class TreeInstantiator : Task<Void, TreeInstantiationResult> {

    private const string TREE_CONTAINER = "Tree Container";
    private const int TREES_PER_STEP = 100;

    private readonly Terrain terrain;
    private readonly Bounds bounds;
    private readonly GameObject treeContainer;
    private int createdTrees;

    private bool includeTerrainTrees;

    public TreeInstantiator(Terrain terrain, int areaWidth, int areaDistance, bool includeTerrainTrees) {
        this.terrain = terrain;
        this.bounds = CalculateBounds(terrain, areaWidth, areaDistance);
        this.treeContainer = FindOrCreateTreeContainer();
        this.createdTrees = 0;
        this.includeTerrainTrees = includeTerrainTrees;

    }

    public override string GetDescription() {
        return "Converting trees from the terrain to objects...";
    }

    public override void TakeInput(Void input) { }

    public override float ContinueProcessingAndReportProgress() {
        TerrainData data = terrain.terrainData;
        float width = data.size.x;
        float distance = data.size.z;
        float altitude = data.size.y;
        
        if (data.treeInstanceCount == 0 || includeTerrainTrees == false) { Debug.Log("There are no trees in the terrain."); return 1; }
        
        // Create trees
        for (int treesInStep = 0; treesInStep < TREES_PER_STEP && createdTrees < data.treeInstanceCount; treesInStep++, createdTrees++) {
            TreeInstance tree = data.treeInstances[createdTrees];
            if (tree.prototypeIndex >= data.treePrototypes.Length)
                continue;
            var _tree = data.treePrototypes[tree.prototypeIndex].prefab;
            Vector3 position = new Vector3(
                tree.position.x * width,
                tree.position.y * altitude,
                tree.position.z * distance) + terrain.transform.position;
            Vector3 scale = new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);
            Bounds treeBounds = new Bounds(position, scale);
            if (bounds.Intersects(treeBounds)) {
                GameObject go = Object.Instantiate(_tree, position, Quaternion.Euler(0f, Mathf.Rad2Deg * tree.rotation, 0f), treeContainer.transform) as GameObject;
                go.name = "Tree " + createdTrees;
                //RemoveUnnecessaryChildren(go);
                Vector3 aux = Vector3.Scale(go.transform.localScale, scale);
                go.transform.localScale = aux;
                go.isStatic = true;
            }


        }

        if (createdTrees == data.treeInstanceCount) {
            AddMeshCollidersToTrees();
        }
        
        return createdTrees / (float) data.treeInstanceCount;
    }

    public override TreeInstantiationResult GetResult() {
        return new TreeInstantiationResult(bounds, treeContainer);
    }

    private static Bounds CalculateBounds(Terrain terrain, int areaWidth, int areaDistance) {
        // Try to find the center of the object of interest 
        Bounds terrainBounds;
        if (terrain.GetComponent<Collider>() != null) {
            terrainBounds = terrain.GetComponent<Collider>().bounds;
        } else if (terrain.GetComponent<Renderer>() != null) {
            terrainBounds = terrain.GetComponent<Renderer>().bounds;
        } else {
            terrainBounds = new Bounds(terrain.transform.position, terrain.transform.localScale);
        }

        return new Bounds(terrainBounds.center, new Vector3(areaWidth, terrainBounds.size.y, areaDistance));
    }


    private void AddMeshCollidersToTrees() {
        // Destroy all previous colliders
        foreach (Collider collider in treeContainer.GetComponentsInChildren<Collider>()) {
            Object.DestroyImmediate(collider);
        }

        foreach (Transform child in treeContainer.transform) {
            // Assign a Mesh Collider to each tree
            LODGroup[] lodGroups = child.GetComponentsInChildren<LODGroup>();
            if (lodGroups.Length > 0) {
                LOD lod = lodGroups[0].GetLODs()[0];
                MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = false;
                meshCollider.sharedMesh = lod.renderers[0].GetComponent<MeshFilter>().sharedMesh;
            }
        }
    }

    private static GameObject FindOrCreateTreeContainer() {
        GameObject treeContainer = GameObject.Find(TREE_CONTAINER);
        if (treeContainer == null) {
            treeContainer = new GameObject(TREE_CONTAINER);
        }
        treeContainer.isStatic = true;
        return treeContainer;
    }

    private static void RemoveUnnecessaryChildren(GameObject tree) {
        Transform treeTransform = tree.transform;
        for (int i = treeTransform.childCount - 1; i >= 0; i--) {
            Transform child = treeTransform.GetChild(i);
            if (child.name.StartsWith("Collision") || child.name.EndsWith("LOD1") || child.name.EndsWith("LOD2")) {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    public class TreeInstantiationResult : ITaskIO {

        public readonly Bounds bounds;
        public readonly GameObject treeContainer;

        public TreeInstantiationResult(Bounds bounds, GameObject treeContainer) {
            this.bounds = bounds;
            this.treeContainer = treeContainer;
        }

    }
}
