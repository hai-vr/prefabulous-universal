using Prefabulous.Universal.Common.Runtime;
using UnityEditor;

namespace Prefabulous.Universal.Shared.Editor
{
    [CustomEditor(typeof(PrefabulousEditAllMeshAnchorOverride))] [CanEditMultipleObjects] public class PrefabulousEditMeshAnchorOverrideEditor : PrefabulousEditor { }
    [CustomEditor(typeof(PrefabulousEditAllMeshBounds))] [CanEditMultipleObjects] public class PrefabulousEditMeshBoundsEditor : PrefabulousEditor { }
    
    public class PrefabulousEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // using (new UnityEditor.LocalizationGroup(target))
            // {
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
            // }
        }
    }
}