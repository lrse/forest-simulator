using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Logic/Lerp")]
public class LerpNode : BiomeNode {
    [Input] public Texture2D tex;

    public float minVal = 0;
    public float maxVal = 1;

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
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Color sample = tex.GetPixel(x, y);
                float newVal = Mathf.Lerp(minVal, maxVal, sample.r);
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