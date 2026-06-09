using UnityEngine;
using System.Collections.Generic;

public class LuaScriptHolder : MonoBehaviour
{
    [System.Serializable]
    public class LuaEntry
    {
        public string Name;
        public TextAsset File;   // <— accepts .lua files
    }

    public List<LuaEntry> Scripts = new List<LuaEntry>();
}
