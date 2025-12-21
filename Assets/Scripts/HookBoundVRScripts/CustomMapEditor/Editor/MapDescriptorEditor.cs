#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapDescriptor))]
public class MapDescriptorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapDescriptor map = (MapDescriptor)target;

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Map Info", EditorStyles.boldLabel);

        map.mapName = EditorGUILayout.TextField("Map Name", map.mapName);
        map.version = EditorGUILayout.TextField("Version", map.version);
        map.autoIncrementVersion = EditorGUILayout.Toggle("Auto Increment Version", map.autoIncrementVersion);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
        map.mapDescription = EditorGUILayout.TextArea(map.mapDescription, GUILayout.Height(60));

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        map.skyboxMaterial = (Material)EditorGUILayout.ObjectField("Skybox Material", map.skyboxMaterial, typeof(Material), false);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("External Export Path", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        map.externalExportPath = EditorGUILayout.TextField(map.externalExportPath);

        GUIStyle browseStyle = new GUIStyle(GUI.skin.button);
        browseStyle.normal.textColor = Color.white;
        browseStyle.fontStyle = FontStyle.Bold;
        browseStyle.fixedHeight = 22;
        browseStyle.fixedWidth = 80;

        if (GUILayout.Button("Browse", browseStyle))
        {
            string p = EditorUtility.OpenFolderPanel("Choose Export Folder", map.externalExportPath, "");
            if (!string.IsNullOrEmpty(p))
            {
                map.externalExportPath = p;
                EditorUtility.SetDirty(map);
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUIStyle exportStyle = new GUIStyle(GUI.skin.button);
        exportStyle.fontSize = 14;
        exportStyle.fontStyle = FontStyle.Bold;
        exportStyle.fixedHeight = 40;

        if (GUILayout.Button("Export Android Bundle", exportStyle))
        {
            map.ExportAndroidBundle();
        }
    }
}
#endif
