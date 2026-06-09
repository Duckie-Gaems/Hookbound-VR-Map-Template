using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class LuaCreationWindow : EditorWindow
{
    private LuaScriptHolder holder;
    private Vector2 scroll;

    [MenuItem("Map Making Tools/Lua Creation")]
    public static void Open()
    {
        GetWindow<LuaCreationWindow>("Lua Creation");
    }

    private void OnGUI()
    {
        GUILayout.Label("Lua Script Table", EditorStyles.boldLabel);

        // Auto-find the holder in the scene
        if (holder == null)
            holder = FindObjectOfType<LuaScriptHolder>();

        if (holder == null)
        {
            if (GUILayout.Button("Create LuaScriptHolder in Scene"))
            {
                GameObject go = new GameObject("LuaScriptHolder");
                holder = go.AddComponent<LuaScriptHolder>();
            }
            return;
        }

        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < holder.Scripts.Count; i++)
        {
            var entry = holder.Scripts[i];

            EditorGUILayout.BeginVertical("box");

            entry.Name = EditorGUILayout.TextField("Name", entry.Name);

            // Accept ONLY TextAsset (Unity-friendly)
            entry.File = (TextAsset)EditorGUILayout.ObjectField(
                "Lua File",
                entry.File,
                typeof(TextAsset),
                false
            );

            if (GUILayout.Button("Remove"))
            {
                holder.Scripts.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Script"))
        {
            holder.Scripts.Add(new LuaScriptHolder.LuaEntry());
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(holder);
        }
    }
}
#endif
