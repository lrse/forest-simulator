using UnityEngine;
using UnityEditor;
using Codice.CM.Common;

[CustomEditor(typeof(Terraformer))]
[CanEditMultipleObjects]
public class TerraformerEditor : Editor {
    Terraformer terraformer;

    // General
    SerializedProperty terrainMaterial;
    SerializedProperty size;
    SerializedProperty pipelines;
    SerializedProperty seed;
    SerializedProperty useGeneralSeed;

    // Exporter
    SerializedProperty exportTerrainPoints;

    // Land properties
    SerializedProperty terrainData;

    // Data editors
    Editor terrainDataEditor;

    void OnEnable() {
        terraformer = (Terraformer)target;
        // General
        terrainMaterial = serializedObject.FindProperty("terrainMaterial");
        size = serializedObject.FindProperty("size");
        pipelines = serializedObject.FindProperty("pipelines");
        seed = serializedObject.FindProperty("seed");
        useGeneralSeed = serializedObject.FindProperty("useGeneralSeed");

        // Exporter
        exportTerrainPoints = serializedObject.FindProperty("exportTerrainPoints");

        // Land
        terrainData = serializedObject.FindProperty("terrainData");
    }
    public override void OnInspectorGUI() {
        GUIStyle richTextStyle = new GUIStyle();
        richTextStyle.richText = true;
        serializedObject.Update();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("<color=#d85d5d><b>General</b></color>", richTextStyle);
        EditorGUILayout.PropertyField(terrainMaterial, new GUIContent("Material"));
        EditorGUILayout.PropertyField(size);
        EditorGUILayout.PropertyField(seed);
        EditorGUILayout.PropertyField(useGeneralSeed);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("<color=#d85d5d><b>Exporter</b></color>", richTextStyle);
        terraformer.exportPipelines = EditorGUILayout.Toggle("Pipelines", terraformer.exportPipelines);
        terraformer.exportGrass = EditorGUILayout.Toggle("Grass", terraformer.exportGrass);
        terraformer.exportTerrain = EditorGUILayout.Toggle("Terrain", terraformer.exportTerrain);
        EditorGUILayout.PropertyField(exportTerrainPoints, new GUIContent("Terrain points to export"));

        if (GUILayout.Button("Export")) {
            terraformer.ExportScene();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("<color=lime><b>Pipeline</b></color>", richTextStyle);
        EditorGUILayout.PropertyField(pipelines);
        if (GUILayout.Button("Run pipelines")) {
            terraformer.RunPipelines();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("<color=lime><b>Terrain</b></color>", richTextStyle);
        EditorGUILayout.PropertyField(terrainData);
        DrawEditor.Draw(terraformer.terrainData, ref terraformer.terrainDataFoldout, ref terrainDataEditor);
        if (GUILayout.Button("Generate terrain")) {
            terraformer.GenerateTerrain();
        }


        serializedObject.ApplyModifiedProperties();
    }
}
