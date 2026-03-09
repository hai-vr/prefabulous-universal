using System;
using System.Linq;
using HVR.EF.Loc;
using UnityEditor;
using UnityEngine;

namespace Prefabulous.Universal.Shared.Editor
{
    [InitializeOnLoad]
    public class PrefabulousInit
    {
        static PrefabulousInit()
        {
            EditorApplication.delayCall += Next;
        }
        
        public static HaiEFLoc localize
        {
            get
            {
                _localize ??= NewLoc();
                return _localize;
            }
        }
        private static HaiEFLoc _localize;
        private static HaiEFLoc NewLoc() => new("dev.hai-vr.resilience.prefabulous.universal", "Packages/dev.hai-vr.resilience.prefabulous.universal/Scripts/Shared/Editor/Locale");
        public static void LocalizeSelector() => localize.Selector(() => _localize = NewLoc());

        private static void Next()
        {
            // GizmoUtility.SetIconEnabled does not appear to exist in Unity 2021
#if UNITY_2022_1_OR_NEWER
            var allPrefabulousTypes = FindAllPrefabulousMonoTypes();
            foreach (var prefabulousType in allPrefabulousTypes)
            {
                GizmoUtility.SetIconEnabled(prefabulousType, false);
            }
#endif
        }

        private static Type[] FindAllPrefabulousMonoTypes()
        {
            var allPrefabulousTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(MonoBehaviour).IsAssignableFrom(type))
                .Where(type => type.Name.StartsWith("Prefabulous"))
                .ToArray();
            return allPrefabulousTypes;
        }
    }
}