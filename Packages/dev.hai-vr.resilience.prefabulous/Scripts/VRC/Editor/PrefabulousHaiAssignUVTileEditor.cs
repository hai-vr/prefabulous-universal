using Prefabulous.Hai.Runtime;
using UnityEditor;
using UnityEngine;

namespace Prefabulous.VRC.Editor
{
    [CustomEditor(typeof(PrefabulousHaiAssignUVTile))]
    public class PrefabulousHaiAssignUVTileEditor : UnityEditor.Editor
    {
        private static bool _doNotHideBodyMesh;

        public override void OnInspectorGUI()
        {
            var my = (PrefabulousHaiAssignUVTile)target;

            if (my.blendShapes == null) my.blendShapes = new string[0];
            if (my.renderers == null) my.renderers = new SkinnedMeshRenderer[0];
            if (my.entireMeshes == null) my.entireMeshes = new Renderer[0];
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.mode)));

            if (my.mode == PrefabulousHaiAssignUVTile.AssignMode.BlendShapes)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.blendShapes)), new GUIContent("BlendShapes"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.limitToSpecificMeshes)));
                if (my.limitToSpecificMeshes)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.renderers)));
                }
            
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.keepPartialPolygons)));
            }
            else if (my.mode == PrefabulousHaiAssignUVTile.AssignMode.EntireMesh)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.entireMeshes)), new GUIContent("Meshes"));
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.uvChannel)), new GUIContent("UV Channel"));

            Row(my, 3);
            Row(my, 2);
            Row(my, 1);
            Row(my, 0);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.u)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.v)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.existingData)), new GUIContent("Existing UV Data"));
            
            if (my.mode == PrefabulousHaiAssignUVTile.AssignMode.BlendShapes)
            {
                var smrs = PrefabulousUtil.FindSmrs(my.transform, my.limitToSpecificMeshes, my.renderers);
                PrefabulousUtil.BuildBlendshapeStruct(smrs, out var affected, out var notAffected, out var smrToBlendshapes, out var blendshapeToMaxval, my.blendShapes, _doNotHideBodyMesh);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                PrefabulousUtil.ShowBlendshapeAssignments(my.blendShapes, smrToBlendshapes);
                
                PrefabulousUtil.ShowAddBlendshapes(serializedObject, my.blendShapes, blendshapeToMaxval, nameof(PrefabulousHaiAssignUVTile.blendShapes), false, null, ref _doNotHideBodyMesh);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void Row(PrefabulousHaiAssignUVTile my, int vVal)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"v = {vVal}", GUILayout.Width(80));
            TriggerToggle(my, 0, vVal);
            TriggerToggle(my, 1, vVal);
            TriggerToggle(my, 2, vVal);
            TriggerToggle(my, 3, vVal);
            EditorGUILayout.EndHorizontal();
        }

        private void TriggerToggle(PrefabulousHaiAssignUVTile my, int uVal, int vVal)
        {
            var currentVal = my.u == uVal && my.v == vVal;
            var newVal = EditorGUILayout.Toggle(currentVal);
            if (currentVal != newVal)
            {
                serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.u)).intValue = uVal;
                serializedObject.FindProperty(nameof(PrefabulousHaiAssignUVTile.v)).intValue = vVal;
            }
        }
    }
}