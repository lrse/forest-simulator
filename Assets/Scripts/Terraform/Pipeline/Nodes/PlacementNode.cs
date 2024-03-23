using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using XNode;

[ShowPreview(false)]
public class PlacementNode : BiomeNode, IRandomNode {

    [Input] public List<Vector2> spawnPointsOS;
    [Input] public Texture2D spawnTexture;
    public int seed;
    public bool enableSpawnTextureUsage;
    public bool enableScaleRandomizer;
    public bool enableTwistRandomizer;
    public bool enableSpinRandomizer;
    public Vector2 scaleFactorMinMax;
    public GameObject[] prefabs;
    [Range(0, 90)] public float maxNormalAngle = 2;

    public override object GetValue(NodePort port) { return null; }

    public override void OnInputChanged() { }

    public void Place(string parent, CustomTerrain customTerrain, SurfacePointToWorldPos localToWorldPos) {
        System.Random prng = new System.Random(seed);
        spawnPointsOS = GetInputValue<List<Vector2>>("spawnPointsOS", this.spawnPointsOS);

        Debug.Assert(spawnPointsOS != null, "There are no spawning points for objects");
        Debug.Assert(prefabs != null, "There are no prefabs to instantiate");

        // Generate spawn points in world pos
        List<Vector3> spawnPointsWS = spawnPointsOS.ConvertAll<Vector3>(point => localToWorldPos(point));

        GameObject parentContainer = customTerrain.TerrainObject.GetOrCreateGameObjectByName(parent);
        parentContainer.transform.DestroyImmediateChildren();

        // Spawn new objects according to probability map
        for (int i = 0; i < spawnPointsWS.Count; i++) {
            Vector2 spawnPoint = spawnPointsOS[i];
            Vector3 spawnPointWS = spawnPointsWS[i];

            float probabilityOfSpawning = 1;
            if (enableSpawnTextureUsage) {
                spawnTexture = GetInputValue<Texture2D>("spawnTexture", this.spawnTexture);
                probabilityOfSpawning = spawnTexture.GetPixel(Mathf.FloorToInt(spawnPoint.x), Mathf.FloorToInt(spawnPoint.y)).r;
            }
            float coinToss = (float)prng.NextDouble();

            if (coinToss < probabilityOfSpawning) {
                // Randomizers
                int prefabIdx = prng.Next(0, prefabs.Length - 1);
                float scale = Mathf.Lerp(scaleFactorMinMax.x, scaleFactorMinMax.y, (float)prng.NextDouble());
                float twistTowardsAngle = Mathf.Lerp(0, 2 * Mathf.PI, (float)prng.NextDouble());
                float twistNormalAngle = Mathf.Lerp(0, maxNormalAngle, (float)prng.NextDouble());
                float spinAngle = Mathf.Lerp(0, Mathf.PI * 2, (float)prng.NextDouble());

                // Instantiate
                GameObject t = Instantiate(prefabs[prefabIdx], spawnPointWS, Quaternion.identity);

                // Twist and spin
                Vector3 twistTowardsDir = new Vector3(Mathf.Cos(twistTowardsAngle), 0f, Mathf.Sin(twistTowardsAngle));
                Vector3 twistAxis = Vector3.Cross(Vector3.up, twistTowardsDir);
                Quaternion twist = Quaternion.AngleAxis(twistNormalAngle, twistAxis.normalized);
                Quaternion spin = Quaternion.AngleAxis(spinAngle * Mathf.Rad2Deg, (twist * Vector3.up).normalized);

                // Apply transformations
                if (enableScaleRandomizer) {
                    t.transform.localScale *= scale;
                }

                Quaternion rotation = Quaternion.identity;
                if (enableSpinRandomizer) {
                    rotation *= spin;
                }
                if (enableTwistRandomizer) {
                    rotation *= twist;
                }
                t.transform.rotation = rotation;
                t.transform.parent = parentContainer.transform;
            }
        }
    }

    public void SetSeed(int seed) {
        this.seed = seed;
    }
}
