using System.Collections.Generic;
using System.Linq;
using Prefabulous.Universal.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace Prefabulous.Universal.Shared.Editor
{
    [CustomEditor(typeof(PrefabulousRecalculateNormals))]
    public class PrefabulousRecalculateNormalsEditor : UnityEditor.Editor
    {
        private static bool _doNotHideBodyMesh;
        private Dictionary<string, float> _animMaxvalsNullable;
        private GUIStyle _red;

        private void OnEnable()
        {
            _animMaxvalsNullable = null;
            
            var my = (PrefabulousRecalculateNormals)target;
            var descriptor = PrefabulousUtil.GetAvatarRootOrNull(my.transform);
            if (descriptor == null) return;

            var blendShapePropertyPrefix = "blendShape.";
            var lengthOfBlendShapePrefix = blendShapePropertyPrefix.Length;
            var blendShapeValues = PrefabulousUtil.FindAllRelevantAnimationClips(descriptor.transform)
                .SelectMany(clip =>
                {
                    return AnimationUtility.GetCurveBindings(clip)
                        .Where(binding => binding.type == typeof(SkinnedMeshRenderer))
                        .Where(binding => binding.propertyName.StartsWith(blendShapePropertyPrefix))
                        .Select(binding => new BlendShapeWeight(
                            binding.propertyName.Substring(lengthOfBlendShapePrefix),
                            AnimationUtility.GetEditorCurve(clip, binding).keys.Max(keyframe => keyframe.value)
                        ))
                        .Where(value => value.weight > 0f)
                        .ToArray();
                })
                .ToArray();
            
            var blendshapeToAnimMaxval = new Dictionary<string, float>();
            foreach (var blendShapeValue in blendShapeValues)
            {
                var blendShapeName = blendShapeValue.name;
                var weight = blendShapeValue.weight;
                if (blendshapeToAnimMaxval.TryGetValue(blendShapeName, out float existingWeight))
                {
                    if (weight > existingWeight)
                    {
                        blendshapeToAnimMaxval[blendShapeName] = weight;
                    }
                }
                else
                {
                    blendshapeToAnimMaxval[blendShapeName] = weight;
                }
            }

            _animMaxvalsNullable = blendshapeToAnimMaxval;
        }

        internal struct BlendShapeWeight
        {
            public BlendShapeWeight(string name, float weight)
            {
                this.name = name;
                this.weight = weight;
            }

            public string name;
            public float weight;
        }

        public override void OnInspectorGUI()
        {
            var localize = PrefabulousInit.localize;
            localize.RefreshIfNecessary();
            if (_red == null)
            {
                _red = new GUIStyle(EditorStyles.textField);
                _red.normal.textColor = Color.red;
            }

            var my = (PrefabulousRecalculateNormals)target;

            if (my.blendShapes == null) my.blendShapes = new string[0];
            if (my.renderers == null) my.renderers = new SkinnedMeshRenderer[0];
            
            localize.PropertyField(Phrases.recalculate_normals.blendshapes, serializedObject.FindProperty(nameof(PrefabulousRecalculateNormals.blendShapes)));
            localize.PropertyField(Phrases.recalculate_normals.limit_to_specific_meshes, serializedObject.FindProperty(nameof(PrefabulousRecalculateNormals.limitToSpecificMeshes)));
            if (my.limitToSpecificMeshes)
            {
                localize.PropertyField(Phrases.recalculate_normals.renderers, serializedObject.FindProperty(nameof(PrefabulousRecalculateNormals.renderers)));
            }

            EditorGUILayout.BeginHorizontal();
            localize.PropertyField(Phrases.recalculate_normals.erase_custom_split_normals, serializedObject.FindProperty(nameof(PrefabulousRecalculateNormals.eraseCustomSplitNormals)));
            if (GUILayout.Button("?", GUILayout.Width(50)))
            {
                Application.OpenURL("https://docs.hai-vr.dev/docs/products/prefabulous-avatar/hai-components/recalculate-normals#option-erase-custom-split-normals");
            }
            EditorGUILayout.EndHorizontal();
            
            if (my.eraseCustomSplitNormals)
            {
                localize.HelpBox(Phrases.recalculate_normals.msg_erase_custom_split_normals_is_enabled, MessageType.Warning);
                if (localize.Button(Phrases.recalculate_normals.open_documentation))
                {
                    Application.OpenURL("https://docs.hai-vr.dev/docs/products/prefabulous-avatar/hai-components/recalculate-normals#option-erase-custom-split-normals");
                }
            }
                    
            EditorGUILayout.Space();
            
            var smrs = PrefabulousUtil.FindSmrs(my.transform, my.limitToSpecificMeshes, my.renderers);
            PrefabulousUtil.BuildBlendshapeStruct(smrs, out var affected, out var notAffected, out var smrToBlendshapes, out var blendshapeToMaxval, my.blendShapes, _doNotHideBodyMesh);

            if (_animMaxvalsNullable != null)
            {
                foreach (var animMaxvals in _animMaxvalsNullable)
                {
                    if (blendshapeToMaxval.ContainsKey(animMaxvals.Key))
                    {
                        if (animMaxvals.Value > blendshapeToMaxval[animMaxvals.Key])
                        {
                            blendshapeToMaxval[animMaxvals.Key] = animMaxvals.Value;
                        }
                    }
                }
            }

            EditorGUILayout.Space();

            PrefabulousUtil.ShowBlendshapeAssignments(my.blendShapes, smrToBlendshapes);

            PrefabulousUtil.ShowAddBlendshapes(serializedObject, my.blendShapes, blendshapeToMaxval, nameof(PrefabulousRecalculateNormals.blendShapes), true, _red, ref _doNotHideBodyMesh);

            serializedObject.ApplyModifiedProperties();

            PrefabulousInit.LocalizeSelector();
        }
    }
}