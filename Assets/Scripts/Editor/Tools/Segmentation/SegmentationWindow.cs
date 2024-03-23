using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static CastingResult;

public class SegmentationWindow : TerrainToolWindow {

    private float GSD = 20;
    private int areaWidth = 200, areaDistance = 200;
    private bool includeTerrainTrees = true;
    private Color noHitColor;
    private Dictionary<EntityType, Color> colors = new Dictionary<EntityType, Color> {
        { EntityType.TERRAIN, new Color(174 / 255f, 144 / 255f, 107 / 255f) },
        { EntityType.TREE, new Color(34 / 255f, 139 / 255f, 34 / 255f) }};
    private bool exportArrayInXML = true;

    [MenuItem("Window/Terrain Tools/Segmentation")]
    static void Init() {
        SegmentationWindow window = (SegmentationWindow)GetWindow(typeof(SegmentationWindow));
    }

    protected override void LoadFields() {
        GSD = EditorGUILayout.FloatField("GSD:", GSD);
        areaWidth = EditorGUILayout.IntField("Area Width:", areaWidth);
        areaDistance = EditorGUILayout.IntField("Area Distance:", areaDistance);
        EditorGUILayout.LabelField("", "Please indicate the colors to use:");
        EntityType[] entityTypes = new EntityType[colors.Keys.Count];
        colors.Keys.CopyTo(entityTypes, 0);
        foreach (EntityType entityType in entityTypes) {
            colors[entityType] = EditorGUILayout.ColorField(entityType.ToString(), colors[entityType]);
        }
        noHitColor = EditorGUILayout.ColorField("Nothing there", Color.black);
        exportArrayInXML = EditorGUILayout.Toggle("Export map array in XML:", exportArrayInXML);
    }

    protected override string GetButtonText() {
        return "Generate Segmentation";
    }

    protected override TaskList GetTaskList(Terrain terrain, string folderPath) {
        return TaskList.From(new TreeInstantiator(terrain, areaDistance, areaDistance, includeTerrainTrees))
            .With(new Raycaster(GSD))
            .With(new Segmentation(colors, noHitColor, GSD, exportArrayInXML, folderPath));
    }
}