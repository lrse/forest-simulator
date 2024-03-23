using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(PlacementNode))]
public class PlacementNodeEditor : NodeEditor {
    private PlacementNode node;
    private bool initialized = false;
    SerializedProperty spawnTexture;
    SerializedProperty spawnPointsOS;
    SerializedProperty seed;
    SerializedProperty scaleFactorMinMax;
    SerializedProperty prefabs;
    SerializedProperty maxNormalAngle;

    void Initialize() {
        if (initialized) return;
        initialized = true;
        if (node == null) node = target as PlacementNode;
        spawnTexture = serializedObject.FindProperty("spawnTexture");
        seed = serializedObject.FindProperty("seed");
        spawnPointsOS = serializedObject.FindProperty("spawnPointsOS");
        scaleFactorMinMax = serializedObject.FindProperty("scaleFactorMinMax");
        prefabs = serializedObject.FindProperty("prefabs");
        maxNormalAngle = serializedObject.FindProperty("maxNormalAngle");
    }
    public override void OnBodyGUI() {
        GUIStyle richTextStyle = new GUIStyle();
        richTextStyle.richText = true;
        Initialize();
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        NodeEditorGUILayout.PropertyField(spawnPointsOS);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("<color=lime><b>Randomizers</b></color>", richTextStyle);
        NodeEditorGUILayout.PropertyField(seed);
        node.enableSpawnTextureUsage = EditorGUILayout.Toggle("Use spawn texture", node.enableSpawnTextureUsage);
        if (node.enableSpawnTextureUsage ) {
            NodeEditorGUILayout.PropertyField(spawnTexture);
        }
        node.enableScaleRandomizer = EditorGUILayout.Toggle("Randomize scale", node.enableScaleRandomizer);
        if (node.enableScaleRandomizer) {
            NodeEditorGUILayout.PropertyField(scaleFactorMinMax);
        }
        node.enableTwistRandomizer = EditorGUILayout.Toggle("Randomize twist", node.enableTwistRandomizer);
        if (node.enableTwistRandomizer) {
            NodeEditorGUILayout.PropertyField(maxNormalAngle);
        }
        node.enableSpinRandomizer = EditorGUILayout.Toggle("Randomize spin", node.enableSpinRandomizer);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("<color=lime><b>Prefabs</b></color>", richTextStyle);
        NodeEditorGUILayout.PropertyField(prefabs);

        serializedObject.ApplyModifiedProperties();
    }

}