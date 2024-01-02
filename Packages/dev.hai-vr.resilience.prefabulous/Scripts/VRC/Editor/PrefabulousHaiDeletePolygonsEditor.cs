using Prefabulous.Hai.Runtime;
using UnityEditor;
using UnityEngine;

namespace Prefabulous.VRC.Editor
{
    [CustomEditor(typeof(PrefabulousHaiDeletePolygons))]
    public class PrefabulousHaiDeletePolygonsEditor : UnityEditor.Editor
    {
        private static bool _doNotHideBodyMesh;

        public override void OnInspectorGUI()
        {
            var my = (PrefabulousHaiDeletePolygons)target;

            if (my.blendShapes == null) my.blendShapes = new string[0];
            if (my.renderers == null) my.renderers = new SkinnedMeshRenderer[0];
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiDeletePolygons.blendShapes)), new GUIContent("BlendShapes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiDeletePolygons.limitToSpecificMeshes)));
            if (my.limitToSpecificMeshes)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiDeletePolygons.renderers)));
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiDeletePolygons.keepPartialPolygons)));
                    
            EditorGUILayout.Space();
            
            var smrs = PrefabulousUtil.FindSmrs(my.transform, my.limitToSpecificMeshes, my.renderers);
            PrefabulousUtil.BuildBlendshapeStruct(smrs, out var affected, out var notAffected, out var smrToBlendshapes, out var blendshapeToMaxval, my.blendShapes, _doNotHideBodyMesh);

            EditorGUILayout.Space();

            PrefabulousUtil.ShowBlendshapeAssignments(my.blendShapes, smrToBlendshapes);

            PrefabulousUtil.ShowAddBlendshapes(serializedObject, my.blendShapes, blendshapeToMaxval, nameof(PrefabulousHaiDeletePolygons.blendShapes), false, null, ref _doNotHideBodyMesh);

            serializedObject.ApplyModifiedProperties();
        }
    }
}