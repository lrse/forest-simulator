using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public abstract class TerrainToolWindow : EditorWindow {

    private Terrain terrain;
    private TaskManager taskManager;

    void Awake() {
        taskManager = new TaskManager();
    }

    void OnGUI() {
        if (taskManager.Processing) {
            if (taskManager.Progress == 1) {
                taskManager.Stop();
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Done!", "Files were created. Check the folder you previously selected", "OK");
            } else {
                EditorUtility.DisplayProgressBar("Working on it...", taskManager.Description, taskManager.Progress);
            }
        } else {
            terrain = (Terrain)EditorGUILayout.ObjectField(terrain, typeof(Terrain), true);
            if (terrain != null) {
                LoadFields();
                if (GUILayout.Button(GetButtonText())) {
                    string folderPath = EditorUtility.OpenFolderPanel("Please select where to store the generated files.", "", "");
                    if (folderPath == "") {
                        EditorUtility.DisplayDialog("Error!", "You must select a folder", "OK");
                    } else {
                        taskManager.Run(GetTaskList(terrain, folderPath));
                    }
                }
            } else {
                EditorGUILayout.LabelField("Message:", "Please select a terrain.");
            }
        }
    }

    protected abstract void LoadFields();
    protected abstract string GetButtonText();
    protected abstract TaskList GetTaskList(Terrain terrain, string folderPath);

    void Update() {
        taskManager.ContinueIfProcessing();    
    }

    void OnInspectorUpdate() {
        Repaint();
    }

}