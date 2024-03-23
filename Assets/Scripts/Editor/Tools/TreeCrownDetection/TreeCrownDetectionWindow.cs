using UnityEngine;
using UnityEditor;

public class TreeCrownDetectionWindow : TerrainToolWindow {

    private float GSD = 20;
    private int areaWidth = 200, areaDistance = 200;
    private bool includeTerrainTrees = true;

    [MenuItem("Window/Terrain Tools/Tree Crown Detection")]
    static void Init() {
        TreeCrownDetectionWindow window = (TreeCrownDetectionWindow)GetWindow(typeof(TreeCrownDetectionWindow));
    }

    protected override void LoadFields() {
        GSD = EditorGUILayout.FloatField("GSD:", GSD);
        areaWidth = EditorGUILayout.IntField("Area Width:", areaWidth);
        areaDistance = EditorGUILayout.IntField("Area Distance:", areaDistance);
    }

    protected override string GetButtonText() {
        return "Detect tree crowns";
    }

    protected override TaskList GetTaskList(Terrain terrain, string folderPath) {
        return TaskList.From(new TreeInstantiator(terrain, areaDistance, areaDistance, includeTerrainTrees))
            .With(new Raycaster(GSD))
            .With(new TreeCrownDetection())
            .With(new TreeCrownDrawing(folderPath));
    }
}