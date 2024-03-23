using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class DrawEditor {
    public static void Draw(Object script, ref bool foldout, ref Editor editor) {
        if (script != null) {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, script);
            using (var check = new EditorGUI.ChangeCheckScope()) {
                if (foldout) {
                    Editor.CreateCachedEditor(script, null, ref editor);
                    editor.OnInspectorGUI();
                    if (check.changed) { }
                }
            }
        }
    }
}
