using UnityEngine;

[System.Serializable]
public class LightingSettingsData
{
    public int ambientMode;
    public Color ambientColor;
    public float ambientIntensity;

    public float reflectionIntensity;
    public int reflectionBounces;

    public bool fog;
    public Color fogColor;
    public int fogMode;
    public float fogDensity;
    public float fogStartDistance;
    public float fogEndDistance;

    public int lightmapsMode;

    public string skyboxMaterialName;
}
