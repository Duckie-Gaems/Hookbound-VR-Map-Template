using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapDescriptor : MonoBehaviour
{
    [Header("Map Info")]
    public string mapName = "MyMap";
    public string version = "1.0.0";
    public string author = "Unknown";

    [TextArea(3, 6)]
    public string description = "";

    [Header("Export Settings")]
    public string exportFolder = "";

    [Header("Advanced")]
    public bool bakeLightingBeforeExport = true;
    public bool autoIncrementVersion = true;

    public void IncrementPatch()
    {
        string[] parts = version.Split('.');
        if (parts.Length != 3)
        {
            version = "1.0.1";
            return;
        }

        if (int.TryParse(parts[2], out int patch))
        {
            patch++;
            version = $"{parts[0]}.{parts[1]}.{patch}";
        }
        else
        {
            version = "1.0.1";
        }
    }

#if UNITY_EDITOR
    public void ExportMap()
    {
        if (autoIncrementVersion)
        {
            IncrementPatch();
            EditorUtility.SetDirty(this);
        }

        MapExporter.ExportFromDescriptor(this);
    }
#endif
}
#if UNITY_EDITOR


[CustomEditor(typeof(MapDescriptor))]
public class MapDescriptorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapDescriptor desc = (MapDescriptor)target;

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Export Controls", EditorStyles.boldLabel);

        // Browse button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Export Folder:", GUILayout.Width(100));
        desc.exportFolder = EditorGUILayout.TextField(desc.exportFolder);

        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Export Folder", desc.exportFolder, "");
            if (!string.IsNullOrEmpty(selected))
            {
                desc.exportFolder = selected;
                EditorUtility.SetDirty(desc);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Export button
        if (GUILayout.Button("Export Map (MapExporter)", GUILayout.Height(30)))
        {
            desc.ExportMap();
        }

        EditorGUILayout.HelpBox(
            "Exports the current scene using the GT-style hybrid pipeline.\n" +
            "Make sure the scene is saved before exporting.",
            MessageType.Info
        );
    }
}
#endif
