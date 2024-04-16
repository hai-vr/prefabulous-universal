using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Prefabulous.Universal.Shared.Editor
{
    public static class PrefabulousUtil
    {
        private static Type _vrcAvatarDescriptorType;

        public static T[] GetAllComponentsInChildrenExceptEditorOnly<T>(Component descriptor) where T : Component
        {
            return ExcludeEditorOnly(descriptor.GetComponentsInChildren<T>(true));
        }

        private static T[] ExcludeEditorOnly<T>(T[] comps) where T : Component
        {
            return comps.Where(comp => !IsInEditorOnly(comp.transform)).ToArray();
        }

        private static bool IsInEditorOnly(Transform t)
        {
            if (t.CompareTag("EditorOnly")) return true;
            var parent = t.parent;
            return parent != null && IsInEditorOnly(parent);
        }
        
        public static void ExtractFromTRS(Matrix4x4 matrix, out Vector3 pos, out Quaternion rot, out Vector3 scale)
        {
            var c0 = matrix.GetColumn(0);
            var c1 = matrix.GetColumn(1);
            var c2 = matrix.GetColumn(2);
            var c3 = matrix.GetColumn(3);
            
            pos = (Vector3)c3;
            rot = Quaternion.LookRotation(c2, c1);
            scale = new Vector3(c0.magnitude, c1.magnitude, c2.magnitude);
        }
        
        public static string[] GetAllBlendshapeNames(SkinnedMeshRenderer smr)
        {
            if (smr.sharedMesh == null) return Array.Empty<string>();
            
            var sharedMesh = smr.sharedMesh;

            return Enumerable.Range(0, sharedMesh.blendShapeCount)
                .Select(i => sharedMesh.GetBlendShapeName(i))
                .ToArray();
        }

        public class InternalOnlyPrefabulousPlatform
        {
            public static IInternalOnlyPrefabulousPlatform Platform;
        }

        // FIXME: This is a stub interface to solve a dependency design issue, do not use other than internally
        public interface IInternalOnlyPrefabulousPlatform
        {
            AnimationClip[] FindAllRelevantAnimationClips(Transform descriptor);
        }

        public static AnimationClip[] FindAllRelevantAnimationClips(Transform descriptor)
        {
            if (InternalOnlyPrefabulousPlatform.Platform == null) return Array.Empty<AnimationClip>();
            
            return InternalOnlyPrefabulousPlatform.Platform.FindAllRelevantAnimationClips(descriptor);
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

        // Algorithm used for Delete Polygons, but it is also used in Assign UV Tile.
        // The output variable "verticesToDeleteOut" is used in Assign UV Tile for the vertices that will be assigned to that UV Tile.
        public static void FigureOutAffectedVertices(out bool[] verticesToDeleteOut, out bool[] partialVerticesOut,
            List<string> thatSmrBlendShapes, List<string> applicableBlendShapes,
            List<string> keepPartialBlendshapes, Mesh originalMesh)
        {
            var verticesToDelete = new bool[originalMesh.vertexCount];
            var partialVertices = new bool[originalMesh.vertexCount];
            
            var anyPartial = false;
            
            var verts = new Vector3[originalMesh.vertexCount];
            var norms = new Vector3[originalMesh.vertexCount];
            var tans = new Vector3[originalMesh.vertexCount];

            foreach (var blendShape in applicableBlendShapes)
            {
                var needsPartial = keepPartialBlendshapes.Contains(blendShape);
                var index = thatSmrBlendShapes.IndexOf(blendShape);
                for (var frame = 0; frame < originalMesh.GetBlendShapeFrameCount(index); frame++)
                {
                    originalMesh.GetBlendShapeFrameVertices(index, frame, verts, norms, tans);

                    for (var vertexIndex = 0; vertexIndex < verts.Length; vertexIndex++)
                    {
                        var vector3 = verts[vertexIndex];
                        if (vector3 != Vector3.zero)
                        {
                            verticesToDelete[vertexIndex] = true;
                            if (needsPartial)
                            {
                                partialVertices[vertexIndex] = true;
                                anyPartial = true;
                            }
                        }
                    }
                }
            }

            if (anyPartial)
            {
                var toUndelete = new bool[originalMesh.vertexCount];
                var triangles = originalMesh.triangles;
                var triangleCount = triangles.Length / 3;
                for (var triangle = 0; triangle < triangleCount; triangle++)
                {
                    var a = triangles[3 * triangle];
                    var b = triangles[3 * triangle + 1];
                    var c = triangles[3 * triangle + 2];

                    var atLeastOneIsPartial = partialVertices[a] || partialVertices[b] || partialVertices[c];
                    bool AnyIsNotDeleted() => !verticesToDelete[a] || !verticesToDelete[b] || !verticesToDelete[c];
                    if (atLeastOneIsPartial && AnyIsNotDeleted())
                    {
                        toUndelete[a] = true;
                        toUndelete[b] = true;
                        toUndelete[c] = true;
                    }
                }

                for (var index = 0; index < toUndelete.Length; index++)
                {
                    var shouldUndelete = toUndelete[index];
                    if (shouldUndelete)
                    {
                        verticesToDelete[index] = false;
                    }
                }
            }
            
            verticesToDeleteOut = verticesToDelete;
            partialVerticesOut = partialVertices;
        }

        public static Vector4[] GetUVsDefensively(Mesh mesh, int uvChannel)
        {
            var result = new List<Vector4>();
            mesh.GetUVs(uvChannel, result);

            if (result.Count != mesh.vertexCount)
            {
                return new Vector4[mesh.vertexCount];
            }

            return result.ToArray();
        }

        public static void DestroyAllAfterBake<T>(BuildContext context) where T : Component
        {
            foreach (var comp in context.AvatarRootTransform.GetComponentsInChildren<T>(true))
            {
                Object.DestroyImmediate(comp);
            }
        }

        public static Component GetAvatarRootOrNull(Transform child)
        {
#if VRC_SDK_VRCSDK3
            if (_vrcAvatarDescriptorType == null)
            {
                // This should not return null because we're in VRC_SDK_VRCSDK3 
                _vrcAvatarDescriptorType = NonCachedReflectiveGetTypeOrNull("VRCAvatarDescriptor");
            }
            
            var avatarDescriptorNullable = child.GetComponentInParent(_vrcAvatarDescriptorType, true);
            return avatarDescriptorNullable != null ? avatarDescriptorNullable : child.GetComponentInParent<Animator>(true);
#elif UNITY_2019
            return child.GetComponentInParent<Animator>();
#else
            return child.GetComponentInParent<Animator>(true);
#endif
        }

        public static Type NonCachedReflectiveGetTypeOrNull(string typeName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.Name == typeName);
        }
    }
}