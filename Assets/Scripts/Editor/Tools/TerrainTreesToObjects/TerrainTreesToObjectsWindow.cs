using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainTreesToObjectsWindow : TerrainToolWindow
{
    private int areaWidth = 200, areaDistance = 200;
    private bool includeTerrainTrees = true;    

    [MenuItem("Window/Terrain Tools/Convert Terrain Trees to Objects")]
    static void Init() {
        TerrainTreesToObjectsWindow window = (TerrainTreesToObjectsWindow)GetWindow(typeof(TerrainTreesToObjectsWindow));
    }

    protected override void LoadFields() {
        areaWidth = EditorGUILayout.IntField("Area Width:", areaWidth);
        areaDistance = EditorGUILayout.IntField("Area Distance:", areaDistance);
    }
    
    protected override string GetButtonText() {
        return "Convert Terrain Trees To Tree Objects";
    }

    protected override TaskList GetTaskList(Terrain terrain, string folderPath) {
        return TaskList.From(new TreeInstantiator(terrain, areaWidth, areaDistance, includeTerrainTrees));
    }
}
