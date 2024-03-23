using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Samplers/Variable Poisson"), ShowPreview(false)]
public class VariablePoissonSamplerNode : BiomeNode, IRandomNode, ISizeNode {
    public float minRadius = 1;
    public float maxRadius = 5;
    public int sampleRejection = 30;
    public int sampleLimit = 10000;
    public int seed;
    public int size = 256;

    private System.Random prng;

    [Input] public Texture2D densityTexture;
    [Output] public List<Vector2> output;

    public override void OnInputChanged() {
        // Does nothing but propagate signal
        SendSignal(GetPort("output"));
    }

    public override object GetValue(NodePort port) {
        densityTexture = GetInputValue<Texture2D>("densityTexture", this.densityTexture);
        output = densityTexture != null ? GeneratePoints(densityTexture) : null;
        return output;
    }

    public List<Vector2> GeneratePoints(Texture2D densityMap) {
        prng = new System.Random(seed);
        SpatialGrid sampleGrid = new SpatialGrid(size, size, minRadius);
        List<Vector2> points = new List<Vector2>();
        List<Vector2> active = new List<Vector2>();

        Vector2 x0 = GetRandomPointInDomain();
        sampleGrid.AddItem(x0, GetRadiusAt(x0, densityMap));
        points.Add(x0);
        active.Add(x0);

        while (active.Count > 0) {
            if (points.Count > sampleLimit) break;

            int index = prng.Next(0, active.Count);
            Vector2 center = active[index];
            bool found = false;

            float radius = GetRadiusAt(center, densityMap);
            for (int i = 0; i < sampleRejection; i++) {
                Vector2 xi = GetRandomPointInAnnulus(prng, center, radius);
                if (sampleGrid.IsValidPos(xi, radius)) {
                    points.Add(xi);
                    active.Add(xi);
                    sampleGrid.AddItem(xi, radius);
                    found = true;
                    break;
                }
            }

            if (!found) {
                active.RemoveAt(index);
            }
        }

        return points;

    }

    Vector2 GetRandomPointInAnnulus(System.Random prng, Vector2 pos, float radius) {
        float angle = (float)prng.NextDouble() * Mathf.PI * 2;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 point = pos + dir * radius;
        return point;
    }

    Vector2 GetRandomPointInDomain() {
        return new Vector2(prng.Next(0, size), prng.Next(0, size));
    }

    float GetRadiusAt(Vector2 pos, Texture2D densityMap) {
        return minRadius + Mathf.Lerp(
            minRadius, maxRadius,
            densityMap.GetPixelBilinear(pos.x / densityMap.width, pos.y / densityMap.height).r
        );
    }

    public void SetSeed(int seed) {
        this.seed = seed;
    }

    public void SetSize(int size) {
        this.size = size;
    }
}