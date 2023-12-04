using Prefabulous.Hai.Runtime;
using Prefabulous.VRC.Runtime;
using UnityEditor;
using UnityEditor.Localization.Editor;

namespace Prefabulous.VRC.Editor
{
    [CustomEditor(typeof(PrefabulousChangeAvatarScale))] [CanEditMultipleObjects] public class PrefabulousChangeAvatarScaleEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousEditMeshAnchorOverride))] [CanEditMultipleObjects] public class PrefabulousEditMeshAnchorOverrideEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousEditMeshBounds))] [CanEditMultipleObjects] public class PrefabulousEditMeshBoundsEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousHaiFaceTrackingExtensions))] [CanEditMultipleObjects] public class PrefabulousHaiFaceTrackingExtensionsEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousBlankExpressions))] [CanEditMultipleObjects] public class PrefabulousBlankExpressionsEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousBlankFXAnimator))] [CanEditMultipleObjects] public class PrefabulousBlankFXAnimatorEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousBlankGestureAnimator))] [CanEditMultipleObjects] public class PrefabulousBlankGestureAnimatorEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousImportExpressionParameters))] [CanEditMultipleObjects] public class PrefabulousImportExpressionParametersEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousReplaceActionAnimator))] [CanEditMultipleObjects] public class PrefabulousReplaceActionAnimatorEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousReplaceLocomotionAnimator))] [CanEditMultipleObjects] public class PrefabulousReplaceLocomotionAnimatorEditor : PrefabulousEditor { }
    
    
    public class PrefabulousEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            using (new LocalizationGroup(target))
            {
                EditorGUI.BeginChangeCheck();
                serializedObject.UpdateIfRequiredOrScript();
                var iterator = serializedObject.GetIterator();
                var count = 0;
                for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
                {
                    if (iterator.propertyPath != "m_Script")
                    {
                        count++;
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }

                if (count == 0)
                {
                    EditorGUILayout.LabelField("(This component has no properties)");
                }
                serializedObject.ApplyModifiedProperties();
                EditorGUI.EndChangeCheck();
            }
        }
    }
}