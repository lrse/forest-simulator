using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(BiomeNode))]
public class BiomeNodeEditor : NodeEditor {
    private bool showPreviewFoldout = true;
    public override void OnBodyGUI() {
        BiomeNode node = target as BiomeNode;
        ShowPreviewAttribute showPreviewAttr = (ShowPreviewAttribute)System.Attribute.GetCustomAttribute(target.GetType(), typeof(ShowPreviewAttribute));
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        base.OnBodyGUI();
        if (EditorGUI.EndChangeCheck()) {
            node.OnInputChanged();
        }

        if (showPreviewAttr.showPreview) {
            showPreviewFoldout = TexturePreviewDrawer.DrawTexturePreviewFoldout(serializedObject.FindProperty("output"), showPreviewFoldout);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
