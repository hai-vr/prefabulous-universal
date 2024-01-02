using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using Prefabulous.VRC.Runtime;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Prefabulous.VRC.Editor
{
    public static class PrefabulousUtil
    {
        public static string[] GetAllBlendshapeNames(SkinnedMeshRenderer smr)
        {
            if (smr.sharedMesh == null) return Array.Empty<string>();
            
            var sharedMesh = smr.sharedMesh;

            return Enumerable.Range(0, sharedMesh.blendShapeCount)
                .Select(i => sharedMesh.GetBlendShapeName(i))
                .ToArray();
        }

        public static AnimationClip[] FindAllRelevantAnimationClips(VRCAvatarDescriptor descriptor)
        {
            var isFxBlank = descriptor.transform.GetComponentsInChildren<PrefabulousBlankFXAnimator>(true).Length > 0;
            var isGestureBlank = descriptor.transform.GetComponentsInChildren<PrefabulousBlankGestureAnimator>(true).Length > 0;
            var replaceActionNullable = descriptor.transform.GetComponentInChildren<PrefabulousReplaceActionAnimator>();
            var replaceLocomotionNullable = descriptor.transform.GetComponentInChildren<PrefabulousReplaceLocomotionAnimator>();

            var runtimeAnimatorControllers = descriptor.baseAnimationLayers
                .Where(layer => !layer.isDefault)
                .Where(layer =>
                {
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.FX && isFxBlank) return false;
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.Gesture && isGestureBlank) return false;
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.Action && replaceActionNullable != null) return false;
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.Base && replaceLocomotionNullable != null) return false;

                    return true;
                })
                .Select(layer => layer.animatorController);

            var additionalControllers = new List<RuntimeAnimatorController>();
            if (replaceActionNullable != null) additionalControllers.Add(replaceActionNullable.controller);
            if (replaceLocomotionNullable != null) additionalControllers.Add(replaceLocomotionNullable.controller);
            
            return runtimeAnimatorControllers
                .Concat(descriptor.GetComponentsInChildren<ModularAvatarMergeAnimator>(true)
                    .Select(animator => animator.animator))
                .Concat(additionalControllers)
                .Where(controller => controller != null)
                .SelectMany(controller => controller.animationClips)
                .Distinct()
                .ToArray();
        }

        public static void BuildBlendshapeStruct(SkinnedMeshRenderer[] smrs, out List<SkinnedMeshRenderer> affected, out List<SkinnedMeshRenderer> notAffected, out Dictionary<SkinnedMeshRenderer, List<string>> smrToBlendshapes, out Dictionary<string, float> blendshapeToMaxval, string[] myBlendShapes, bool doNotHideBodyMesh)
        {
            affected = new List<SkinnedMeshRenderer>();
            notAffected = new List<SkinnedMeshRenderer>();
            smrToBlendshapes = new Dictionary<SkinnedMeshRenderer, List<string>>();
            blendshapeToMaxval = new Dictionary<string, float>();
            foreach (var smr in smrs)
            {
                if (smr == null || smr.sharedMesh == null) continue;

                var shapes = GetAllBlendshapeNames(smr);
                if (!doNotHideBodyMesh && shapes.Contains("vrc.v_aa")) continue;

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

                smrToBlendshapes[smr] = shapes.ToList();
                var isAffected = myBlendShapes.Any(s => shapes.Contains(s));
                if (isAffected)
                {
                    affected.Add(smr);
                }
                else
                {
                    notAffected.Add(smr);
                }
            }
        }

        public static SkinnedMeshRenderer[] FindSmrs(Transform transform, bool limitTo, SkinnedMeshRenderer[] renderers)
        {
            if (limitTo && renderers != null) return renderers;
            
            var animators = transform.GetComponentsInParent<Animator>(true);
            if (animators.Length == 0) return Array.Empty<SkinnedMeshRenderer>();
            
            var lastAnimator = animators.Last();
            return lastAnimator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }

        public static void ShowBlendshapeAssignments(string[] blendShapes, Dictionary<SkinnedMeshRenderer, List<string>> smrToBlendshapes)
        {
            foreach (var blendShape in blendShapes)
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
        }

        public static void ShowAddBlendshapes(SerializedObject serializedObject, string[] blendShapes,
            Dictionary<string, float> blendshapeToMaxval, string propertyName, bool showHidersAsRed,
            GUIStyle redStyleOptional, ref bool doNotHideBodyMesh)
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Add BlendShapes", EditorStyles.boldLabel);

                doNotHideBodyMesh = EditorGUILayout.Toggle(new GUIContent("Show face blendShapes"), doNotHideBodyMesh);

                var weightedBlendshapes = blendshapeToMaxval
                    .OrderByDescending(pair => pair.Value)
                    .ThenBy(pair => pair.Key)
                    .ToArray();

                foreach (var blendshapeToWeight in weightedBlendshapes)
                {
                    var blendShapeName = blendshapeToWeight.Key;

                    EditorGUILayout.BeginHorizontal();
                    var bslower = blendShapeName.ToLowerInvariant();
                    if (showHidersAsRed && (bslower.StartsWith("kisekae_")
                                            || bslower.StartsWith("shrink_")
                                            || bslower.StartsWith("hidemesh_")
                        ))
                    {
                        EditorGUILayout.TextField(blendShapeName, redStyleOptional);
                    }
                    else
                    {
                        EditorGUILayout.TextField(blendShapeName);
                    }

                    EditorGUILayout.TextField($"{(int)Mathf.Floor(blendshapeToWeight.Value)}", GUILayout.Width(50));
                    EditorGUI.BeginDisabledGroup(blendShapes.Contains(blendShapeName));
                    if (GUILayout.Button("+ Add", GUILayout.Width(50)))
                    {
                        var bsp = serializedObject.FindProperty(propertyName);
                        bsp.arraySize += 1;
                        bsp.GetArrayElementAtIndex(bsp.arraySize - 1).stringValue = blendShapeName;
                    }

                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}