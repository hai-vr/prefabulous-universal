using Prefabulous.Universal.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace Prefabulous.Universal.Shared.Editor
{
    [CustomEditor(typeof(PrefabulousAssignUVTile))]
    public class PrefabulousAssignUVTileEditor : UnityEditor.Editor
    {
        private static bool _doNotHideBodyMesh;

        public override void OnInspectorGUI()
        {
            var my = (PrefabulousAssignUVTile)target;

            if (my.blendShapes == null) my.blendShapes = new string[0];
            if (my.renderers == null) my.renderers = new SkinnedMeshRenderer[0];
            if (my.entireMeshes == null) my.entireMeshes = new Renderer[0];
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.mode)));

            if (my.mode == PrefabulousAssignUVTile.AssignMode.BlendShapes)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.blendShapes)), new GUIContent("BlendShapes"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.limitToSpecificMeshes)));
                if (my.limitToSpecificMeshes)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.renderers)));
                }
            
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.keepPartialPolygons)));
            }
            else if (my.mode == PrefabulousAssignUVTile.AssignMode.EntireMesh)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.entireMeshes)), new GUIContent("Meshes"));
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.uvChannel)), new GUIContent("UV Channel"));

            Row(my, 3);
            Row(my, 2);
            Row(my, 1);
            Row(my, 0);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.u)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.v)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.existingData)), new GUIContent("Existing UV Data"));
            
            if (my.mode == PrefabulousAssignUVTile.AssignMode.BlendShapes)
            {
                var smrs = PrefabulousUtil.FindSmrs(my.transform, my.limitToSpecificMeshes, my.renderers);
                PrefabulousUtil.BuildBlendshapeStruct(smrs, out var affected, out var notAffected, out var smrToBlendshapes, out var blendshapeToMaxval, my.blendShapes, _doNotHideBodyMesh);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                PrefabulousUtil.ShowBlendshapeAssignments(my.blendShapes, smrToBlendshapes);
                
                PrefabulousUtil.ShowAddBlendshapes(serializedObject, my.blendShapes, blendshapeToMaxval, nameof(PrefabulousAssignUVTile.blendShapes), false, null, ref _doNotHideBodyMesh);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void Row(PrefabulousAssignUVTile my, int vVal)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"v = {vVal}", GUILayout.Width(80));
            TriggerToggle(my, 0, vVal);
            TriggerToggle(my, 1, vVal);
            TriggerToggle(my, 2, vVal);
            TriggerToggle(my, 3, vVal);
            EditorGUILayout.EndHorizontal();
        }

        private void TriggerToggle(PrefabulousAssignUVTile my, int uVal, int vVal)
        {
            var currentVal = my.u == uVal && my.v == vVal;
            var newVal = EditorGUILayout.Toggle(currentVal);
            if (currentVal != newVal)
            {
                serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.u)).intValue = uVal;
                serializedObject.FindProperty(nameof(PrefabulousAssignUVTile.v)).intValue = vVal;
            }
        }
    }
}