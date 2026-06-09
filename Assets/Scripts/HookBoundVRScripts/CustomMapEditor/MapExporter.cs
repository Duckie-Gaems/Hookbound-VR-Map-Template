#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using System.IO;
using System.IO.Compression;

[Serializable]
public class MapPackageInfo
{
    public string androidFileName;
    public string initialScene;
    public string maxPlayers;
    public string defaultGamemode;
    public string avaliableGamemodes;
    public string customMapEditorVer;
}

public static class MapExporter
{
#if UNITY_EDITOR
    private static bool exporting;
    private static bool VERBOSE = true;

    private static void LogVerbose(string msg)
    {
        if (VERBOSE)
            Debug.Log("[MapExporter VERBOSE] " + msg);
    }
    public static string BuildPackageJson(MapPackageInfo pkg, string gamemodeListJson)
{
    var sb = new System.Text.StringBuilder();

    sb.AppendLine("{");
    sb.AppendLine($"  \"androidFileName\": \"{pkg.androidFileName}\",");
    sb.AppendLine($"  \"maxPlayers\": \"{pkg.maxPlayers}\",");
    sb.AppendLine($"  \"initialScene\": \"{pkg.initialScene}\",");
    sb.AppendLine($"  \"defaultGamemode\": \"{pkg.defaultGamemode}\",");

    if (!string.IsNullOrEmpty(gamemodeListJson))
        sb.AppendLine($"  \"avaliableGamemodes\": {gamemodeListJson},");

    sb.AppendLine($"  \"customMapEditorVer\": \"{pkg.customMapEditorVer}\"");
    sb.AppendLine("}");

    return sb.ToString();
}


    // ---------------------------------------------------------
    // ENTRY POINT (SYNC, NO ASYNC BUGS)
    // ---------------------------------------------------------
    public static void ExportFromDescriptor(MapDescriptor descriptor)
    {
        if (exporting)
        {
            Debug.LogError("[MapExporter] Already exporting.");
            return;
        }

        exporting = true;

        try
        {
            ExportInternal(descriptor);
        }
        catch (Exception ex)
        {
            Debug.LogError("[MapExporter] Export failed:\n" + ex);
            EditorUtility.DisplayDialog("Export Failed", ex.Message, "OK");
        }
        finally
        {
            exporting = false;
        }
    }

    // ---------------------------------------------------------
    // MAIN EXPORT FUNCTION (SYNC)
    // ---------------------------------------------------------
    
    private static void ExportInternal(MapDescriptor descriptor)
    {
        if (descriptor == null)
            throw new Exception("MapDescriptor is null.");

        if (string.IsNullOrEmpty(descriptor.mapName))
            throw new Exception("Map name cannot be empty.");

        if (string.IsNullOrEmpty(descriptor.exportFolder))
            throw new Exception("Export folder not set.");

        Scene activeScene = SceneManager.GetActiveScene();

        // Prevent the "Untitled" bug
        if (string.IsNullOrEmpty(activeScene.path))
            throw new Exception("Active scene must be saved before exporting.");

        Directory.CreateDirectory(descriptor.exportFolder);

        string sanitizedName = Sanitize(descriptor.mapName);
        string androidBundleName = sanitizedName + "_android";

        // ---------------------------------------------------------
        // FORCE URP DEPTH + OPAQUE TEXTURES
        // ---------------------------------------------------------
        var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urp != null)
        {
            urp.supportsCameraDepthTexture = true;
            urp.supportsCameraOpaqueTexture = true;
            EditorUtility.SetDirty(urp);
            AssetDatabase.SaveAssets();
        }
// ---------------------------------------------------------
// SCRIPT WHITELIST CLEANUP
// ---------------------------------------------------------
int removedScripts = ScriptSanitizer.CleanScene();
Debug.Log($"[Whitelist] Removed {removedScripts} unauthorized scripts before export.");

        // ---------------------------------------------------------
        // DUPLICATE SCENE INTO EXPORTED FOLDER
        // ---------------------------------------------------------
        string exportSceneFolder = "Assets/Scenes/Exported/";
        Directory.CreateDirectory(exportSceneFolder);

        string exportedScenePath = exportSceneFolder + sanitizedName + ".unity";

        LogVerbose("Saving exported scene: " + exportedScenePath);

        if (!EditorSceneManager.SaveScene(activeScene, exportedScenePath))
            throw new Exception("Failed to save exported scene.");

        // ---------------------------------------------------------
        // SWITCH TO TEMPORARY SCENE
        // ---------------------------------------------------------
        string originalScenePath = activeScene.path;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        LogVerbose("Opened temporary scene for clean bundle build.");

        // ---------------------------------------------------------
        // BUILD STREAMED SCENE BUNDLE
        // ---------------------------------------------------------
        string tempDir = Application.temporaryCachePath;

        AssetBundleBuild abb = new AssetBundleBuild
        {
            assetBundleName = androidBundleName,
            assetNames = new[] { exportedScenePath }
        };

        LogVerbose("Building Android streamed scene bundle: " + androidBundleName);

        BuildPipeline.BuildAssetBundles(
            tempDir,
         new[] { abb },
            BuildAssetBundleOptions.None,
            BuildTarget.Android
        );

        string androidPath = Path.Combine(tempDir, androidBundleName);
        if (!File.Exists(androidPath))
            throw new Exception("Android bundle was not created: " + androidPath);

        // ---------------------------------------------------------
        // RESTORE ORIGINAL SCENE
        // ---------------------------------------------------------
        EditorSceneManager.OpenScene(originalScenePath);
        LogVerbose("Restored original scene.");

        // ---------------------------------------------------------
        // DO NOT DELETE THE EXPORTED SCENE (IMPORTANT FOR URP)
        // ---------------------------------------------------------
        LogVerbose("Keeping exported scene (URP metadata preserved).");

        // ---------------------------------------------------------
        // CREATE JSON
        // ---------------------------------------------------------
        string packageJsonPath = Path.Combine(tempDir, "package.json");
var first = descriptor.GetFirstGamemode();
int count = descriptor.GetSelectedGamemodeCount();

MapPackageInfo pkg = new MapPackageInfo
{
    androidFileName = androidBundleName,
    maxPlayers = descriptor.MaxPlayers.ToString(),
    initialScene = SceneManager.GetActiveScene().name,
    defaultGamemode = first.ToString(),
    customMapEditorVer = MapDescriptor.NormalizeVersion(Application.version)
};

string gmJson = count > 1 ? descriptor.GetGamemodeListJson() : null;

string json = BuildPackageJson(pkg, gmJson);

File.WriteAllText(packageJsonPath, json);


        // ---------------------------------------------------------
        // BUILD EXPORT FOLDER
        // ---------------------------------------------------------
        string cleanFolder = Path.Combine(descriptor.exportFolder, sanitizedName + "_Export");
        if (Directory.Exists(cleanFolder))
            Directory.Delete(cleanFolder, true);
        Directory.CreateDirectory(cleanFolder);

        File.Copy(androidPath, Path.Combine(cleanFolder, androidBundleName), true);
        File.Copy(packageJsonPath, Path.Combine(cleanFolder, "package.json"), true);

        // ---------------------------------------------------------
        // ZIP IT
        // ---------------------------------------------------------
        string zipPath = Path.Combine(descriptor.exportFolder, sanitizedName + ".zip");
        int counter = 1;
string baseName = sanitizedName;
string folder = descriptor.exportFolder;

while (File.Exists(zipPath))
{
    string newName = $"{baseName}_{counter}.zip";
    zipPath = Path.Combine(folder, newName);
    counter++;
}

        ZipFile.CreateFromDirectory(cleanFolder, zipPath);
        Directory.Delete(cleanFolder, true);

        AssetDatabase.Refresh();

        EditorUtility.RevealInFinder(zipPath);
        if (removedScripts > 0)
{
    EditorUtility.DisplayDialog(
        "Script Whitelist",
        $"Export completed.\n\nRemoved {removedScripts} unauthorized scripts.\nOnly approved scripts are allowed in custom maps.",
        "OK"
    );
}

        EditorUtility.DisplayDialog("Export Complete", "Exported to:\n" + zipPath, "OK");
    }

    private static string Sanitize(string input)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            input = input.Replace(c.ToString(), "_");

        return input.Replace(" ", "_");
    }

#endif
}
