using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

// Reference paper: https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
[CreateNodeMenu("Samplers/Static Poisson"), ShowPreview(false)]
public class StaticPoissonDiskSampler : BiomeNode, IRandomNode, ISizeNode {
    public float radius;
    public int sampleRejection = 30;
    public int sampleLimit = 10000;
    public int seed;
    public int size = 256;

    [Output] public List<Vector2> output;

    private System.Random prng;

    public override object GetValue(NodePort port) {
        output = GeneratePoints();
        return output;
    }

    public List<Vector2> GeneratePoints() {
        prng = new System.Random(seed);
        // Step 0. Initialize data structures.
        float cellSize = radius / Mathf.Sqrt(2);

        int height = Mathf.CeilToInt(size / cellSize);
        int width = Mathf.CeilToInt(size / cellSize);
        Vector2[,] grid = new Vector2[height, width];

        List<Vector2> points = new List<Vector2>();
        List<Vector2> active = new List<Vector2>();

        // Step 1. Select initial sample, x_0, randomly chosen uniformly from the domain.
        Vector2 x0 = new Vector2(prng.Next(0, size), prng.Next(0, size));
        grid[(int)(x0.x / cellSize), (int)(x0.y / cellSize)] = x0;
        points.Add(x0);
        active.Add(x0);

        while (active.Count > 0)
        {
            if (points.Count > sampleLimit) break;
            int index = prng.Next(0, active.Count);
            Vector2 center = active[index];
            bool found = false;

            for (int i = 0; i < sampleRejection; i++)
            {
                // Step 2: Generate up to k points chosen uniformly from the spherical annulus
                // between radius r and 2r around x_i. In this case x_i is the spawn center.
                // Find a suitable candidate or discard the active point as a suitable generator.
                Vector2 xi = GetRandomPointInAnnulus(prng, center, radius);
                if (IsValidPoint(xi, cellSize, radius, grid))
                {
                    points.Add(xi);
                    active.Add(xi);
                    grid[(int)(xi.x / cellSize), (int)(xi.y / cellSize)] = xi;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                active.RemoveAt(index);
            }
        }

        return points;
    }

    private Vector2 GetRandomPointInAnnulus(System.Random prng, Vector2 center, float radius) {
        float angle = (float)prng.NextDouble() * Mathf.PI * 2;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 point = center + dir * Mathf.Lerp(radius, 2 * radius, (float)prng.NextDouble());
        return point;
    }

    private bool IsValidPoint(Vector2 candidate, float cellSize, float radius, Vector2[,] grid) {
        if (candidate.x < 0 || candidate.x >= size || candidate.y < 0 || candidate.y >= size)
        {
            return false;
        }

        // We check a 5x5 neighborhood.
        int cellX = (int)(candidate.x / cellSize);
        int cellY = (int)(candidate.y / cellSize);
        int startX = Mathf.Max(0, cellX - 2);
        int endX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
        int startY = Mathf.Max(0, cellY - 2);
        int endY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector2 point = grid[x, y];
                if (point != Vector2.zero && (((candidate - point).sqrMagnitude) < (radius * radius)))
                {
                    // If there's a point and it's inside the radius, then the candidate
                    // is no good.
                    return false;
                }
            }
        }
        return true;
    }

    public void SetSeed(int seed) {
        this.seed = seed;
    }

    public void SetSize(int size) {
        this.size = size;
    }

    public override void OnInputChanged() {
        // Does nothing but propagate signal
        SendSignal(GetPort("output"));
    }
}
