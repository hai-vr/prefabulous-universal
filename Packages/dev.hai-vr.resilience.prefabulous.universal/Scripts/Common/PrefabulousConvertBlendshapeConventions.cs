using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if VRC_SDK_VRCSDK3
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Convert Blendshape Conventions")]
    public class PrefabulousConvertBlendshapeConventions : MonoBehaviour, IPrefabulousEditorOnly
    {
        public bool limitToSpecificMeshes;
        public SkinnedMeshRenderer[] renderers;

        [TextArea]
        public string keyValueMapping;
        public string keyValueSeparator = "=";
        public bool reverse;

        public Dictionary<string, string> ParseMapping()
        {
            var mapping = new Dictionary<string, string>();
            if (keyValueSeparator.Length != 1)
            {
                return mapping;
            }
            char separator = keyValueSeparator.ToCharArray()[0]; // Unity 2019 compat.
            
            var store = keyValueMapping ?? "";

            var lines = store.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s));
            foreach (var keyValue in lines)
            {
                var equalSplit = keyValue.Split(separator);
                if (equalSplit.Length == 2)
                {
                    var key = equalSplit[reverse ? 1 : 0];
                    var value = equalSplit[reverse ? 0 : 1];
                    mapping[key] = value;
                }
            }

            return mapping;
        }
    }
}