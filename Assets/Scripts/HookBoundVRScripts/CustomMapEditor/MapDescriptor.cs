#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.IO.Compression;

public class MapDescriptor : MonoBehaviour
{
    public string mapName = "";
    public string version = "0.0.1";
    public bool autoIncrementVersion = true;

    public Material skyboxMaterial;
    [TextArea(3, 6)]
    public string mapDescription = "";

    public string externalExportPath = "";

    bool Validate(out string error)
    {
        if (string.IsNullOrEmpty(mapName))
        {
            error = "Map name cannot be empty.";
            return false;
        }

        if (string.IsNullOrEmpty(externalExportPath))
        {
            error = "External export path is not set.";
            return false;
        }

        if (transform.parent != null)
        {
            error = "MapDescriptor must be on a ROOT GameObject.";
            return false;
        }

        if (transform.Find("PlayerSpawnPod") == null)
        {
            error = "PlayerSpawnPod is missing.";
            return false;
        }

        var scene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path))
        {
            error = "Scene must be saved before exporting.";
            return false;
        }

        error = "";
        return true;
    }

    void IncrementVersion()
    {
        var p = version.Split('.');
        if (p.Length != 3)
        {
            version = "0.0.1";
            return;
        }

        int a, b, c;
        if (!int.TryParse(p[0], out a) ||
            !int.TryParse(p[1], out b) ||
            !int.TryParse(p[2], out c))
        {
            version = "0.0.1";
            return;
        }

        c++;
        version = $"{a}.{b}.{c}";
    }

    public void ExportAndroidBundle()
    {
        if (!Validate(out string err))
        {
            EditorUtility.DisplayDialog("Export Error", err, "OK");
            return;
        }

        if (autoIncrementVersion)
        {
            IncrementVersion();
            EditorUtility.SetDirty(this);
        }

        // Save original scene
        Scene originalScene = EditorSceneManager.GetActiveScene();
        string originalScenePath = originalScene.path;
        EditorSceneManager.SaveScene(originalScene);

        // Switch to blank scene
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Apply skybox
        RenderSettings.skybox = skyboxMaterial;

        // Build folder inside Assets (Unity requires this)
        string internalFolder = "Assets/MapExports/" + mapName;
        if (!Directory.Exists(internalFolder))
            Directory.CreateDirectory(internalFolder);

        string bundleName = mapName + "_android";

        // Build the bundle
        AssetBundleBuild build = new AssetBundleBuild
        {
            assetBundleName = bundleName,
            assetNames = new[] { originalScenePath }
        };

        BuildPipeline.BuildAssetBundles(
            internalFolder,
            new[] { build },
            BuildAssetBundleOptions.None,
            BuildTarget.Android
        );

        // Write package.json inside internal folder
        string safeDescription = (mapDescription ?? "").Replace("\"", "\\\"");
        string json =
            "{\n" +
            $"  \"name\": \"{mapName}\",\n" +
            $"  \"version\": \"{version}\",\n" +
            $"  \"description\": \"{safeDescription}\"\n" +
            "}";

        File.WriteAllText(Path.Combine(internalFolder, "package.json"), json);

        // Refresh Unity
        AssetDatabase.Refresh();

        // ✅ Create CLEAN TEMP FOLDER outside Assets
        string cleanFolder = Path.Combine(externalExportPath, mapName + "_Clean");
        if (Directory.Exists(cleanFolder))
            Directory.Delete(cleanFolder, true);
        Directory.CreateDirectory(cleanFolder);

        // ✅ Copy ONLY the clean files
        File.Copy(Path.Combine(internalFolder, bundleName), Path.Combine(cleanFolder, bundleName));
        File.Copy(Path.Combine(internalFolder, "package.json"), Path.Combine(cleanFolder, "package.json"));

        // ✅ Create ZIP
        string zipPath = Path.Combine(externalExportPath, mapName + ".zip");
        if (File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(cleanFolder, zipPath);

        // ✅ Delete clean temp folder
        Directory.Delete(cleanFolder, true);

        // ✅ Return to original scene
        EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);

        // ✅ Show ZIP in File Explorer
        EditorUtility.RevealInFinder(zipPath);

        EditorUtility.DisplayDialog("Export Complete", "Your ZIP is ready!", "OK");
    }
}
#endif
