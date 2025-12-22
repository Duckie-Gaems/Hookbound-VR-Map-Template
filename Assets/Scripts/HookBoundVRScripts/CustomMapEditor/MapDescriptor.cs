#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Content;
using UnityEditor.Rendering;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

public class MapDescriptor : MonoBehaviour
{
    public string mapName = "";
    public string version = "0.0.1";
    public bool autoIncrementVersion = true;

    public Material skyboxMaterial;

    [TextArea(3, 6)]
    public string mapDescription = "";

    public string externalExportPath = "";

    // ---------------- VALIDATION ----------------

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

    // ---------------- VERSIONING ----------------

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

    // ---------------- LIGHTING CAPTURE ----------------

    LightingSettingsData CaptureLightingSettings()
    {
        LightingSettingsData data = new LightingSettingsData();

        data.ambientMode = (int)RenderSettings.ambientMode;
        data.ambientColor = RenderSettings.ambientLight;
        data.ambientIntensity = RenderSettings.ambientIntensity;

        data.reflectionIntensity = RenderSettings.reflectionIntensity;
        data.reflectionBounces = RenderSettings.defaultReflectionResolution;

        data.fog = RenderSettings.fog;
        data.fogColor = RenderSettings.fogColor;
        data.fogMode = (int)RenderSettings.fogMode;
        data.fogDensity = RenderSettings.fogDensity;
        data.fogStartDistance = RenderSettings.fogStartDistance;
        data.fogEndDistance = RenderSettings.fogEndDistance;

        data.lightmapsMode = (int)LightmapSettings.lightmapsMode;

        Material sky = RenderSettings.skybox;
        if (sky != null)
            data.skyboxMaterialName = sky.name;
        else if (skyboxMaterial != null)
            data.skyboxMaterialName = skyboxMaterial.name;
        else
            data.skyboxMaterialName = "";

        return data;
    }

    void WriteLightingSettingsJson(string path)
    {
        var data = CaptureLightingSettings();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    // ---------------- STATIC OBJECT CAPTURE ----------------

    private List<StaticObjectInfo> CaptureStaticObjects()
    {
        List<StaticObjectInfo> list = new List<StaticObjectInfo>();

        Scene scene = EditorSceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (var root in roots)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                GameObject go = t.gameObject;
                int flags = (int)GameObjectUtility.GetStaticEditorFlags(go);

                if (flags != 0)
                {
                    string path = GetHierarchyPath(go);
                    list.Add(new StaticObjectInfo { path = path, staticFlags = flags });
                }
            }
        }

        return list;
    }

    private string GetHierarchyPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform;

        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }

    // ---------------- OBJECT VALIDATOR ----------------

    public void ValidateMap()
    {
        List<string> errors = new List<string>();

        if (transform.Find("PlayerSpawnPod") == null)
            errors.Add("❌ Missing PlayerSpawnPod");

        if (skyboxMaterial == null && RenderSettings.skybox == null)
            errors.Add("❌ No skybox assigned");

        if (Lightmapping.lightingDataAsset == null)
            errors.Add("❌ No baked lighting found (LightingData.asset missing)");

        if (errors.Count == 0)
        {
            EditorUtility.DisplayDialog("Map Validator", "✅ No issues found!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Map Validator", string.Join("\n", errors), "OK");
        }
    }

    // ---------------- EXPORT ----------------

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

        Scene originalScene = EditorSceneManager.GetActiveScene();
        string originalScenePath = originalScene.path;
        EditorSceneManager.SaveScene(originalScene);

        if (skyboxMaterial != null)
            RenderSettings.skybox = skyboxMaterial;

        string internalFolder = "Assets/MapExports/" + mapName;
        if (!Directory.Exists(internalFolder))
            Directory.CreateDirectory(internalFolder);

        string bundleName = mapName + "_android";

        List<string> assetList = new List<string>();
        string[] deps = AssetDatabase.GetDependencies(originalScenePath, true);
        assetList.AddRange(deps);

        if (skyboxMaterial != null)
        {
            string skyboxPath = AssetDatabase.GetAssetPath(skyboxMaterial);
            if (!assetList.Contains(skyboxPath))
                assetList.Add(skyboxPath);
        }

        if (Lightmapping.lightingDataAsset != null)
        {
            string lightingDataPath = AssetDatabase.GetAssetPath(Lightmapping.lightingDataAsset);
            if (!assetList.Contains(lightingDataPath))
                assetList.Add(lightingDataPath);
        }

        AssetBundleBuild build = new AssetBundleBuild
        {
            assetBundleName = bundleName,
            assetNames = assetList.ToArray()
        };

        BuildPipeline.BuildAssetBundles(
            internalFolder,
            new[] { build },
            BuildAssetBundleOptions.None,
            BuildTarget.Android
        );

        // Write package.json
        string safeDescription = (mapDescription ?? "").Replace("\"", "\\\"");
        string pkgJson =
            "{\n" +
            $"  \"name\": \"{mapName}\",\n" +
            $"  \"version\": \"{version}\",\n" +
            $"  \"description\": \"{safeDescription}\"\n" +
            "}";

        File.WriteAllText(Path.Combine(internalFolder, "package.json"), pkgJson);

        // Write lighting.json
        WriteLightingSettingsJson(Path.Combine(internalFolder, "lighting.json"));

        // Write staticobjects.json
        var staticList = new StaticObjectList { list = CaptureStaticObjects() };
        File.WriteAllText(
            Path.Combine(internalFolder, "staticobjects.json"),
            JsonUtility.ToJson(staticList, true)
        );

        AssetDatabase.Refresh();

        // Clean export folder
        string cleanFolder = Path.Combine(externalExportPath, mapName + "_Clean");
        if (Directory.Exists(cleanFolder))
            Directory.Delete(cleanFolder, true);
        Directory.CreateDirectory(cleanFolder);

        File.Copy(Path.Combine(internalFolder, bundleName), Path.Combine(cleanFolder, bundleName), true);
        File.Copy(Path.Combine(internalFolder, "package.json"), Path.Combine(cleanFolder, "package.json"), true);
        File.Copy(Path.Combine(internalFolder, "lighting.json"), Path.Combine(cleanFolder, "lighting.json"), true);
        File.Copy(Path.Combine(internalFolder, "staticobjects.json"), Path.Combine(cleanFolder, "staticobjects.json"), true);

        string zipPath = Path.Combine(externalExportPath, mapName + ".zip");
        if (File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(cleanFolder, zipPath);

        Directory.Delete(cleanFolder, true);

        EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);

        EditorUtility.RevealInFinder(zipPath);
        EditorUtility.DisplayDialog("Export Complete", "Your ZIP is ready!", "OK");
    }
}

#endif
