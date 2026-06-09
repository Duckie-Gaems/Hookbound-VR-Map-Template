using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace EasyXRSystems
{
    [DisallowMultipleComponent]
      public abstract class XRButton : MonoBehaviour
    {
        [Header("General Settings")]
        public bool leftHandOnly = false;
        public float cooldown = 0.15f;
        public float maxCheckDistance = 0.05f;

        [Header("Hold Settings - only used when hold-able button")]
        public float handHoldDuration = 0.5f;
        public float bodyHoldDuration = 0.5f;
                public virtual void ButtonActivationWithHand(bool isLeftHand, bool isPressed) { }
        public virtual void ButtonActivationWithBody(bool isPressed) { }

        public virtual void OnHandHold(bool isLeftHand, float duration) { }
        public virtual void OnBodyHold(float duration) { }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(XRButton), true)]
    public class EasyXRButtonEditor : Editor
    {
        // XRButton base fields
        private readonly string[] baseFields =
        {
        "leftHandOnly",
        "cooldown",
        "maxCheckDistance",
        "handHoldDuration",
        "bodyHoldDuration"
    };

        // Foldout states
        private static bool showBase = false;
        private static bool showSubclass = false;


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            XRButton xRButton = (XRButton)target;





            string subclassName = target.GetType().Name;
            string prettyName = ObjectNames.NicifyVariableName(subclassName);

            showSubclass = EditorGUILayout.Foldout(showSubclass, prettyName + " Settings", true);


            if (showSubclass)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                SerializedProperty iterator = serializedObject.GetIterator();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    if (iterator.name == "m_Script") continue;

                    bool isBaseField = false;
                    foreach (string field in baseFields)
                    {
                        if (iterator.name == field)
                        {
                            isBaseField = true;
                            break;
                        }
                    }
                    if (isBaseField) continue;

                    EditorGUILayout.PropertyField(iterator, true);
                }

                GUILayout.Space(10);
            }
            GUILayout.Space(10);

            showBase = EditorGUILayout.Foldout(showBase, "XR Button Settings", true);

            if (showBase)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                foreach (string field in baseFields)
                {
                    SerializedProperty prop = serializedObject.FindProperty(field);
                    if (prop != null)
                        EditorGUILayout.PropertyField(prop, true);
                }

                GUILayout.Space(10);
            }


            GUILayout.Space(10);


            GUIStyle richText = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("<b>Tip:</b> Add layers 6 - Right, 7 - Left, 8 - Body", richText);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}