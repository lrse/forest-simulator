using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralGrassRenderer))]
public class GrassRendererEditor : Editor {
    Editor grassDataEditor;
    Editor samplerParallelEditor;
    public override void OnInspectorGUI() {
        GUIStyle richTextStyle = new GUIStyle();
        richTextStyle.richText = true;
        ProceduralGrassRenderer pgr = (ProceduralGrassRenderer)target;

        serializedObject.Update();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("<color=lime><b>Grass</b></color>", richTextStyle);
        base.OnInspectorGUI();
        DrawEditor.Draw(pgr.grassData, ref pgr.grassDataFoldout, ref grassDataEditor);
        DrawEditor.Draw(pgr.windowSamplerParallel, ref pgr.samplerParallelDataFoldout, ref samplerParallelEditor);

        if (GUILayout.Button("Generate grass")) {
            pgr.Initialize();
            pgr.Dispatch();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
