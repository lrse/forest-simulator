using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using static CastingResult;

public class Segmentation : Task<CastingResult, Void> {

    private const string IMAGE_NAME = "segmentation.jpg";
    private const string XML_NAME = "segmentation.xml";

    private readonly Dictionary<EntityType, Color> colors;
    private readonly Color noHitColor;
    private readonly float GSD;
    private readonly bool exportArray;
    private readonly string folderPath;

    private CastingResult castingResult;

    public Segmentation(Dictionary<EntityType, Color> colors, Color noHitColor, float GSD, bool exportArray, string folderPath) {
        this.colors = colors;
        this.noHitColor = noHitColor;
        this.GSD = GSD;
        this.exportArray = exportArray;
        this.folderPath = folderPath;
    }

    public override string GetDescription() {
        return "Doing segmentation...";
    }

    public override void TakeInput(CastingResult input) {
        this.castingResult = input;
    }

    public override float ContinueProcessingAndReportProgress() {
        Texture2D image = new Texture2D(castingResult.Width, castingResult.Height, TextureFormat.RGB24, false);
        int[,] array = new int[castingResult.Height, castingResult.Width];

        for (int row = 0; row < image.height; row++) {
            for (int col = 0; col < image.width; col++) {
                RaycastResult result = castingResult[row, col];
                if (result == null) {
                    image.SetPixel(col, row, noHitColor);
                    array[image.height - 1 - row, col] = -1;
                } else {
                    image.SetPixel(col, row, colors[result.entityType]);
                    array[image.height - 1 - row, col] = (int) result.entityType;
                }
            }
        }

        // Apply all SetPixel calls
        image.Apply();
        byte[] data = image.EncodeToJPG();
        File.WriteAllBytes(Path.Combine(folderPath, IMAGE_NAME), data);

        // Export XML
        ExportXml(array);

        return 1F;
    }

    public override Void GetResult() {
        return Void.INSTANCE;
    }

    private void ExportXml(int[,] array) {
        using (XmlWriter writer = XmlWriter.Create(Path.Combine(folderPath, XML_NAME))) {
            writer.WriteStartDocument();
            writer.WriteStartElement("Segmentation");
            WriteProperties(writer);
            if (exportArray) {
                WriteCellsInNumpyFormat(writer, array);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }

    private void WriteProperties(XmlWriter writer) {
        writer.WriteStartElement("Properties");
        writer.WriteElementString("GSD", GSD.ToString());
        writer.WriteElementString("Width", castingResult.Width.ToString());
        writer.WriteElementString("Height", castingResult.Height.ToString());
        writer.WriteEndElement();
    }

    private void WriteCellsInNumpyFormat(XmlWriter writer, int[,] array) {
        string[] rows = new string[array.GetLength(0)];
        for (int i = 0; i < array.GetLength(0); i++) {
            int[] columns = new int[array.GetLength(1)];
            for (int j = 0; j < array.GetLength(1); j++) {
                columns[j] = array[i, j];
            }
            rows[i] = string.Join(",", columns);
        }
        writer.WriteStartElement("Map");
        writer.WriteStartElement("Values");
        writer.WriteElementString("Nothing", "-1");
        foreach (EntityType entityType in Enum.GetValues(typeof(EntityType))) {
            writer.WriteElementString(entityType.ToString(), Convert.ToString((int)entityType));
        }
        writer.WriteEndElement();
        writer.WriteElementString("ArrayInNumpyFormat", string.Join("\n", rows));
        writer.WriteEndElement();
    }

}
