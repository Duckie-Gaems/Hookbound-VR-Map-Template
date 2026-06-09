#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System.IO;

[CreateAssetMenu(
    fileName = "BuildDLLs",
    menuName = "DLL Builder/Build DLL Settings",
    order = 1
)]
public class BuildDLLs : ScriptableObject
{
    [Header("DLL Build Settings")]
    public string sourceFolder = "Assets/MyScripts";
    public string outputFolder = "Assets/OutputDLLs";
    public string dllName = "MyLibrary.dll";

    [Header("Optional")]
    public bool includeUnityEditor = false;

    // ---------------------------------------------------------
    // BUILD DLL
    // ---------------------------------------------------------
    public void Build()
    {
        // Validate source folder
        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogError("[BuildDLLs] Source folder does not exist: " + sourceFolder);
            return;
        }

        // Ensure output folder exists
        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string outputPath = Path.Combine(outputFolder, dllName);

        // Collect all .cs files in the source folder
        string[] scripts = Directory.GetFiles(sourceFolder, "*.cs", SearchOption.AllDirectories);

        if (scripts.Length == 0)
        {
            Debug.LogError("[BuildDLLs] No C# scripts found in: " + sourceFolder);
            return;
        }

        // Build absolute paths to Unity DLLs
        string unityEnginePath = Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll");
        string unityEditorPath = Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEditor.dll");

        if (!File.Exists(unityEnginePath))
        {
            Debug.LogError("[BuildDLLs] Could not locate UnityEngine.dll at: " + unityEnginePath);
            return;
        }

        // Create assembly builder
        AssemblyBuilder builder = new AssemblyBuilder(outputPath, scripts);

        // Add Unity references
        builder.additionalReferences = includeUnityEditor
            ? new[] { unityEnginePath, unityEditorPath }
            : new[] { unityEnginePath };

        builder.buildFinished += OnBuildFinished;

        Debug.Log("[BuildDLLs] Building DLL: " + outputPath);
        builder.Build();
    }

    // ---------------------------------------------------------
    // BUILD CALLBACK
    // ---------------------------------------------------------
    private void OnBuildFinished(string path, CompilerMessage[] messages)
    {
        bool hasErrors = false;

        foreach (var msg in messages)
        {
            if (msg.type == CompilerMessageType.Error)
            {
                hasErrors = true;
                Debug.LogError("[DLL ERROR] " + msg.message);
            }
            else if (msg.type == CompilerMessageType.Warning)
            {
                Debug.LogWarning("[DLL WARNING] " + msg.message);
            }
        }

        if (!hasErrors)
            Debug.Log("[BuildDLLs] DLL built successfully at: " + path);
        else
            Debug.LogError("[BuildDLLs] DLL build failed.");
    }
}

// ---------------------------------------------------------
// CUSTOM INSPECTOR
// ---------------------------------------------------------
[CustomEditor(typeof(BuildDLLs))]
public class BuildDLLsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BuildDLLs builder = (BuildDLLs)target;

        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Build DLL", GUILayout.Height(35)))
        {
            builder.Build();
        }

        EditorGUILayout.HelpBox(
            "This tool compiles all .cs files in the source folder into a DLL.\n" +
            "Uses Unity's AssemblyBuilder API.",
            MessageType.Info
        );
    }
}
#endif
