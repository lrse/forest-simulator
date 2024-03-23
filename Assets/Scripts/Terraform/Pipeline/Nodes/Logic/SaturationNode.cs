using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Logic/Saturation")]
public class SaturationNode : BiomeNode {
    [Input] public Texture2D tex;

    public float minXVal = 0;
    public float maxXVal = 1;

    [Output] public Texture2D output;

    public override void OnInputChanged() {
        tex = GetInputValue<Texture2D>("tex", this.tex);
        output = ApplyOperation(tex);
        SendSignal(GetPort("output"));
    }

    public override object GetValue(NodePort port) {
        return output;
    }

    private Texture2D ApplyOperation(Texture2D tex) {
        if (tex == null) {
            Debug.LogWarning("Texture is not set");
            return new Texture2D(0, 0);
        }
        int width = tex.width;
        int height = tex.height;

        Texture2D textureResult = new Texture2D(width, height);
        Color[] resultColors = new Color[width * height];
        Color ones = new Color(1, 1, 1, 2);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Color sample = tex.GetPixel(x, y);
                float newVal = new float();
                if (sample.r < minXVal) { newVal = 0; } else if (sample.r > maxXVal) { newVal = 1; } else {
                    float a = 1 / (maxXVal - minXVal);
                    float b = -minXVal / (maxXVal - minXVal);
                    newVal = a * sample.r + b;
                }
                resultColors[x + y * height] = new Color(newVal, newVal, newVal);
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