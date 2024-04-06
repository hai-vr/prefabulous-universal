using System.Collections.Generic;
using Prefabulous.Native.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace Prefabulous.Native.Shared.Editor
{
    [CustomEditor(typeof(PrefabulousConvertBlendshapeConventions))]
    public class PrefabulousConvertBlendshapeConventionsEditor : UnityEditor.Editor
    {
        private GUIStyle _red;
        private const string UseTabAsSeparatorLabel = "Use TAB as separator";

        public override void OnInspectorGUI()
        {
            if (_red == null)
            {
                _red = new GUIStyle(EditorStyles.label);
                _red.normal.textColor = Color.red;
            }
            
            var my = (PrefabulousConvertBlendshapeConventions)target;
            if (my.renderers == null) my.renderers = new SkinnedMeshRenderer[0];
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousConvertBlendshapeConventions.limitToSpecificMeshes)));
            if (my.limitToSpecificMeshes)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousConvertBlendshapeConventions.renderers)));
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousConvertBlendshapeConventions.keyValueMapping)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousConvertBlendshapeConventions.keyValueSeparator)));
            if (GUILayout.Button(UseTabAsSeparatorLabel))
            {
                serializedObject.FindProperty(nameof(PrefabulousConvertBlendshapeConventions.keyValueSeparator)).stringValue = "\t";
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousConvertBlendshapeConventions.reverse)));
            serializedObject.ApplyModifiedProperties();
            
            var descriptor = PrefabulousUtil.GetAvatarRootOrNull(my.transform);
            if (descriptor == null) return;

            var blendshapes = new HashSet<string>();
            var smrs = my.limitToSpecificMeshes
                ? my.renderers
                : PrefabulousUtil.GetAllComponentsInChildrenExceptEditorOnly<SkinnedMeshRenderer>(descriptor);
            foreach (var smr in smrs)
            {
                if (smr != null)
                {
                    blendshapes.UnionWith(PrefabulousUtil.GetAllBlendshapeNames(smr));
                }
            }

            EditorGUILayout.LabelField("Blendshapes", EditorStyles.boldLabel);
            var mapping = my.ParseMapping();
            foreach (var keyValuePair in mapping)
            {
                EditorGUILayout.BeginHorizontal();
                if (blendshapes.Contains(keyValuePair.Key))
                {
                    EditorGUILayout.LabelField(keyValuePair.Key);
                }
                else
                {
                    EditorGUILayout.LabelField(keyValuePair.Key, _red);
                }
                EditorGUILayout.LabelField("\u2192", GUILayout.Width(30));
                EditorGUILayout.LabelField(keyValuePair.Value);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}