#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

[System.Serializable]
public class MapPackageInfo
{
    public string name;
    public string version;
    public string author;
    public string description;

    public string pcFileName;
    public string androidFileName;
}

public static class MapExporter
{
#if UNITY_EDITOR
    private static bool exporting;

    public static void ExportFromDescriptor(MapDescriptor descriptor)
    {
        if (exporting)
        {
            Debug.LogError("[MapExporter] Already exporting, please wait.");
            return;
        }

        ExportInternalAsync(descriptor);
    }

    private static async void ExportInternalAsync(MapDescriptor descriptor)
    {
        exporting = true;

        try
        {
            await ExportInternal(descriptor);
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

    private static async Task ExportInternal(MapDescriptor descriptor)
    {
        if (descriptor == null)
            throw new Exception("MapDescriptor is null.");

        if (string.IsNullOrEmpty(descriptor.mapName))
            throw new Exception("Map name cannot be empty.");

        if (string.IsNullOrEmpty(descriptor.exportFolder))
            throw new Exception("Export folder not set on MapDescriptor.");

        Scene scene = SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path))
            throw new Exception("Scene must be saved before exporting.");

        Directory.CreateDirectory(descriptor.exportFolder);

        if (descriptor.bakeLightingBeforeExport)
        {
            Debug.Log("[MapExporter] Baking lighting...");
            Lightmapping.BakeAsync();

            while (Lightmapping.isRunning)
                await Task.Delay(100);

            Debug.Log("[MapExporter] Lighting bake complete.");
        }

        CreateMissingProjectFolders();

        string sanitizedName = SanitizeString(descriptor.mapName);
        string exportScenePath = $"Assets/Scenes/Exported/{sanitizedName}.unity";

        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.SaveScene(scene, exportScenePath, true);
        Scene exportScene = EditorSceneManager.OpenScene(exportScenePath);

        UnpackAllPrefabsInScene(exportScene);

        string lightingAssetPath = null;

        if (descriptor.bakeLightingBeforeExport)
        {
            LightingDataAsset lightingData = Lightmapping.lightingDataAsset;
            if (lightingData != null)
            {
                lightingAssetPath = $"Assets/Scenes/Exported/{sanitizedName}_LightingData.asset";
                AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(lightingData), lightingAssetPath);
            }
        }

        EditorSceneManager.SaveScene(exportScene);

        string tempDir = Application.temporaryCachePath;
        string pcBundleName = sanitizedName + "_win64";
        string androidBundleName = sanitizedName + "_android";

        // -------------------------------
        // INCLUDE SCENE + LIGHTING DATA
        // -------------------------------
        var assetList = lightingAssetPath != null
            ? new[] { exportScenePath, lightingAssetPath }
            : new[] { exportScenePath };

        Debug.Log("[MapExporter] Building PC AssetBundle...");
        BuildAssetBundle(tempDir, pcBundleName, assetList, BuildTarget.StandaloneWindows64);

        Debug.Log("[MapExporter] Building Android AssetBundle...");
        BuildAssetBundle(tempDir, androidBundleName, assetList, BuildTarget.Android);

        MapPackageInfo pkg = new MapPackageInfo
        {
            name = descriptor.mapName,
            version = descriptor.version,
            author = descriptor.author,
            description = descriptor.description,
            pcFileName = pcBundleName,
            androidFileName = androidBundleName
        };

        string json = JsonUtility.ToJson(pkg, true);
        string packageJsonPath = Path.Combine(tempDir, "package.json");
        File.WriteAllText(packageJsonPath, json);

        string cleanFolder = Path.Combine(descriptor.exportFolder, sanitizedName + "_Export");
        if (Directory.Exists(cleanFolder))
            Directory.Delete(cleanFolder, true);
        Directory.CreateDirectory(cleanFolder);

        File.Copy(Path.Combine(tempDir, pcBundleName), Path.Combine(cleanFolder, pcBundleName), true);
        File.Copy(Path.Combine(tempDir, androidBundleName), Path.Combine(cleanFolder, androidBundleName), true);
        File.Copy(packageJsonPath, Path.Combine(cleanFolder, "package.json"), true);

        string zipPath = Path.Combine(descriptor.exportFolder, sanitizedName + ".zip");
        if (File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(cleanFolder, zipPath);
        Directory.Delete(cleanFolder, true);

        AssetDatabase.Refresh();

        EditorUtility.RevealInFinder(zipPath);
        EditorUtility.DisplayDialog("Export Complete", "Exported to:\n" + zipPath, "OK");

        Debug.Log("[MapExporter] Export complete: " + zipPath);
    }

    private static void CreateMissingProjectFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes/Exported"))
            AssetDatabase.CreateFolder("Assets/Scenes", "Exported");
    }

    private static string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "Map";

        foreach (char c in Path.GetInvalidFileNameChars())
            input = input.Replace(c.ToString(), "_");

        return input.Replace(" ", "_");
    }

    private static void UnpackAllPrefabsInScene(Scene scene)
    {
        foreach (var go in scene.GetRootGameObjects())
            UnpackPrefabRecursive(go);
    }

    private static void UnpackPrefabRecursive(GameObject go)
    {
        var status = PrefabUtility.GetPrefabInstanceStatus(go);
        if (status == PrefabInstanceStatus.Connected)
        {
            var root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        foreach (Transform child in go.transform)
            UnpackPrefabRecursive(child.gameObject);
    }

    private static void BuildAssetBundle(string outputDir, string bundleName, string[] assetNames, BuildTarget target)
    {
        AssetBundleBuild abb = new AssetBundleBuild
        {
            assetBundleName = bundleName,
            assetNames = assetNames
        };

        BuildPipeline.BuildAssetBundles(
            outputDir,
            new AssetBundleBuild[] { abb },
            BuildAssetBundleOptions.None,
            target
        );
    }
#endif
}
