using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public enum MinMaxNodeType { Min, Max }

[CreateNodeMenu("Logic/MinMax")]
public class MinMaxNode : BiomeNode {
    [Input] public Texture2D texA;
    [Input] public Texture2D texB;

    public MinMaxNodeType nodeType = MinMaxNodeType.Min;

    [Output] public Texture2D output;

    public override void OnInputChanged() {
        Texture2D texA = GetInputValue<Texture2D>("texA", this.texA);
        Texture2D texB = GetInputValue<Texture2D>("texB", this.texB);
        output = ApplyOperation(texA, texB);
        SendSignal(GetPort("output"));
    }

    public override object GetValue(NodePort port) {
        return output;
    }

    private Texture2D ApplyOperation(Texture2D texA, Texture2D texB) {
        if (texA == null || texB == null) {
            Debug.LogWarning("Textures are not set");
            return new Texture2D(0, 0);
        }
        if (texA.width != texB.width || texA.height != texB.height) {
            Debug.LogError("Width and height differ");
            return new Texture2D(0, 0);
        }

        int width = texA.width;
        int height = texA.height;
        Texture2D textureResult = new Texture2D(width, height);

        Color[] resultColors = new Color[width * height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float sampleA = texA.GetPixel(x, y).r;
                float sampleB = texB.GetPixel(x, y).r;
                float newVal = Mathf.Min(sampleA, sampleB);
                if (nodeType == MinMaxNodeType.Max) {
                    newVal = Mathf.Max(sampleA, sampleB);
                }
                resultColors[y * width + x] = new Color(newVal, newVal, newVal);
            }
        }

        textureResult.SetPixels(resultColors);
        textureResult.Apply();
        return textureResult;
    }

    public NodePort GetInputPort() {
        // CAREFUL: works because there's only one
        foreach (NodePort port in Inputs) {
            if (port != null) return port;
        }
        return null;
    }
}