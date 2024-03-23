using UnityEditor;
using UnityEngine;

public static class TexturePreviewDrawer {
    public static bool DrawTexturePreviewFoldout(SerializedProperty textureProperty, bool foldoutState) {
        // Use EditorPrefs to store the foldout state
        foldoutState = EditorGUILayout.Foldout(foldoutState, "Preview");

        // If foldout is closed, return the current foldout state
        if (!foldoutState) return foldoutState;

        EditorGUILayout.BeginVertical(GUI.skin.box);

        // Access the Texture2D property from the SerializedProperty
        Texture2D texture = (Texture2D)textureProperty.objectReferenceValue;

        if (texture != null) {
            // Draw the texture preview
            GUILayout.Label(texture, GUILayout.Width(256), GUILayout.Height(256));
        } else {
            GUILayout.Label("No Texture Selected");
        }

        EditorGUILayout.EndVertical();

        // Return the updated foldout state
        return foldoutState;
    }

    public static bool DrawTexturePreviewFoldout(Texture2D texture, bool foldoutState)
    {
        // Use EditorPrefs to store the foldout state
        foldoutState = EditorGUILayout.Foldout(foldoutState, "Preview");

        // If foldout is closed, return the current foldout state
        if (!foldoutState) return foldoutState;

        EditorGUILayout.BeginVertical(GUI.skin.box);

        if (texture != null)
        {
            // Draw the texture preview
            GUILayout.Label(texture, GUILayout.Width(256), GUILayout.Height(256));
        }
        else
        {
            GUILayout.Label("No Texture Selected");
        }

        EditorGUILayout.EndVertical();

        // Return the updated foldout state
        return foldoutState;
    }
}
