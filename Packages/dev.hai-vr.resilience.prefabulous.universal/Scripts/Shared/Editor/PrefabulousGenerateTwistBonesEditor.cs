using Prefabulous.Universal.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace Prefabulous.Universal.Shared.Editor
{
    [CustomEditor(typeof(PrefabulousGenerateTwistBones))]
    public class PrefabulousGenerateTwistBonesEditor : UnityEditor.Editor
    {
        private static bool _doNotHideBodyMesh;
        private GUIStyle _red;

        public override void OnInspectorGUI()
        {
            var localize = PrefabulousInit.localize;
            localize.RefreshIfNecessary();
            if (_red == null)
            {
                _red = new GUIStyle(EditorStyles.textField);
                _red.normal.textColor = Color.red;
            }

            var my = (PrefabulousGenerateTwistBones)target;
            
            localize.LabelField(Phrases.generate_twist_bones.basic_twist_bones, EditorStyles.boldLabel);
            localize.PropertyField(Phrases.generate_twist_bones.left_elbow_joint_lower_arm, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.leftElbowJointLowerArm)));
            localize.PropertyField(Phrases.generate_twist_bones.right_elbow_joint_lower_arm, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.rightElbowJointLowerArm)));
            
            EditorGUILayout.Space();
            
            localize.LabelField(Phrases.generate_twist_bones.options, EditorStyles.boldLabel);
            localize.PropertyField(Phrases.generate_twist_bones.weight_distribution, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.weightDistribution)));
            localize.PropertyField(Phrases.generate_twist_bones.exclude_bracelets_and_wristwatches_blendshapes, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.excludeBraceletsAndWristwatchesBlendshapes)));
            
            EditorGUILayout.Space();
            
            localize.LabelField(Phrases.generate_twist_bones.custom_twist_bone, EditorStyles.boldLabel);
            localize.PropertyField(Phrases.generate_twist_bones.use_custom, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.useCustom)));
            if (my.useCustom)
            {
                localize.PropertyField(Phrases.generate_twist_bones.upper_up_suggestion, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.upperUpSuggestion)));
                localize.PropertyField(Phrases.generate_twist_bones.lower_up_suggestion, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.lowerUpSuggestion)));
                localize.PropertyField(Phrases.generate_twist_bones.upper, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.upper)));
                localize.PropertyField(Phrases.generate_twist_bones.lower, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.lower)));
                localize.PropertyField(Phrases.generate_twist_bones.tip, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.tip)));
                localize.PropertyField(Phrases.generate_twist_bones.is_main_armature, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.isMainArmature)));
            }
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            localize.LabelField(Phrases.generate_twist_bones.experimental_compatibility_options, EditorStyles.boldLabel);
            if (GUILayout.Button("?", GUILayout.Width(50)))
            {
                Application.OpenURL("https://docs.hai-vr.dev/docs/products/prefabulous/universal/generate-twist-bones#experimental-compatibility-options");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.BeginDisabledGroup(my.generateInOptimizingPhase);
            localize.PropertyField(Phrases.generate_twist_bones.generate_before_modular_avatar_merge_armature, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.generateBeforeModularAvatarMergeArmature)));
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(my.generateBeforeModularAvatarMergeArmature);
            localize.PropertyField(Phrases.generate_twist_bones.generate_in_optimizing_phase, serializedObject.FindProperty(nameof(PrefabulousGenerateTwistBones.generateInOptimizingPhase)));
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
            
            PrefabulousInit.LocalizeSelector();
        }
    }
}