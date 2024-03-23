using UnityEngine;
using System.IO;
using System.Xml;

public class ElevationMapExporter : Task<ElevationMap, Void> {

    private const string IMAGE_NAME = "elevation_map.jpg";
    private const string XML_NAME = "elevation_map.xml";


    private readonly float GSD;
    private readonly string folderPath;
    private ElevationMap elevationMap;
    private int step = 0;

    public ElevationMapExporter(float GSD, string folderPath) {
        this.GSD = GSD;
        this.folderPath = folderPath;
    }

    public override string GetDescription() {
        return "Exporting the elevation map as XML and JPG...";
    }

    public override void TakeInput(ElevationMap input) {
        this.elevationMap = input;
    }

    public override float ContinueProcessingAndReportProgress() {
        if (step == 0) {
            ExportGreyscaleImage();
            step++;
            return 0.5F;
        } else {
            ExportXml();
            return 1;
        }
    }

    public override Void GetResult() {
        return Void.INSTANCE;
    }


    private void ExportGreyscaleImage() {
        Texture2D image = new Texture2D(elevationMap.Width, elevationMap.Height, TextureFormat.RGB24, false);
        for (int y = 0; y < image.height; y++) {
            for (int x = 0; x < image.width; x++) {
                float elevation = elevationMap[y, x];
                if (elevation == elevationMap.NO_VALUE) {
                    image.SetPixel(x, y, Color.black);
                } else {
                    float adjusted = elevation / elevationMap.MaxElevation;
                    Color color = new Color(adjusted, adjusted, adjusted);
                    image.SetPixel(x, y, color);
                }
            }
        }

        // Apply all SetPixel calls
        image.Apply();
        byte[] data = image.EncodeToJPG();
        File.WriteAllBytes(Path.Combine(folderPath, IMAGE_NAME), data);
    }

    private void ExportXml() {
        using (XmlWriter writer = XmlWriter.Create(Path.Combine(folderPath, XML_NAME))) {
            writer.WriteStartDocument();
            writer.WriteStartElement("ElevationMap");
            WriteProperties(writer);
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }

    private void WriteProperties(XmlWriter writer) {
        writer.WriteStartElement("Properties");
        writer.WriteElementString("GSD", GSD.ToString());
        writer.WriteElementString("Width", elevationMap.Width.ToString());
        writer.WriteElementString("Height", elevationMap.Height.ToString());
        writer.WriteElementString("MaxElevation", elevationMap.MaxElevation.ToString());
        writer.WriteElementString("MinElevation", elevationMap.MinElevation.ToString());
        writer.WriteElementString("NoValue", elevationMap.NO_VALUE.ToString());
        writer.WriteEndElement();
    }    

}
