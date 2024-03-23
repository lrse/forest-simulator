using System.Xml.Serialization;
using System.IO;

public class XMLWriter {
    
    public static void WriteToXml(CameraDefinition cameraDefinition, SurveyArea surveyArea, Mission.FlightPlan flightPlan, Georeferencing georeferencing, string folderPath) {
        SerializedDroneFlight droneFlight = new SerializedDroneFlight(cameraDefinition, surveyArea, flightPlan, georeferencing);
        XmlSerializer serializer = new XmlSerializer(typeof(SerializedDroneFlight));
        TextWriter writer = new StreamWriter(Path.Combine(folderPath, "droneFlight.xml"));
        serializer.Serialize(writer, droneFlight);
        writer.Close();
    }
}