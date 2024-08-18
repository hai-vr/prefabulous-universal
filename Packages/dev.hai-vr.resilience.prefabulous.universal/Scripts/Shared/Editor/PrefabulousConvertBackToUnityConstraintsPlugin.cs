using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using Prefabulous.Universal.Common.Runtime;
using Prefabulous.Universal.Shared.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;

[assembly: ExportsPlugin(typeof(PrefabulousConvertBackToUnityConstraintsPlugin))]
namespace Prefabulous.Universal.Shared.Editor
{
    [CustomEditor(typeof(PrefabulousConvertBackToUnityConstraints))]
    public class PrefabulousConvertBackToUnityConstraintsEditor : UnityEditor.Editor {
        public override void OnInspectorGUI()
        {
#if PREFABULOUS_UNIVERSAL_VRCHAT_IS_INSTALLED
            EditorGUILayout.HelpBox("This is a VRChat project. You should not use this component, unless you're exporting an avatar for use in another Unity application, VTubing app, or social platform.", MessageType.Warning);
#endif
        }
    }
    
    public class PrefabulousConvertBackToUnityConstraintsPlugin : Plugin<PrefabulousConvertBackToUnityConstraintsPlugin>
    {
        public override string QualifiedName => "dev.hai-vr.prefabulous.universal.ConvertBackToUnityConstraints";
        public override string DisplayName => "Prefabulous Universal - Convert back to Unity Constraints";

        private readonly HashSet<string> _candidates = new HashSet<string>
        {
            "VRC.SDK3.Dynamics.Constraint.Components.VRCAimConstraint",
            "VRC.SDK3.Dynamics.Constraint.Components.VRCLookAtConstraint",
            "VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint",
            "VRC.SDK3.Dynamics.Constraint.Components.VRCPositionConstraint",
            "VRC.SDK3.Dynamics.Constraint.Components.VRCRotationConstraint",
            "VRC.SDK3.Dynamics.Constraint.Components.VRCScaleConstraint"
        };

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Transforming);
            
            seq.Run("Convert back to Unity Constraints", ConvertBackToUnityConstraints);
        }

        private void ConvertBackToUnityConstraints(BuildContext context)
        {
            var converts = context.AvatarRootTransform.GetComponentsInChildren<PrefabulousConvertBackToUnityConstraints>(true);
            if (converts.Length == 0) return;

            var foundConstraints = context.AvatarRootObject.GetComponentsInChildren<Component>(true)
                .Where(component => component != null) // Unloaded scripts may be null
                .Where(component => IsVrcConstraintOrConstraintStub(component.GetType()))
                .ToArray();

            if (foundConstraints.Length == 0) return;
            
            foreach (var fromConstraint in foundConstraints)
            {
                var fromSerialized = new SerializedObject(fromConstraint);
                var fromTargetTransformNullable = fromSerialized.FindProperty("TargetTransform").objectReferenceValue as Transform;

                var whereToAddItTo = fromTargetTransformNullable != null
                    ? fromTargetTransformNullable.gameObject
                    : fromConstraint.gameObject;
                var toConstraint = whereToAddItTo.AddComponent(ToType(fromConstraint.GetType()));
                
                var toSerialized = new SerializedObject(toConstraint);

                if (false)
                {
                    var toIterator = toSerialized.GetIterator();
                    bool enterChildren = true;
                    while (toIterator.Next(enterChildren))
                    {
                        enterChildren = true;
                        var path = toIterator.propertyPath;
                        Debug.Log("found:" + path);
                    }
                }

                // Found in Aim Constraint first
                CopyVerbatim(fromSerialized, toSerialized, "m_Enabled");
                Copy(fromSerialized, toSerialized, "GlobalWeight", "m_Weight");
                
                Copy(fromSerialized, toSerialized, "RotationAtRest", "m_RotationAtRest");
                Copy(fromSerialized, toSerialized, "RotationOffset", "m_RotationOffset");
                Copy(fromSerialized, toSerialized, "AimAxis", "m_AimVector");
                Copy(fromSerialized, toSerialized, "UpAxis", "m_UpVector");
                Copy(fromSerialized, toSerialized, "WorldUpVector", "m_WorldUpVector");
                Copy(fromSerialized, toSerialized, "WorldUpTransform", "m_WorldUpObject");
                Copy(fromSerialized, toSerialized, "WorldUp", "m_UpType");
                Copy(fromSerialized, toSerialized, "AffectsRotationX", "m_AffectRotationX");
                Copy(fromSerialized, toSerialized, "AffectsRotationY", "m_AffectRotationY");
                Copy(fromSerialized, toSerialized, "AffectsRotationZ", "m_AffectRotationZ");
                Copy(fromSerialized, toSerialized, "IsActive", "m_Active");
                Copy(fromSerialized, toSerialized, "Locked", "m_IsLocked");
                
                // Found in Look At Constraint first
                Copy(fromSerialized, toSerialized, "UseUpTransform", "m_UseUpObject");
                Copy(fromSerialized, toSerialized, "Roll", "m_Roll");
                
                // Found in Position Constraint first
                Copy(fromSerialized, toSerialized, "PositionAtRest", "m_TranslationAtRest");
                Copy(fromSerialized, toSerialized, "PositionOffset", "m_TranslationOffset");
                Copy(fromSerialized, toSerialized, "AffectsPositionX", "m_AffectTranslationX");
                Copy(fromSerialized, toSerialized, "AffectsPositionY", "m_AffectTranslationY");
                Copy(fromSerialized, toSerialized, "AffectsPositionZ", "m_AffectTranslationZ");
                
                // Found in Scale Constraint first
                Copy(fromSerialized, toSerialized, "ScaleAtRest", "m_ScaleAtRest");
                Copy(fromSerialized, toSerialized, "ScaleOffset", "m_ScaleOffset");
                Copy(fromSerialized, toSerialized, "AffectsScaleX", "m_AffectScalingX");
                Copy(fromSerialized, toSerialized, "AffectsScaleY", "m_AffectScalingY");
                Copy(fromSerialized, toSerialized, "AffectsScaleZ", "m_AffectScalingZ");
                
                // Found in all
                var fromSources = fromSerialized.FindProperty("Sources");
                var toSources = toSerialized.FindProperty("m_Sources");
                
                var deconstructedSources = DeconstructSources(fromSources);
                Debug.Log($"Deconstructed {deconstructedSources.Count} elts");

                toSources.arraySize = deconstructedSources.Count;
                for (var index = 0; index < deconstructedSources.Count; index++)
                {
                    var deconstructed = deconstructedSources[index];
                    
                    var toElement = toSources.GetArrayElementAtIndex(index);
                    toElement.FindPropertyRelative("sourceTransform").objectReferenceValue = deconstructed.transform;
                    toElement.FindPropertyRelative("weight").floatValue = deconstructed.weight;
                }
                
                // Found only in Parent Constraint
                if (toConstraint is ParentConstraint)
                {
                    var toTranslationOffsets = toSerialized.FindProperty("m_TranslationOffsets");
                    var toRotationOffsets = toSerialized.FindProperty("m_RotationOffsets");
                    toTranslationOffsets.arraySize = deconstructedSources.Count;
                    toRotationOffsets.arraySize = deconstructedSources.Count;
                    for (var index = 0; index < deconstructedSources.Count; index++)
                    {
                        var deconstructed = deconstructedSources[index];
                    
                        toTranslationOffsets.GetArrayElementAtIndex(index).vector3Value = deconstructed.parentPositionOffset;
                        toRotationOffsets.GetArrayElementAtIndex(index).vector3Value = deconstructed.parentRotationOffset;
                    }
                }

                toSerialized.ApplyModifiedPropertiesWithoutUndo();
            }
            
            foreach (var foundConstraint in foundConstraints)
            {
                Object.DestroyImmediate(foundConstraint);
            }
        }

        private List<DeconstructedSource> DeconstructSources(SerializedProperty fromSources)
        {
            var totalLength = fromSources.FindPropertyRelative("totalLength").intValue;
            var overflow = fromSources.FindPropertyRelative("overflowList");
            var overflowLength = overflow.arraySize;

            const int maxKeyable = 16;
            var deconstructedSources = new List<DeconstructedSource>();
            for (var i = 0; i < totalLength; i++)
            {
                if (i < maxKeyable)
                {
                    deconstructedSources.Add(Deconstruct(fromSources.FindPropertyRelative($"source{i}")));
                }
                else
                {
                    var overflowIndex = i - maxKeyable;
                    if (overflowIndex < overflowLength)
                    {
                        deconstructedSources.Add(Deconstruct(overflow.GetArrayElementAtIndex(overflowIndex)));
                    }
                }
            }

            return deconstructedSources;
        }

        private DeconstructedSource Deconstruct(SerializedProperty source)
        {
            return new DeconstructedSource
            {
                transform = source.FindPropertyRelative("SourceTransform").objectReferenceValue as Transform,
                weight = source.FindPropertyRelative("Weight").floatValue,
                parentPositionOffset = source.FindPropertyRelative("ParentPositionOffset").vector3Value,
                parentRotationOffset = source.FindPropertyRelative("ParentRotationOffset").vector3Value
            };
        }

        private struct DeconstructedSource
        {
            public Transform transform;
            public float weight;
            public Vector3 parentPositionOffset;
            public Vector3 parentRotationOffset;
        }

        private void CopyVerbatim(SerializedObject fromSerialized, SerializedObject toSerialized, string propertyPath)
        {
            Copy(fromSerialized, toSerialized, propertyPath, propertyPath);
        }

        private void Copy(SerializedObject fromSerialized, SerializedObject toSerialized, string fromPropertyPath, string toPropertyPath)
        {
            var fromProperty = fromSerialized.FindProperty(fromPropertyPath);
            var toProperty = toSerialized.FindProperty(toPropertyPath);
            if (toProperty == null)
            {
                return;
            }
            CopyProperty(fromProperty, toProperty);
        }

        private void CopyInsideElement(SerializedProperty fromElement, SerializedProperty toElement, string fromPropertyPath, string toPropertyPath)
        {
            var fromProperty = fromElement.FindPropertyRelative(fromPropertyPath);
            var toProperty = toElement.FindPropertyRelative(toPropertyPath);
            if (toProperty == null)
            {
                return;
            }
            CopyProperty(fromProperty, toProperty);
        }

        private static void CopyProperty(SerializedProperty fromProperty, SerializedProperty toProperty)
        {
            switch (toProperty.propertyType)
            {
                case SerializedPropertyType.Boolean: toProperty.boolValue = fromProperty.boolValue; break;
                case SerializedPropertyType.Float: toProperty.floatValue = fromProperty.floatValue; break;
                case SerializedPropertyType.Quaternion: toProperty.quaternionValue = fromProperty.quaternionValue; break;
                case SerializedPropertyType.Vector3: toProperty.vector3Value = fromProperty.vector3Value; break;
                case SerializedPropertyType.ObjectReference: toProperty.objectReferenceValue = fromProperty.objectReferenceValue; break;
                case SerializedPropertyType.Integer: toProperty.intValue = fromProperty.intValue; break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported: {toProperty.propertyType}");
            }
        }

        private Type ToType(Type getType)
        {
            switch (getType.FullName)
            {
                case "VRC.SDK3.Dynamics.Constraint.Components.VRCAimConstraint": return typeof(AimConstraint);
                case "VRC.SDK3.Dynamics.Constraint.Components.VRCLookAtConstraint": return typeof(LookAtConstraint);
                case "VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint": return typeof(ParentConstraint);
                case "VRC.SDK3.Dynamics.Constraint.Components.VRCPositionConstraint": return typeof(PositionConstraint);
                case "VRC.SDK3.Dynamics.Constraint.Components.VRCRotationConstraint": return typeof(RotationConstraint);
                case "VRC.SDK3.Dynamics.Constraint.Components.VRCScaleConstraint": return typeof(ScaleConstraint);
                default: throw new ArgumentException($"Unknown type {getType.FullName}");
            }
        }

        private bool IsVrcConstraintOrConstraintStub(Type type)
        {
            return _candidates.Contains(type.FullName);
        }
    }
}