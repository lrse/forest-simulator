using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Logic/And")]
public class AndNode : BiomeNode {
    [Input] public Texture2D texA;
    [Input] public Texture2D texB;

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
                resultColors[y * width + x] = texA.GetPixel(x, y) * texB.GetPixel(x, y);
			}
		}

		textureResult.SetPixels(resultColors);
		textureResult.Apply();
        return textureResult;
    }
}