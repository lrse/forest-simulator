using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class TreeCrownDrawing : Task<DetectionResult, Void> {

    private const int TOP_SIZE = 10;
    private static readonly Color FLOOR = new Color(174 / 255f, 144 / 255f, 107 / 255f);
    private static readonly Color BORDER = Color.black;
    private static readonly Color TREE = new Color(34 / 255f, 139 / 255f, 34 / 255f);
    private static readonly Color TOP = Color.red;

    private const string IMAGE_NAME = "tree_crowns.jpg";

    private readonly string folderPath;

    private int width, height;
    private Color[] image;
    private List<DetectionResult.Tree> trees;
    private int treesDrawn = 0;

    public TreeCrownDrawing(string folderPath) {
        this.folderPath = folderPath;
    }

    public override string GetDescription() {
        return "Drawing tree crowns...";
    }

    public override void TakeInput(DetectionResult input) {
        this.trees = input.GetTrees();
        this.image = new Color[input.Width * input.Height];
        this.width = input.Width;
        this.height = input.Height;
        // Initialize the image marking everything as floor
        for (int i = 0; i < image.Length; i++) {
            image[i] = FLOOR;
        }
    }

    public override float ContinueProcessingAndReportProgress() {
        if (treesDrawn < trees.Count ) {
            DrawTree(trees[treesDrawn]);
            treesDrawn++;
        }

        if (treesDrawn == trees.Count) {
            // Apply all SetPixel calls
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.SetPixels(image);
            texture.Apply();
            byte[] data = texture.EncodeToJPG();
            File.WriteAllBytes(Path.Combine(folderPath, IMAGE_NAME), data);
        }

        return treesDrawn / (float) trees.Count;
    }

    public override Void GetResult() {
        return Void.INSTANCE;
    }

    private void DrawTree(DetectionResult.Tree tree) {
        foreach ((int, int) pixel in tree.GetAssignedPixels()) {
            int row = pixel.Item1;
            int col = pixel.Item2;
            if (tree.IsPixelBorder(row, col)) {
                SetColorInImage(row, col, BORDER);
            } else {
                SetColorInImage(row, col, TREE);
            }
        }

        for (int row = Mathf.Max(0, tree.HighestAltitudeRow - TOP_SIZE / 2); row < Mathf.Min(tree.HighestAltitudeRow + TOP_SIZE / 2, height); row++) {
            for (int col = Mathf.Max(0, tree.HighestAltitudeCol - TOP_SIZE / 2); col < Mathf.Min(tree.HighestAltitudeCol + TOP_SIZE / 2, width); col++) {
                SetColorInImage(row, col, TOP);
            }
        }
    }

    private void SetColorInImage(int row, int col, Color color) {
        image[(row * width) + col] = color;
    }
}
