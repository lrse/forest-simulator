using UnityEngine;
using UnityEditor;

public class ElevationMapWindow : TerrainToolWindow {

    private bool includeTerrainTrees = true;
    private float GSD = 20;
    private int areaWidth = 200, areaDistance = 200;    

    [MenuItem("Window/Terrain Tools/Elevation Map")]
    static void Init() {
        ElevationMapWindow window = (ElevationMapWindow)GetWindow(typeof(ElevationMapWindow));
    }

    protected override void LoadFields() {
        includeTerrainTrees = EditorGUILayout.Toggle("Include Terrain Trees:", includeTerrainTrees);
        GSD = EditorGUILayout.FloatField("GSD:", GSD);
        areaWidth = EditorGUILayout.IntField("Area Width:", areaWidth);
        areaDistance = EditorGUILayout.IntField("Area Distance:", areaDistance);        
    }

    protected override string GetButtonText() {
        return "Generate Elevation Map";
    }

    protected override TaskList GetTaskList(Terrain terrain, string folderPath) {
        return TaskList.From(new TreeInstantiator(terrain, areaDistance, areaDistance, includeTerrainTrees))
            .With(new Raycaster(GSD))
            .With(new ElevationMapGenerator())
            .With(new ElevationMapExporter(GSD, folderPath));
    }
}