using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(SourceNode))]
public class SourceNodeEditor : NodeEditor {
    private SourceNode node;
    private bool initialized = false;
    private Texture2D texturePreview;
    private bool showPreview = true;
    SerializedProperty texture;
    SerializedProperty pipeline;
    SerializedProperty sourceNode;
    SerializedProperty seed;

    // Fractal noise props
    SerializedProperty scale;
    SerializedProperty octaves;
    SerializedProperty persistance;
    SerializedProperty lacunarity;
    SerializedProperty offset;

    // Voronoi noise props
    SerializedProperty centroids;

    void Initialize() {
        if (initialized) return;
        initialized = true;
        if (node == null) node = target as SourceNode;
        texture = serializedObject.FindProperty("texture");
        pipeline = serializedObject.FindProperty("pipeline");
        sourceNode = serializedObject.FindProperty("sourceNode");
        scale = serializedObject.FindProperty("scale");
        octaves = serializedObject.FindProperty("octaves");
        persistance = serializedObject.FindProperty("persistance");
        lacunarity = serializedObject.FindProperty("lacunarity");
        seed = serializedObject.FindProperty("seed");
        offset = serializedObject.FindProperty("offset");
        centroids = serializedObject.FindProperty("centroids");
        texturePreview = node.GetTexture();
        node.OnInputChanged();
    }
    public override void OnBodyGUI() {
        Initialize();
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        node.sourceType = (SourceNodeType)EditorGUILayout.EnumPopup("Source", node.sourceType);

        if (node.dirty) {
            texturePreview = node.GetTexture();
            node.dirty = false;
        }

        switch (node.sourceType) {
            case SourceNodeType.Texture:
                NodeEditorGUILayout.PropertyField(texture);
                break;
            case SourceNodeType.SourceNode:
                NodeEditorGUILayout.PropertyField(sourceNode);
                break;
            case SourceNodeType.Pipeline:
                NodeEditorGUILayout.PropertyField(pipeline);
                break;
            case SourceNodeType.FractalNoise:
                DrawFractalNoiseGUI();
                break;
            case SourceNodeType.VoronoiNoise:
                DrawVoronoiNoiseGUI();
                break;
        }
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("output"));
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck()) {
            node.OnInputChanged();
            texturePreview = node.output;
        }

        showPreview = TexturePreviewDrawer.DrawTexturePreviewFoldout(texturePreview, showPreview);
    }

    private void DrawFractalNoiseGUI() {
        NodeEditorGUILayout.PropertyField(scale);
        NodeEditorGUILayout.PropertyField(octaves);
        NodeEditorGUILayout.PropertyField(persistance);
        NodeEditorGUILayout.PropertyField(lacunarity);
        NodeEditorGUILayout.PropertyField(seed);
        NodeEditorGUILayout.PropertyField(offset);

    }

    private void DrawVoronoiNoiseGUI() {
        NodeEditorGUILayout.PropertyField(centroids);
        NodeEditorGUILayout.PropertyField(seed);
    }
}