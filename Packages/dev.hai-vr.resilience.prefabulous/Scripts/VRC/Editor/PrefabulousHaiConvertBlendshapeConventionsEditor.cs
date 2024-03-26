using Prefabulous.Hai.Runtime;
using UnityEditor;
using UnityEngine;

namespace VRC.Editor
{
    [CustomEditor(typeof(PrefabulousHaiConvertBlendshapeConventions))]
    public class PrefabulousHaiConvertBlendshapeConventionsEditor : UnityEditor.Editor
    {
        private const string UseTabAsSeparatorLabel = "Use TAB as separator";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button(UseTabAsSeparatorLabel))
            {
                serializedObject.FindProperty(nameof(PrefabulousHaiConvertBlendshapeConventions.keyValueSeparator)).stringValue = "\t";
            }
            serializedObject.ApplyModifiedProperties();

            var my = (PrefabulousHaiConvertBlendshapeConventions)target;
            
            var mapping = my.ParseMapping();

            foreach (var keyValuePair in mapping)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(keyValuePair.Key);
                EditorGUILayout.LabelField("\u2192", GUILayout.Width(30));
                EditorGUILayout.LabelField(keyValuePair.Value);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}