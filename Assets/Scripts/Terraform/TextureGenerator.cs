using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height) {
        Texture2D t = new Texture2D(width, height);

        // Decide el comportamiento de los bordes de cada pixel
        t.filterMode = FilterMode.Point;

        // Decide el comportamiento de los bordes de la textura (wrapping)
        t.wrapMode = TextureWrapMode.Clamp;

        t.SetPixels(colorMap);
        t.Apply();
        return t;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colourMap = new Color[width * height];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColorMap(colourMap, width, height);
    }

    public static void SaveTextureAsPNG(Texture2D texture) {
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/Data";
        if (!System.IO.Directory.Exists(dirPath)) {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        System.IO.File.WriteAllBytes(dirPath + "/texture_" + Random.Range(0, 100000) + ".png", bytes);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
