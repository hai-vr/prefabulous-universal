using System;
using System.Collections.Generic;
using System.Linq;
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
                    
            EditorGUILayout.Space();
            
            var smrs = FindSmrs(my);

            var affected = new List<SkinnedMeshRenderer>();
            var notAffected = new List<SkinnedMeshRenderer>();
            var smrToBlendshapes = new Dictionary<SkinnedMeshRenderer, List<string>>();
            var blendshapeToMaxval = new Dictionary<string, float>();
            foreach (var smr in smrs)
            {
                if (smr == null || smr.sharedMesh == null) continue;

                var shapes = GetAllBlendshapesOf(smr);
                if (!_doNotHideBodyMesh && shapes.Contains("vrc.v_aa")) continue;

                for (var i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    var blendShapeName = shapes[i];
                    var weight = smr.GetBlendShapeWeight(i);
                    if (blendshapeToMaxval.TryGetValue(blendShapeName, out float existingWeight))
                    {
                        if (weight > existingWeight)
                        {
                            blendshapeToMaxval[blendShapeName] = weight;
                        }
                    }
                    else
                    {
                        blendshapeToMaxval[blendShapeName] = weight;
                    }
                }

                smrToBlendshapes[smr] = shapes;
                var isAffected = my.blendShapes.Any(s => shapes.Contains(s));
                if (isAffected)
                {
                    affected.Add(smr);
                }
                else
                {
                    notAffected.Add(smr);
                }
            }

            EditorGUILayout.Space();

            foreach (var blendShape in my.blendShapes)
            {
                EditorGUILayout.LabelField(blendShape, EditorStyles.boldLabel);
                var foundAny = false;
                foreach (var smrToBlendshape in smrToBlendshapes)
                {
                    if (smrToBlendshape.Value.Contains(blendShape))
                    {
                        foundAny = true;
                        EditorGUILayout.ObjectField(smrToBlendshape.Key, typeof(SkinnedMeshRenderer));
                    }
                }

                if (!foundAny)
                {
                    EditorGUILayout.LabelField("(No meshes found)", EditorStyles.label);
                }
            }

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Add BlendShapes", EditorStyles.boldLabel);

                _doNotHideBodyMesh = EditorGUILayout.Toggle(new GUIContent("Show face blendShapes"), _doNotHideBodyMesh);

                var weightedBlendshapes = blendshapeToMaxval
                    .OrderByDescending(pair => pair.Value)
                    .ThenBy(pair => pair.Key)
                    .ToArray();

                foreach (var blendshapeToWeight in weightedBlendshapes)
                {
                    var blendShapeName = blendshapeToWeight.Key;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(blendShapeName);
                    EditorGUILayout.TextField($"{(int)Mathf.Floor(blendshapeToWeight.Value)}", GUILayout.Width(50));
                    EditorGUI.BeginDisabledGroup(my.blendShapes.Contains(blendShapeName));
                    if (GUILayout.Button("+ Add", GUILayout.Width(50)))
                    {
                        var bsp = serializedObject.FindProperty(nameof(PrefabulousHaiDeletePolygons.blendShapes));
                        bsp.arraySize += 1;
                        bsp.GetArrayElementAtIndex(bsp.arraySize - 1).stringValue = blendShapeName;
                    }

                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static SkinnedMeshRenderer[] FindSmrs(PrefabulousHaiDeletePolygons my)
        {
            if (my.limitToSpecificMeshes && my.renderers != null) return my.renderers;
            
            var animators = my.transform.GetComponentsInParent<Animator>(true);
            if (animators.Length == 0) return Array.Empty<SkinnedMeshRenderer>();
            
            var lastAnimator = animators.Last();
            return lastAnimator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }

        private List<string> GetAllBlendshapesOf(SkinnedMeshRenderer smr)
        {
            var sharedMesh = smr.sharedMesh;

            return new List<string>(Enumerable.Range(0, sharedMesh.blendShapeCount)
                .Select(i => sharedMesh.GetBlendShapeName(i))
                .ToList());
        }
    }
}