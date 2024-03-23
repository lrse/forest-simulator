using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using UnityEngine;
using XNode;

public enum SourceNodeType { FractalNoise, Texture, Pipeline, SourceNode, VoronoiNoise }

public class SourceNode : BiomeNode, IRandomNode, ISizeNode {

    public SourceNodeType sourceType;
    public Texture2D texture;
    public Pipeline pipeline;
    public SourceNode sourceNode;
    public NoiseData noiseData;

    public float scale;
    [MinAttribute(0)] public int octaves = 4;
    [Range(0, 1)] public float persistance = 0.25f;
    [MinAttribute(0)] public float lacunarity = 0.25f;
    public int seed;
    public Vector2 offset;
    public int centroids;
    public int size = 256;

    [Output] public Texture2D output;

    // To redraw inspector
    [HideInInspector] public bool dirty = false;

    protected override void Init() {
        // This is called as part of the "OnEnable" of nodes. We only need
        // source nodes to propagate the signal downstream to have references
        // of output variables of the rest of the nodes in play mode.
        OnInputChanged();
    }

    public override object GetValue(NodePort port) {
        if (output == null) {
            output = GetTexture();
        }
        return output;
    }

    public override void OnInputChanged() {
        output = GetTexture();
        SendSignal(GetPort("output"));
    }

    public Texture2D GetTexture() =>
        sourceType switch {
            SourceNodeType.Texture => texture ? texture : new Texture2D(size, size),
            SourceNodeType.Pipeline => pipeline ? pipeline.GetFinalTexture() : new Texture2D(size, size),
            SourceNodeType.SourceNode => sourceNode ? sourceNode.GetTexture() : new Texture2D(size, size),
            SourceNodeType.FractalNoise => GetTextureFromNoise(SourceNodeType.FractalNoise),
            SourceNodeType.VoronoiNoise => GetTextureFromNoise(SourceNodeType.VoronoiNoise),
            _ => new Texture2D(size, size)
        };

    private Texture2D GetTextureFromNoise(SourceNodeType type) {
        float[,] noiseMap = new float[size, size];
        if (type == SourceNodeType.FractalNoise) {
            noiseMap = Noise.GenerateNoiseMap(size, seed, scale, octaves, persistance, lacunarity, offset);
        } else if (type == SourceNodeType.VoronoiNoise) {
            noiseMap = Noise.GenerateVoronoiNoiseMap(size, centroids, seed);
        }
        return TextureGenerator.TextureFromHeightMap(noiseMap);
    }

    public void SetSeed(int seed) {
        this.seed = seed;
    }

    public void SetSize(int size) {
        this.size = size;
    }
}