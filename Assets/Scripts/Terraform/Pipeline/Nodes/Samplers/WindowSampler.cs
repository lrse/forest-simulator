using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class WindowSampler : BiomeNode, ISizeNode {
    public int size = 256;
    public int window = 16;
    public int maxBladesPerTile = 256;

    [Input] public Texture2D densityTexture;
    [Output] public List<Vector2> output;

    public List<Vector2> GeneratePoints() {
        List<Vector2> spawnPoints = new List<Vector2>();
        float density;
        for (int i = 0; i < size; i += window) {
            for (int j = 0; j < size; j += window) {
                density = GetDensityForWindow(i, j);
                foreach (Vector2 point in GetPointsForWindow(i, j, density)) {
                    spawnPoints.Add(point);
                }
            }
        }

        return spawnPoints;
    }

    public override void OnInputChanged() {
        SendSignal(GetPort("output"));
    }

    public override object GetValue(NodePort port) {
        densityTexture = GetInputValue<Texture2D>("densityTexture", this.densityTexture);
        output = densityTexture != null ? GeneratePoints() : null;
        return output;
    }

    float GetDensityForWindow(int i, int j) {
        float density = 0;
        for (int u = 0; u < window; u++) {
            for (int v = 0; v < window; v++) {
                density += densityTexture.GetPixel(i + u, j + v).r;
            }
        }
        return density / (window * window);
    }

    List<Vector2> GetPointsForWindow(int i, int j, float density) {
        List<Vector2> points = new List<Vector2>();
        if (density == 0) return points;
        float gridSize = (float)window / Mathf.FloorToInt(Mathf.Sqrt(maxBladesPerTile * density));

        if (gridSize < 0.0001f) {
            Debug.LogWarning("Grid size is too small");
            return points;
        }
        for (float x = 0; x < window; x += gridSize) {
            for (float y = 0; y < window; y += gridSize) {
                Vector2 point = new Vector2(i + x, j + y);
                points.Add(point);
            }
        }

        return points;
    }

    public void SetSize(int size) {
        this.size = size;
    }
}
